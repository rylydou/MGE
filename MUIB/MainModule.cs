using MGE.UI;

namespace UIDemo;

public class MainModule : Module
{
	UICanvas canvas = new()
	{
		enableDebug = true,
		direction = UIFlow.Vertical,
		spacing = 6,
		padding = new(12),
	};

	public MainModule()
	{
	}

	protected override void Startup()
	{
		App.window.onRender += Render;
	}

	protected override void Update(float delta)
	{
		canvas.UpdateInputs(App.window.mouse, App.input.mouse, App.input.keyboard);
		canvas.Update(delta);
	}

	void Render(Window window)
	{
		var batch = new Batch2D();

		canvas.RenderCanvas(batch);

		// FF9B7B 1b53bd
		App.graphics.Clear(window, new Color(0x202020FF));
		var renderInfo = batch.Render(window);
	}
}
