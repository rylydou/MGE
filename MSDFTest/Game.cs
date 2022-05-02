using MGE;

namespace MSDFTest;

public class Game : Module
{
	MSDFFont _font = App.content.Get<MSDFFont>("Regular.json");

	protected override void Startup()
	{
		App.window.onRender += Render;
	}

	protected override void Update(float delta)
	{
	}

	void Render(Window window)
	{
		var batch = Batch2D.current;

		_font.DrawString(batch, "Hello world", new(16, 16), 4);

		batch.Render(window);
	}
}
