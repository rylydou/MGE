using MGE;

namespace Demo;

public class Game : Module
{
	public readonly Batch2D batcher = new Batch2D();
	public float offset = 0f;

	// This is called when the Application has Started
	protected override void Startup()
	{
		// Add a Callback to the primary window's Render loop
		App.window.onRender += Render;
	}

	// This is called when the Application is shutting down, or when the Module is removed
	protected override void Shutdown()
	{
		// Remove our Callback
		App.window.onRender -= Render;
	}

	// This is called every frame of the Application
	protected override void Update()
	{
		offset += 32 * Time.delta;
	}

	void Render(Window window)
	{
		// clear the batcher from the previous frame
		batcher.Clear();

		// draw a rectangle
		batcher.Rect(offset, 0, 32, 32, Color.red);

		// clear the Window
		App.graphics.Clear(window, Color.black);

		// draw the batcher to the Window
		batcher.Render(window);
	}
}
