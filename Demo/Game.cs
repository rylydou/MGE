using System.Diagnostics.CodeAnalysis;
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
		new(0x2E77A4),
		new(0x6BA841),
		new(0xD46092),
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

	public static AutoDictionary<string, Skin> skins = new(s => s.name);
	public static Font font = App.content.Get<Font>("Fonts/Montserrat/Bold.ttf");

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

	public Game()
	{
		instance = this;

		LoadSkins(Folder.here / "Skins");
	}

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
		if (kb.Pressed(Keys.F11) || (kb.Pressed(Keys.RightAlt) && kb.Pressed(Keys.Enter)))
		{
			App.window.fullscreen = !App.window.fullscreen;
			return;
		}

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
		// Draw game framebuffer onto window
		Batch2D.current.Image(gameFramebuffer, Vector2.zero, window.size / gameScreenSize, Vector2.zero, 0, Color.white);

		// Render screen
		screen.Render(Batch2D.current);

		Batch2D.current.Render(window);
	}
}
