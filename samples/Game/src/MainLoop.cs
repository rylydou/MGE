#nullable disable

using Game.Screens;

namespace Game;

public class MainLoop : Module
{
	public enum ScreenScalingMode
	{
		Fit,
		IntegerScale,
	}

#nullable disable
	public static MainLoop instance;
#nullable restore

	public FrameBuffer gameFramebuffer = new(Main.screenSize.x, Main.screenSize.y);

	public ScreenScalingMode screenScalingMode = ScreenScalingMode.Fit;

	public MainLoop()
	{
		instance = this;

		App.content.font = App.content.Get<SDFFont>("Fonts/Montserrat.json");
	}

	protected override void Startup()
	{
		App.window.onRender += Render;

		// App.window.SetAspectRatio(Main.screenSize);
		App.window.SetMinSize(Main.screenSize);

		Main.LoadSkins(Folder.here / "Skins");
		Main.LoadSkins(Folder.data / "Skins");

		Main.tilesets.Add(App.content.Get<Tileset>("Tiles/Block.meml"));
		Main.tilesets.Add(App.content.Get<Tileset>("Tiles/Grass.meml"));
		Main.tilesets.Add(App.content.Get<Tileset>("Tiles/Dirt.meml"));
		Main.tilesets.Add(App.content.Get<Tileset>("Tiles/Stone.meml"));

		App.window.onResize += UpdateRenderRect;
		UpdateRenderRect(App.window);
	}

	public void UpdateRenderRect(Window window)
	{
		var size = screenScalingMode switch
		{
			ScreenScalingMode.IntegerScale => Calc.ScaleScreenInteger(window.size.x, App.window.size.y, Main.screenSize.x, Main.screenSize.y),
			ScreenScalingMode.Fit => Calc.ScaleScreenFit(window.size.x, App.window.size.y, Main.screenSize.x, Main.screenSize.y),
			_ => throw new Exception("Unsupported screen scaling mode " + screenScalingMode),
		};

		var rect = new RectInt((int)(window.size.x - size.x) / 2, (int)(window.size.y - size.y) / 2, (int)size.x, (int)size.y);

		Main.renderRect = rect;
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
		Batch2D.current.SetBox(new(Main.screenSize), new(0x394778FF));

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

		var scale = Main.renderRect.size / (Vector2)Main.screenSize;

		batch.PushMatrix(Main.renderRect.topLeft / scale, Vector2.zero, scale, 0f);
		Main.mouse = (App.window.mouse - Main.renderRect.position) / scale;

		// Draw game framebuffer onto window
		batch.DrawImage(gameFramebuffer, new Rect(Main.screenSize), Color.white);

		// Render screen
		Main.screen.Render(Batch2D.current);

		batch.PopMatrix();

		App.graphics.Clear(window, Color.black);

		batch.Render(window);
	}
}
