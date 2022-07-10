#nullable disable

using Game.Screens;

namespace Game;

public class MainLoop : Module
{
#nullable disable
	public static MainLoop instance;
#nullable restore

	public FrameBuffer gameFramebuffer = new(Main.screenSize.x, Main.screenSize.y);

	public MainLoop()
	{
		instance = this;

		Main.LoadSkins(Folder.here / "Skins");
		Main.LoadSkins(Folder.data / "Skins");
	}

	protected override void Startup()
	{
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
		if (kb.Pressed(Keys.F11) || (kb.alt && kb.Pressed(Keys.Enter)))
		{
			App.window.fullscreen = !App.window.fullscreen;
		}

		if (kb.Pressed(Keys.F2))
		{
			Main.scene.showColliders = !Main.scene.showColliders;
		}

		if (kb.shift && kb.Pressed(Keys.Escape))
		{
			Main.ChangeScreen(new MainMenuScreen());
		}

		if (kb.Pressed(Keys.V))
		{
			App.window.vsync = !App.window.vsync;
		}

		var topColor = new Color(0x3978A8FF);
		var bottomColor = new Color(0x394778FF);
		Batch2D.current.SetBox(new(Main.screenSize), topColor, topColor, bottomColor, bottomColor);
		Batch2D.current.SetBox(new(Main.screenSize), new(0x302C2EFF));

		Main.screen.Update(delta);
		Main.scene.onUpdate(delta);

		Batch2D.current.Render(gameFramebuffer);
	}

	protected override void Tick(float delta)
	{
		Main.screen.Tick(delta);
		Main.scene.onTick(delta);
	}

	void Render(Window window)
	{
		var batch = Batch2D.current;

		// Draw game framebuffer onto window
		batch.DrawImage(gameFramebuffer, new Rect(window.width, window.height), Color.white);

		// Render screen
		Main.screen.Render(Batch2D.current);

		batch.Render(window);
	}
}
