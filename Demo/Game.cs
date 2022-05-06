using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Demo.Screens;
using MGE;

namespace Demo;

public class Game : Module
{
	static Game()
	{
		ChangeScreen(new SetupScreen());
	}

#nullable disable
	public static Game instance;
#nullable restore

	public static Color[] playerColors = new Color[] {
		new(0xEF4A3A),
		new(0x3385B8),
		new(0x6BA841),
		new(0xCD6093),
	};

	public static PlayerData?[] players = new PlayerData[4];

	public static Controls[] controls = new Controls[]
	{
		new Controls(-2),
		new Controls(-1),
		new Controls(0),
		new Controls(1),
		new Controls(2),
		new Controls(3),
	};

	public static Rect renderRect;
	public static readonly Vector2Int gameScreenSize = new(320 * 2, 180 * 2);
	public static FrameBuffer gameFramebuffer = new(gameScreenSize.x, gameScreenSize.y);

	public static Scene scene = new();
	public static GameScreen screen { get; private set; }
	[MemberNotNull(nameof(screen))]
	public static void ChangeScreen(GameScreen screen)
	{
		scene.RemoveAllChildren();

		Game.screen = screen;
		screen.Start();
	}

	public static AutoDictionary<string, Skin> skins = new(s => s.name);
	void LoadSkins(Folder folder)
	{
		var aseLoader = new MGE.Loaders.AsepriteTextureLoader();

		foreach (var file in folder.GetFiles("*.ase"))
		{
			var tex = (Texture)aseLoader.Load(file, string.Empty);
			var skin = new Skin(file.name, tex);
			skins.Set(skin);
			Log.Info("Loaded skin: " + file);
		}
	}

	SDFFont _font = App.content.Get<SDFFont>("Fonts/Regular.json");

	public Game()
	{
		instance = this;

		LoadSkins(Folder.here / "Skins");
	}

	protected override void Startup()
	{
		App.window.onRender += Render;

		// App.window.SetAspectRatio(new(320, 180));
		// App.window.SetMinSize(new(320, 180));
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

		renderRect = Rect.Fit(gameScreenSize, new Rect(App.window.size));

		var topColor = new Color(0x3978a8);
		var bottomColor = new Color(0x394778);
		Batch2D.current.Rect(new(gameScreenSize), topColor, topColor, bottomColor, bottomColor);

		screen.Update(delta);

		scene.onUpdate(delta);

		Batch2D.current.Render(gameFramebuffer);
	}

	protected override void Tick(float delta)
	{
		screen.Tick(delta);

		scene.onTick(delta);
	}

	void Render(Window window)
	{
		var batch = Batch2D.current;

		// Draw game framebuffer onto window
		batch.Image(gameFramebuffer, renderRect, Color.white);

		// Render screen
		screen.Render(Batch2D.current);

		var profiler = App.profiler;

		batch.Rect(new(0, 0, 200, 100), new(0x000000, 127));
		for (int i = 0; i < App.profiler.frametime.Length; i++)
		{
			var height = (float)(profiler.frametime[i] * 100);
			batch.Rect(i * 2, 0, 1, height, i == profiler.frametimePosition ? Color.white : new(0x6BA841));
		}
		batch.DrawString(_font, $"{1.0 / profiler.currentFrametime:F0} fps", new(4, 4), Color.white, 16);

		var max = App.profiler.memAvailable.Max();
		batch.Rect(new(200 + 2, 0, 200, 100), new(0x000000, 127));
		for (int i = 0; i < App.profiler.memUsage.Length; i++)
		{
			var height = (float)((double)App.profiler.memUsage[i] / max * 100);
			batch.Rect(200 + 2 + i * 2, 0, 1, height, i == App.profiler.memPosition ? Color.white : new(0x3385B8));
		}
		batch.DrawString(_font, $"{(double)profiler.currentMemUsage / 1048576:F2}MB / {profiler.currentMemAvailable / 1048576}MB", new(200 + 2 + 4, 4), Color.white, 16);

		batch.Render(window);
	}
}
