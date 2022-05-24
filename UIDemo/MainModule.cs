using MGE.UI;

namespace UIDemo;

public class MainModule : Module
{
	UICanvas canvas = new();

	protected override void Startup()
	{
		App.window.onRender += Render;
	}

	protected override void Update(float delta)
	{
		canvas.fixedSize = App.window.size;
	}

	void Render(Window window)
	{
		var batch = new Batch2D();

		canvas.RenderCanvas(batch);
	}
}
