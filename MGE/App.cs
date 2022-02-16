using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MGE;

public static class App
{
	/// <summary>
	/// The Application Name
	/// </summary>
	public static string name = "";

	/// <summary>
	/// MGE Version Number
	/// </summary>
	public static readonly Version version = new Version(0, 1, 0);

	/// <summary>
	/// Whether the Application is running
	/// </summary>
	public static bool running { get; private set; } = false;

	/// <summary>
	/// Whether the Application is exiting
	/// </summary>
	public static bool exiting { get; private set; } = false;

	/// <summary>
	/// A List of all the Application Modules
	/// </summary>
	public static readonly ModuleList modules = new ModuleList();

	/// <summary>
	/// Gets the System Module
	/// </summary>
	public static System system => modules.Get<System>();

	/// <summary>
	/// Gets the Graphics Module
	/// </summary>
	public static Graphics graphics => modules.Get<Graphics>();

	/// <summary>
	/// Gets the Audio Module
	/// </summary>
	// public static Audio audio => modules.Get<Audio>();

	/// <summary>
	/// Gets the System Input
	/// </summary>
	// public static Input input => system.Input;

	/// <summary>
	/// Gets the Primary Window
	/// </summary>
	public static Window window => primaryWindow ?? throw new Exception("Application has not yet created a Primary Window");

	/// <summary>
	/// When set to true, this forces the entire application to use Fixed Timestep, including normal Update methods.
	/// </summary>
	public static bool forceFixedTimestep;

	/// <summary>
	/// Called when the App is told to exit (ex. SDL2 calls this when the X button on the last window is clicked). By default, it simply exits.
	/// </summary>
	public static Action? onExitRequest = Exit;

	/// <summary>
	/// Reference to the Primary Window
	/// </summary>
	private static Window? primaryWindow;

	/// <summary>
	/// Starts running the Application
	/// You must register the System Module before calling this
	/// </summary>
	public static void Start(string title, int width, int height, WindowFlags flags = WindowFlags.ScaleToMonitor, Action? callback = null)
	{
		if (running)
			throw new Exception("App is already running");

		if (exiting)
			throw new Exception("App is still exiting");

		if (string.IsNullOrWhiteSpace(name))
			name = title;

		Log.Info($"Version: {version}");
		Log.Info($"Platform: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
		Log.Info($"MGE: {RuntimeInformation.FrameworkDescription}");

#if DEBUG
		Launch();
#else
            try
            {
                Launch();
            }
            catch (Exception e)
            {
                var path = System.DefaultUserDirectory(Name);
                if (Modules.Has<System>())
                    path = Modules.Get<System>().UserDirectory(Name);

                Log.Error(e.Message);
                // TODO - this call was broken with the logging updates!
                //Log.AppendToFile(Name, Path.Combine(path, "ErrorLog.txt"));
                throw;
            }
#endif
		void Launch()
		{
			// init modules
			modules.ApplicationStarted();

			if (!modules.Has<System>())
				throw new Exception("App requires a System Module to be registered before it can Start");

			// our primary Window
			primaryWindow = new Window(system, title, width, height, flags);
			modules.FirstWindowCreated();

			// startup application
			running = true;
			modules.Startup();
			callback?.Invoke();
			Run();

		}
	}

	private static void Run()
	{
		// timer
		var timer = Stopwatch.StartNew();
		var lastTime = new TimeSpan();
		var fixedAccumulator = TimeSpan.Zero;
		var framecount = 0;
		var frameticks = 0L;

		while (running)
		{
			var forceFixedTimestep = App.forceFixedTimestep;
			if (!forceFixedTimestep)
				system.input.Step();

			modules.FrameStart();

			// get current time & diff
			var currTime = TimeSpan.FromTicks(timer.Elapsed.Ticks);
			var diffTime = (currTime - lastTime);
			lastTime = currTime;

			// fixed timestep update
			{
				// fixed delta time is always the same
				var fixedTarget = TimeSpan.FromSeconds(1f / Time.fixedStepTarget);
				Time.rawDelta = Time.rawFixedDelta = (float)fixedTarget.TotalSeconds;
				Time.delta = Time.fixedDelta = Time.rawFixedDelta * Time.DeltaScale;

				if (forceFixedTimestep)
				{
					Time.rawVariableDelta = Time.rawFixedDelta;
					Time.variableDelta = Time.fixedDelta;
				}

				// increment time
				fixedAccumulator += diffTime;

				// if we're forcing fixed timestep and running too fast,
				// we should sleep the thread while we wait
				if (forceFixedTimestep)
				{
					while (fixedAccumulator < fixedTarget)
					{
						Thread.Sleep((int)((fixedTarget - fixedAccumulator).TotalMilliseconds));

						currTime = TimeSpan.FromTicks(timer.Elapsed.Ticks);
						diffTime = (currTime - lastTime);
						lastTime = currTime;
						fixedAccumulator += diffTime;
					}
				}

				// Do not allow any update to take longer than our maximum.
				if (fixedAccumulator > Time.fixedMaxElapsedTime)
					fixedAccumulator = Time.fixedMaxElapsedTime;

				// do as many fixed updates as we can
				while (fixedAccumulator >= fixedTarget)
				{
					Time.fixedDuration += fixedTarget;
					fixedAccumulator -= fixedTarget;

					if (forceFixedTimestep)
					{
						Time.duration += fixedTarget;

						system.input.Step();
						modules.FixedUpdate();
						modules.Update();
					}
					else
					{
						modules.FixedUpdate();
					}

					if (exiting)
						break;
				}
			}

			// variable timestep update
			if (!forceFixedTimestep)
			{
				// get Time values
				Time.duration += diffTime;
				Time.rawDelta = Time.rawVariableDelta = (float)diffTime.TotalSeconds;
				Time.delta = Time.variableDelta = Time.rawDelta * Time.DeltaScale;

				// update
				modules.Update();
			}

			modules.FrameEnd();

			// Check if the Primary Window has been closed
			if (primaryWindow == null || !primaryWindow.opened)
				Exit();

			// render
			if (!exiting)
			{
				modules.BeforeRender();

				for (int i = 0; i < system.windows.Count; i++)
					if (system.windows[i].opened)
						system.windows[i].Render();

				for (int i = 0; i < system.windows.Count; i++)
					if (system.windows[i].opened)
						system.windows[i].Present();

				modules.AfterRender();
			}

			// get fps
			framecount++;
			if (TimeSpan.FromTicks(timer.Elapsed.Ticks - frameticks).TotalSeconds >= 1)
			{
				Time.fps = framecount;
				frameticks = timer.Elapsed.Ticks;
				framecount = 0;
			}
		}

		// finalize
		modules.Shutdown();
		primaryWindow = null;
		exiting = false;

		Log.Info("Exited");
	}

	/// <summary>
	/// Begins Exiting the Application. This will not exit the application immediately and will finish the current Update.
	/// </summary>
	public static void Exit()
	{
		if (running && !exiting)
		{
			running = false;
			exiting = true;
		}
	}
}
