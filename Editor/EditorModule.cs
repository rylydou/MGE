using MGE.UI;

namespace MGE.Editor;

public class EditorModule : Module
{
	public readonly Batch2D batch = new();

#pragma warning disable CS8618
	Font _font;
#pragma warning restore CS8618

	UICanvas _canvas = new() { direction = UIDirection.Vertical };

	// This is called when the Application has Started
	protected override void Startup()
	{
		// Add a Callback to the primary window's Render loop
		App.window.onRender += Render;

		// Load Content
		_font = App.content.Get<Font>("Fonts/Normal/Regular.ttf");

		App.window.Center();

		_canvas.AddChild(new UIButton()
		{
			resizing = new(UIResizing.FillContainer, UIResizing.FillContainer),
		});
		_canvas.AddChild(new UIButton()
		{
			fixedSize = new(90, 30),
		});
		_canvas.AddChild(new UIButton()
		{
			resizing = new(UIResizing.FillContainer, UIResizing.Fixed),
			fixedSize = new(90, 30),
		});
	}

	// This is called when the Application is shutting down, or when the Module is removed
	protected override void Shutdown()
	{
		// Remove our Callback
		App.window.onRender -= Render;
	}

	// This is called every frame of the Application
	protected override void Update(float delta)
	{
		_canvas.fixedSize = App.window.size;
	}

	void Render(Window window)
	{
		// Clear the batcher from the previous frame
		batch.Clear();

		// Draw UI
		_canvas.Draw(batch);

		// Clear the Window
		App.graphics.Clear(window, new Color(22, 22, 22, 255));

		// Draw the batcher to the Window
		batch.Render(window);
	}
}
