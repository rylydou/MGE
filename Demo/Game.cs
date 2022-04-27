using MGE;

namespace Demo;

public class Game : Module
{
#nullable disable
	public static Game instance;
#nullable restore

	public PlayerData?[] players = new PlayerData[4];

	public Controls[] controls = new Controls[]
	{
		new Controls(-2),
		new Controls(-1),
		new Controls(0),
		new Controls(1),
		new Controls(2),
		new Controls(3),
	};

	public static readonly Vector2Int screenSize = new(320 * 2, 180 * 2);
	public static readonly float unitSize = 8;
	public static readonly Vector2Int screenUnitSize = new(Mathf.CeilToInt(screenSize.x / unitSize), Mathf.CeilToInt(screenSize.y / unitSize));

	Scene scene = new();
	Player player1 = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Player player2 = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Player player3 = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Ground ground = new();

	Font _font = App.content.Get<Font>("Fonts/Montserrat/Bold.ttf");

	FrameBuffer _framebuffer = new(screenSize.x, screenSize.y);

	public Game()
	{
		instance = this;

		ground.mapSize = screenUnitSize;
		ground.tileSize = unitSize;

		scene.AddChild(ground);
		scene.AddChild(player3);
		scene.AddChild(player2);
		scene.AddChild(player1);

		scene.AddChild(App.content.Get<Prefab>("Scene/Items/Shotgun/Shotgun.node").CreateInstance());
		scene.AddChild(App.content.Get<Prefab>("Scene/Items/Shotgun/Shotgun.node").CreateInstance());
	}

	protected override void Startup()
	{
		player1.controls.index = 0;
		player1.sprite = App.content.Get<Texture>("Scene/Player/Chicken.ase");

		player2.controls.index = -1;
		player2.sprite = App.content.Get<Texture>("Scene/Player/Amogus.ase");

		player3.controls.index = -2;
		player3.sprite = App.content.Get<Texture>("Scene/Player/Red.ase");

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
			$"M: {player2.hMoveSpeed / Time.tickDelta:F0}" + '\n' +
			$"H: {player2.hSpeed / Time.tickDelta:F0}" + '\n' +
			$"V: {player2.vSpeed / Time.tickDelta:F0}" + '\n' +
		"";

		_font.DrawString(Batch2D.current, str, new(8), Color.white.translucent);

		/*
		var controller = App.input.controllers[0];
		var smallSize = 128 * 0.15f;

		Batch2D.current.HollowRect(new(64, 64, 128, 128), 1, Color.white);
		Batch2D.current.HollowRect(new(128 - smallSize / 2, 128 - smallSize / 2, smallSize, smallSize), 1, Color.red);

		Batch2D.current.Line(new(128), new Vector2(128) + controller.leftStick * 64, 2, Color.red);
		Batch2D.current.Circle(new Vector2(128) + controller.leftStick * 64, 4, 4, Color.red);

		Batch2D.current.Line(new(128), new Vector2(128) + controller.rightStick * 64, 2, Color.green);
		Batch2D.current.Circle(new Vector2(128) + controller.rightStick * 64, 4, 4, Color.green);
		 */

		Batch2D.current.Render(window);
	}
}
