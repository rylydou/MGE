using MGE;

namespace Demo;

public class Game : Module
{
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
		public float lifetime;
		public bool alt;

		public void Start()
		{
			velocity = new(RNG.shared.RandomFloat(-48, 48), RNG.shared.RandomFloat(48, 192));
			position = new(RNG.shared.RandomFloat(size.x), -RNG.shared.RandomFloat(size.y));
			lifetime = 0;
			alt = RNG.shared.RandomBool();
		}

		public void Update(float delta)
		{
			lifetime += delta;
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
		_particles = new Particle[2048];
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Start();
		}
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

		// Batch2D.current.CheckeredPattern(new(0, 0, App.window.size.x, App.window.size.y), 12, 12, Color.darkGray, Color.gray);

		player.onUpdate(delta);
		playerAlt.onUpdate(delta);

		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Update(delta / 4);
		}
	}

	protected override void Tick(float delta)
	{
		player.onTick(delta);
		playerAlt.onTick(delta);
	}

	void Render(Window window)
	{
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i].Render();
		}

		Batch2D.current.Render(_framebuffer);
		Batch2D.current.Clear();

		var scale = new Vector2((float)window.width / _framebuffer.renderWidth, (float)window.height / _framebuffer.renderHeight);
		Batch2D.current.Image(_framebuffer, Vector2.zero, scale, Vector2.zero, 0, Color.white);
		Batch2D.current.Render(window);
	}
}
