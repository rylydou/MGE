using System.Collections.Generic;
using MGE;

namespace MSDFTest;

public class Game : Module
{
	MSDFFont _msdfFont = App.content.Get<MSDFFont>("Regular.msdf");

	List<char> text = new();

	protected override void Startup()
	{
		App.window.onRender += Render;
	}

	protected override void Update(float delta)
	{
		if (App.input.keyboard.Repeated(Keys.Backspace))
		{
			if (text.Count > 0)
				text.RemoveAt(text.Count - 1);
		}

		foreach (var ch in App.input.keyboard.text.ToString())
		{
			text.Add(ch);
		}
	}

	void Render(Window window)
	{
		var batch = Batch2D.current;

		_msdfFont.DrawString(batch, text, new(16, 16), Color.white, 64);
		_msdfFont.DrawString(batch, text, new(16, 16 + 64 + 16), Color.white, 32);
		_msdfFont.DrawString(batch, text, new(16, 16 + 64 + 16 + 32 + 16), Color.white, 16);
		_msdfFont.DrawString(batch, text, new(16, 16 + 64 + 16 + 32 + 16 + 16 + 16), Color.white, 256);

		App.graphics.Clear(window, Color.darkGray);
		batch.Render(window);
	}
}
