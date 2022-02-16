namespace MGE;

public abstract class Module
{
	public string name;

	protected internal readonly int priority;

	public int mainThreadId { get; internal set; }

	public bool isRegistered { get; internal set; }

	public bool isStarted { get; internal set; }

	protected Module(int priority = 10000)
	{
		name = GetType().Name;
		this.priority = priority;
	}

	/// <summary>
	/// Called when the Application begins, after the Primary Window is created.
	/// If the Application has already started when the module is Registered, this will be called immediately.
	/// </summary>
	protected internal virtual void Startup() { }

	/// <summary>
	/// Called when the Application shuts down, or the Module is Removed
	/// </summary>
	protected internal virtual void Shutdown() { }

	/// <summary>
	/// Called after the Shutdown method when the Module should be fully Disposed
	/// </summary>
	protected internal virtual void Disposed() { }

	/// <summary>
	/// Called at the start of the frame, before Update or Fixed Update.
	/// </summary>
	protected internal virtual void FrameStart() { }

	/// <summary>
	/// Called every fixed step
	/// </summary>
	protected internal virtual void FixedUpdate() { }

	/// <summary>
	/// Called every variable step
	/// </summary>
	protected internal virtual void Update() { }

	/// <summary>
	/// Called at the end of the frame, after Update and Fixed Update.
	/// </summary>
	protected internal virtual void FrameEnd() { }

	/// <summary>
	/// Called before any rendering
	/// </summary>
	protected internal virtual void BeforeRender() { }

	/// <summary>
	/// Called when a Window is being rendered to, before the Window.OnRender callback
	/// </summary>
	protected internal virtual void BeforeRenderWindow(Window window) { }

	/// <summary>
	/// Called when a Window is being rendered to, after the Window.OnRender callback
	/// </summary>
	protected internal virtual void AfterRenderWindow(Window window) { }

	/// <summary>
	/// Called after all rendering
	/// </summary>
	protected internal virtual void AfterRender() { }
}

public abstract class AppModule : Module
{
	protected AppModule(int priority = 10000) : base(priority) { }

	/// <summary>
	/// Called when Application is starting, before the Primary Window is created.
	/// </summary>
	protected internal virtual void ApplicationStarted() { }

	/// <summary>
	/// Called when the Module is created, before Startup but after the first Window is created
	/// </summary>
	protected internal virtual void FirstWindowCreated() { }

}
