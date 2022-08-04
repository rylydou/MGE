namespace Game.Screens;

public class MainMenuScreen : GameScreen
{
	public override void Start()
	{
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		var kb = App.input.keyboard;

		if (kb.Released(Keys.E))
		{
			Main.ChangeScreen(new EditorScreen());
			return;
		}

		if (kb.Released(Keys.S))
		{
			Main.ChangeScreen(new SetupScreen());
			return;
		}
	}

	public override void Render(Batch2D batch)
	{
		batch.SetBox(new(Main.screenSize), new(0x302C2EFF));

		var time = (float)Time.duration.TotalSeconds;
		var color = new Color(
			Math.Abs(Mathf.Sin(time)),
			Math.Abs(Mathf.Sin(time + Mathf.TAU / 3)),
			Math.Abs(Mathf.Sin(time + Mathf.TAU * 2 / 3)),
		1f);

		var text =
			"Unnamed Party Game\n" +
			"\n";
		batch.DrawString(App.content.font, text, new(0, 0, Main.screenSize.x, Main.screenSize.y), TextAlignment.Center, color, 24);

		text =
			"\n" +
			"S - Start\n" +
			"E - Editor";
		batch.DrawString(App.content.font, text, new(0, 0, Main.screenSize.x, Main.screenSize.y), TextAlignment.Center, color, 24);

		batch.DrawString(App.content.font, "(press [Shift+Escape] to come back here)", new(8, Main.screenSize.y - 18), Color.white, 8);
	}
}
