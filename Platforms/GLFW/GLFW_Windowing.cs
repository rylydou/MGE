using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MGE.GLFW
{
	public class GLFW_System : Windowing, ISystemOpenGL, ISystemVulkan
	{
		readonly GLFW_Input _input;

		readonly List<IntPtr> _windowPointers = new List<IntPtr>();
		readonly Dictionary<IntPtr, GLFW_Window> _glfwWindows = new Dictionary<IntPtr, GLFW_Window>();
		readonly Dictionary<IntPtr, GLFW_GLContext> _glContexts = new Dictionary<IntPtr, GLFW_GLContext>();
		readonly Dictionary<IntPtr, IntPtr> _vkSurfaces = new Dictionary<IntPtr, IntPtr>();

		public override bool supportsMultipleWindows => true;
		public override Input input => _input;

		public GLFW_System()
		{
			_input = new GLFW_Input();

			GLFW.GetVersion(out int major, out int minor, out int rev);

			apiName = "GLFW";
			apiVersion = new Version(major, minor, rev);
		}

		protected override void ApplicationStarted()
		{
			if (GLFW.Init() == 0)
			{
				GLFW.GetError(out string error);
				throw new MGException($"GLFW Error: {error}");
			}

			// OpenGL Setup
			if (App.graphics is IGraphicsOpenGL)
			{
				// macOS requires versions to be set to 3.2
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					GLFW.WindowHint(GLFW_Enum.CONTEXT_VERSION_MAJOR, 3);
					GLFW.WindowHint(GLFW_Enum.CONTEXT_VERSION_MINOR, 2);
					GLFW.WindowHint(GLFW_Enum.OPENGL_PROFILE, 0x00032001);
					GLFW.WindowHint(GLFW_Enum.OPENGL_FORWARD_COMPAT, true);
				}
			}
			else
			{
				GLFW.WindowHint(GLFW_Enum.CLIENT_API, (int)GLFW_Enum.NO_API);
			}

			// Various constant Window Hints
			GLFW.WindowHint(GLFW_Enum.DOUBLEBUFFER, true);
			GLFW.WindowHint(GLFW_Enum.DEPTH_BITS, 24);
			GLFW.WindowHint(GLFW_Enum.STENCIL_BITS, 8);

			// Monitors
			unsafe
			{
				var monitorPtrs = GLFW.GetMonitors(out int count);
				for (int i = 0; i < count; i++)
					_monitors.Add(new GLFW_Monitor(monitorPtrs[i]));
			}

			// Init Input
			_input.Init();
		}

		protected override void Shutdown()
		{
			_input.Dispose();

			foreach (var window in _windowPointers)
				GLFW.SetWindowShouldClose(window, true);

			Poll(); // this will actually close the Windows
		}

		protected override void Disposed()
		{
			GLFW.Terminate();
		}

		protected override void FrameStart()
		{
			_input.BeforeUpdate();
		}

		protected override void FrameEnd()
		{
			Poll();

			// Update Monitors
			foreach (GLFW_Monitor monitor in monitors)
				monitor.FetchProperties();

			// update input
			_input.AfterUpdate();
		}

		void Poll()
		{
			GLFW.PollEvents();

			// check for closing windows
			for (int i = _windowPointers.Count - 1; i >= 0; i--)
			{
				if (GLFW.WindowShouldClose(_windowPointers[i]))
				{
					// see if we have a GLFW_Window associated
					if (_glfwWindows.TryGetValue(_windowPointers[i], out var window))
					{
						_input.StopListening(window._pointer);
						_glfwWindows.Remove(window._pointer);
						window.InvokeCloseWindowCallback();
					}

					// remove OpenGL context
					if (App.graphics is IGraphicsOpenGL)
					{
						_glContexts.Remove(_windowPointers[i]);
					}
					// remove Vulkan Surface
					else if (App.graphics is IGraphicsVulkan vkGraphics)
					{
						var vkInstance = vkGraphics.GetVulkanInstancePointer();

						if (vkDestroySurfaceKHR is null)
						{
							var ptr = GetVKProcAddress(vkInstance, "vkDestroySurfaceKHR");
							if (ptr != IntPtr.Zero)
								vkDestroySurfaceKHR = (VkDestroySurfaceKHR)Marshal.GetDelegateForFunctionPointer(ptr, typeof(VkDestroySurfaceKHR));
						}

						vkDestroySurfaceKHR?.Invoke(vkInstance, _vkSurfaces[_windowPointers[i]], IntPtr.Zero);
						_vkSurfaces.Remove(_windowPointers[i]);
					}

					GLFW.DestroyWindow(_windowPointers[i]);
					_windowPointers.RemoveAt(i);
				}
			}
		}

		protected override Window.Platform CreateWindow(string title, int width, int height, WindowFlags flags = WindowFlags.None)
		{
			if (Environment.CurrentManagedThreadId != mainThreadId) throw new MGException("Creating a Window must be called from the Main Thread");

			// create GLFW Window
			var ptr = CreateGlfwWindow(title, width, height, flags);

			// start listening for input on this Window
			_input.StartListening(ptr);

			// Add the GL Context
			if (App.graphics is IGraphicsOpenGL)
			{
				_glContexts.Add(ptr, new GLFW_GLContext(ptr));
			}

			// create the actual Window object
			var window = new GLFW_Window(this, ptr, title, !flags.HasFlag(WindowFlags.Hidden));
			_glfwWindows.Add(ptr, window);
			return window;
		}

		internal IntPtr CreateGlfwWindow(string title, int width, int height, WindowFlags flags)
		{
			GLFW.WindowHint(GLFW_Enum.VISIBLE, false);
			GLFW.WindowHint(GLFW_Enum.FOCUS_ON_SHOW, true);
			GLFW.WindowHint(GLFW_Enum.FOCUSED, true);
			GLFW.WindowHint(GLFW_Enum.TRANSPARENT_FRAMEBUFFER, flags.HasFlag(WindowFlags.Transparent));
			GLFW.WindowHint(GLFW_Enum.SCALE_TO_MONITOR, flags.HasFlag(WindowFlags.ScaleToMonitor));
			GLFW.WindowHint(GLFW_Enum.SAMPLES, flags.HasFlag(WindowFlags.MultiSampling) ? 4 : 0);
			GLFW.WindowHint(GLFW_Enum.MAXIMIZED, flags.HasFlag(WindowFlags.Maximized));

			var shared = IntPtr.Zero;
			if (App.graphics is IGraphicsOpenGL && _windowPointers.Count > 0)
				shared = _windowPointers[0];

			var monitor = IntPtr.Zero;
			if (flags.HasFlag(WindowFlags.Fullscreen))
				monitor = GLFW.GetPrimaryMonitor();

			// create the GLFW Window and return thr pointer
			var ptr = GLFW.CreateWindow(width, height, title, monitor, shared);
			if (ptr == IntPtr.Zero)
			{
				GLFW.GetError(out string error);
				throw new MGException($"Unable to create a new Window: {error}");
			}
			_windowPointers.Add(ptr);

			// create the Vulkan surface
			if (App.graphics is IGraphicsVulkan vkGraphics)
			{
				var vkInstance = vkGraphics.GetVulkanInstancePointer();
				var result = GLFW.CreateWindowSurface(vkInstance, ptr, IntPtr.Zero, out var surface);

				if (result != GLFW_VkResult.Success) throw new MGException($"Unable to create a Vulkan Surface, {result}");

				_vkSurfaces.Add(ptr, surface);
			}

			// show window
			if (!flags.HasFlag(WindowFlags.Hidden))
				GLFW.ShowWindow(ptr);

			return ptr;
		}

		#region ISystemOpenGL Method Calls

		public IntPtr GetGLProcAddress(string name)
		{
			return GLFW.GetProcAddress(name);
		}

		public ISystemOpenGL.Context CreateGLContext()
		{
			// GLFW has no way to create a context without a window ...
			// so we create a Window and just hide it

			var ptr = CreateGlfwWindow("hidden-context", 128, 128, WindowFlags.Hidden);
			var context = new GLFW_GLContext(ptr);
			_glContexts.Add(ptr, context);
			return context;
		}

		public ISystemOpenGL.Context GetWindowGLContext(Window window)
		{
			return _glContexts[((GLFW_Window)window.implementation)._pointer];
		}

		public ISystemOpenGL.Context? GetCurrentGLContext()
		{
			var ptr = GLFW.GetCurrentContext();

			if (ptr != IntPtr.Zero)
				return _glContexts[ptr];

			return null;
		}

		public void SetCurrentGLContext(ISystemOpenGL.Context? context)
		{
			if (context is GLFW_GLContext ctx && ctx is not null)
				GLFW.MakeContextCurrent(ctx.window);
			else
				GLFW.MakeContextCurrent(IntPtr.Zero);
		}

		internal void SetCurrentGLContext(IntPtr window)
		{
			GLFW.MakeContextCurrent(window);
		}

		#endregion

		#region ISystemVulkan Method Calls

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		unsafe delegate int VkDestroySurfaceKHR(IntPtr instance, IntPtr surface, IntPtr allocator);
		VkDestroySurfaceKHR? vkDestroySurfaceKHR;

		public IntPtr GetVKProcAddress(IntPtr instance, string name)
		{
			return GLFW.GetInstanceProcAddress(instance, name);
		}

		public IntPtr GetVKSurface(Window window)
		{
			return _vkSurfaces[((GLFW_Window)window.implementation)._pointer];
		}

		public List<string> GetVKExtensions()
		{
			unsafe
			{
				var ptr = (byte**)GLFW.GetRequiredInstanceExtensions(out uint count);
				var list = new List<string>();

				for (int i = 0; i < count; i++)
				{
					var str = Marshal.PtrToStringAnsi(new IntPtr(ptr[i]));
					if (str is not null)
						list.Add(str);
				}

				return new List<string>(list);
			}
		}

		#endregion
	}

}
