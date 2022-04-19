using MGE;

namespace Demo;

public class Game : Module
{
	Scene scene = new();
	Player player = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Player playerAlt = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();

	FrameBuffer _framebuffer = new(320 * 2, 180 * 2);

	struct Particle
	{
		public static Vector2 size;
		public static Color color = new(0xdff6f5);
		public static Color colorAlt = new(0x8aebf1);

		public Vector2 position;
		public Vector2 velocity;
		public bool alt;

		public void Start()
		{
			velocity = new(RNG.shared.RandomFloat(-48, 48), RNG.shared.RandomFloat(48, 192));
			position = new(RNG.shared.RandomFloat(-size.x / 2, size.x * 1.5f), -RNG.shared.RandomFloat(size.y));
			alt = RNG.shared.RandomBool();
		}

		public void Update(float delta)
		{
			position += velocity * delta;

			if (position.y > size.y)
			{
				Start();
			}
		}

		public void Render()
		{
			Batch2D.current.Rect(position, Vector2.one, alt ? colorAlt : color);
		}
	}

	Particle[] _particles;

	public Game()
	{
		Particle.size = new(_framebuffer.renderWidth, _framebuffer.renderHeight);
		_particles = new Particle[4096];
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Start();
			_particles[i].position = new(_particles[i].position.x, RNG.shared.RandomFloat(-_framebuffer.renderHeight, _framebuffer.renderHeight));
		}

		scene.AddChild(player);
		scene.AddChild(playerAlt);
	}

	protected override void Startup()
	{
		player.floorY = _framebuffer.renderHeight - 24;
		playerAlt.floorY = _framebuffer.renderHeight - 24;
		playerAlt.alt = true;

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
		Batch2D.current.Rect(new(_framebuffer.renderWidth, _framebuffer.renderHeight), topColor, topColor, bottomColor, bottomColor);

		scene.onUpdate(delta);

		for (int i = 0; i < _particles.Length; i++)
			_particles[i].Update(delta / 2);
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

		var scale = new Vector2((float)window.width / _framebuffer.renderWidth, (float)window.height / _framebuffer.renderHeight);
		Batch2D.current.Image(_framebuffer, Vector2.zero, scale, Vector2.zero, 0, Color.white);
		Batch2D.current.Render(window);
	}
}
