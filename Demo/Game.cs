using MGE;

namespace Demo;

public class Game : Module
{
	public static readonly Vector2Int screenSize = new(320 * 2, 180 * 2);
	public static readonly float unitSize = 16;
	public static readonly Vector2Int screenUnitSize = new(Math.CeilToInt(screenSize.x / unitSize), Math.CeilToInt(screenSize.y / unitSize));

	Scene scene = new();
	Player player1 = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Player player2 = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Tilemap<byte> tilemap = new();

	FrameBuffer _framebuffer = new(screenSize.x, screenSize.y);

	struct Particle
	{
		public static Color color = new(0xdff6f5);
		public static Color colorAlt = new(0x8aebf1);

		public Vector2 position;
		public Vector2 velocity;
		public bool alt;

		public void Start()
		{
			velocity = new(RNG.shared.RandomFloat(-48, 48), RNG.shared.RandomFloat(48, 192));
			position = new(RNG.shared.RandomFloat(-screenSize.x / 2, screenSize.x * 1.5f), -RNG.shared.RandomFloat(screenSize.y));
			alt = RNG.shared.RandomBool();
		}

		public void Update(float delta)
		{
			position += velocity * delta;

			if (position.y > screenSize.y)
				Start();
		}

		public void Render()
		{
			Batch2D.current.Rect(position, Vector2.one, alt ? colorAlt : color);
		}
	}

	Particle[] _particles;

	public Game()
	{
		_particles = new Particle[4096];
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Start();
			_particles[i].position = new(_particles[i].position.x, RNG.shared.RandomFloat(-screenSize.y, screenSize.y));
		}

		tilemap.mapSize = screenUnitSize;
		tilemap.tileSize = new(unitSize);

		scene.AddChild(tilemap);
		scene.AddChild(player1);
		scene.AddChild(player2);
	}

	protected override void Startup()
	{
		player1.floorY = _framebuffer.renderHeight - 24;
		player2.floorY = _framebuffer.renderHeight - 24;
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

		for (int i = 0; i < _particles.Length; i++)
			_particles[i].Update(delta / 2);

		var screenScale = App.window.size / screenSize;
		var mousePosition = App.window.renderMouse / screenScale;
		var tilePosition = tilemap.WorldToTilePosition(mousePosition);
		tilePosition = new(Math.Clamp(tilePosition.x, 0, screenUnitSize.x - 1), Math.Clamp(tilePosition.y, 0, screenUnitSize.y - 1));

		if (App.input.mouse.Down(MouseButtons.Left))
			tilemap.SetTile(tilePosition, 1);
		else if (App.input.mouse.Down(MouseButtons.Right))
			tilemap.SetTile(tilePosition, 0);
	}

	protected override void Tick(float delta)
	{
		scene.onTick(delta);
	}

	void Render(Window window)
	{
		for (int i = 0; i < _particles.Length; i++)
			_particles[i].Render();

		Batch2D.current.Render(_framebuffer);
		Batch2D.current.Clear();

		var screenScale = window.size / screenSize;
		Batch2D.current.Image(_framebuffer, Vector2.zero, screenScale, Vector2.zero, 0, Color.white);
		Batch2D.current.Render(window);
	}
}
