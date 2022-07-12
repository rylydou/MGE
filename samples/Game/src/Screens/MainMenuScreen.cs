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
		var text =
			"Unnamed Party Game\n" +
			"S - Start\n" +
			"E - Editor";

		var time = (float)Time.duration.TotalSeconds;
		var color = new Color(
			Math.Abs(Mathf.Sin(time)),
			Math.Abs(Mathf.Sin(time + Mathf.TAU / 3)),
			Math.Abs(Mathf.Sin(time + Mathf.TAU * 2 / 3)),
		1f);

		batch.DrawString(App.content.font, text, new(Mathf.Sin(time) * 64, Mathf.Cos(time) * 16, App.window.size.x, App.window.size.y), TextAlignment.Center, color, 96 + Mathf.Sin(time * 0.67f) * 16);

		batch.DrawString(App.content.font, "(press [Shift+Escape] to come back here)", new(8, App.window.height - 42), Color.white, 24);
	}
}
