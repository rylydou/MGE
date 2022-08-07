namespace Game;

public class MainMenuScreen : GameScreen
{
	int _index;
	int _max = 30;

	public override void Start()
	{
		App.input.SetMouseCursorMode(CursorModes.Normal);
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		var kb = App.input.keyboard;
		var con = Main.controls[2];

		if (kb.Released(Keys.E))
		{
			Main.ChangeScreen(new EditorScreen());
			return;
		}

		if (kb.Released(Keys.S) || con.pause.pressed)
		{
			Main.ChangeScreen(new SetupScreen());
			return;
		}

		if (con.navigateRight.repeated)
			_index++;
		if (con.navigateLeft.repeated)
			_index--;

		_index = Mathf.Clamp(_index, 0, _max - 1);
	}

	public override void Render(Batch2D batch)
	{
		var time = (float)Time.duration.TotalSeconds;
		var color = new Color(
			Time.SineWave(0, 1, 10, 0f / 3),
			Time.SineWave(0, 1, 10, 1f / 3),
			Time.SineWave(0, 1, 10, 2f / 3),
		1f);

		batch.SetBox(new(Main.screenSize), new(0x302C2EFF));

		var text =
			"Unnamed Party Game\n" +
			"\n";
		batch.DrawString(App.content.font, text, new(0, 0, Main.screenSize.x, Main.screenSize.y), TextAlignment.Center, color, 24);

		text =
			"\n" +
			"S - Start\n" +
			"E - Editor";
		batch.DrawString(App.content.font, text, new(0, 0, Main.screenSize.x, Main.screenSize.y), TextAlignment.Center, new(0xcfc6b8FF), 24);

		text = "(press [Shift+Escape] to come back here)";
		batch.DrawString(App.content.font, text, new(0, Main.screenSize.y - 16, Main.screenSize.x, 16), TextAlignment.Center, new(0x5a5353FF), 8);

		var s = 10;
		for (int i = 0; i < _max; i++)
		{
			var rect = new Rect(i * (s * 2) + s, s, s, s);
			batch.SetBox(rect, Color.gray);

			if (i == _index)
			{
				batch.SetRect(rect.Expanded(1), 1, Color.cyan);
			}
		}
	}
}
