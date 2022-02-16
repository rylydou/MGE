using MGE;

namespace Demo;

public class Game : Module
{
	public readonly Batch2D Batcher = new Batch2D();
	public float Offset = 0f;

	// This is called when the Application has Started
	protected override void Startup()
	{
		// Add a Callback to the primary window's Render loop
		App.Window.OnRender += Render;
	}

	// This is called when the Application is shutting down, or when the Module is removed
	protected override void Shutdown()
	{
		// Remove our Callback
		App.Window.OnRender -= Render;
	}

	// This is called every frame of the Application
	protected override void Update()
	{
		Offset += 32 * Time.DeltaTime;
	}

	private void Render(Window window)
	{
		// clear the batcher from the previous frame
		Batcher.Clear();

		// draw a rectangle
		Batcher.Rect(Offset, 0, 32, 32, Color.Red);

		// clear the Window
		App.Graphics.Clear(window, Color.Black);

		// draw the batcher to the Window
		Batcher.Render(window);
	}
}
