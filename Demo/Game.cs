using MGE;

namespace Demo;

public class Game : Module
{
	public static readonly Vector2Int screenSize = new(320 * 2, 180 * 2);
	public static readonly float unitSize = 8;
	public static readonly Vector2Int screenUnitSize = new(Math.CeilToInt(screenSize.x / unitSize), Math.CeilToInt(screenSize.y / unitSize));

	Scene scene = new();
	Player player1 = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Player player2 = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Ground ground = new();

	Font _font = App.content.Get<Font>("Fonts/Inter/Regular.ttf");

	FrameBuffer _framebuffer = new(screenSize.x, screenSize.y);

	public Game()
	{
		ground.mapSize = screenUnitSize;
		ground.tileSize = unitSize;

		scene.AddChild(ground);
		scene.AddChild(player1);
		// scene.AddChild(player2);
	}

	protected override void Startup()
	{
		// player1.floorY = _framebuffer.renderHeight - 24;
		// player2.floorY = _framebuffer.renderHeight - 24;
		player2.isPlayer2 = true;

		App.window.onRender += Render;

		App.window.SetAspectRatio(new(320, 180));
		App.window.SetMinSize(new(320, 180));
	}

	protected override void Shutdown()
	{
		App.window.onRender -= Render;
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;
		if (kb.Pressed(Keys.F11) || (kb.Pressed(Keys.RightAlt) && kb.Pressed(Keys.Enter)))
		{
			App.window.fullscreen = !App.window.fullscreen;
			return;
		}

		var topColor = new Color(0x3978a8);
		var bottomColor = new Color(0x394778);
		Batch2D.current.Rect(new(screenSize), topColor, topColor, bottomColor, bottomColor);

		scene.onUpdate(delta);
	}

	protected override void Tick(float delta)
	{
		scene.onTick(delta);
	}

	void Render(Window window)
	{
		Batch2D.current.Render(_framebuffer);
		Batch2D.current.Clear();

		var screenScale = window.size / screenSize;
		Batch2D.current.Image(_framebuffer, Vector2.zero, screenScale, Vector2.zero, 0, Color.white);

		var str =
			$"{Time.fps} fps" + '\n' +
			$"H: {player1.hSpeed / Time.fixedDelta:F0}" + '\n' +
			$"X: {player1.extraHSpeed / Time.fixedDelta:F0}" + '\n' +
			$"V: {player1.vSpeed / Time.fixedDelta:F0}";
		_font.DrawString(Batch2D.current, str, new(8), Color.white.translucent);

		Batch2D.current.Render(window);
	}
}
