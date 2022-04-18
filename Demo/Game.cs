using MGE;

namespace Demo;

public class Game : Module
{
	Player player = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();
	Player playerAlt = App.content.Get<Prefab>("Scene/Player/Player.node").CreateInstance<Player>();

	FrameBuffer _framebuffer = new(320 * 2, 180 * 2);

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

		Batch2D.current.CheckeredPattern(new(0, 0, App.window.size.x, App.window.size.y), 12, 12, Color.darkGray, Color.gray);

		player.onUpdate(delta);
		playerAlt.onUpdate(delta);
	}

	protected override void Tick(float delta)
	{
		player.onTick(delta);
		playerAlt.onTick(delta);
	}

	void Render(Window window)
	{
		Batch2D.current.Render(_framebuffer);
		Batch2D.current.Clear();

		var scale = new Vector2((float)window.width / _framebuffer.renderWidth, (float)window.height / _framebuffer.renderHeight);
		Batch2D.current.Image(_framebuffer, Vector2.zero, scale, Vector2.zero, 0, Color.white);
		Batch2D.current.Render(window);
	}
}
