using Demo.Screens;

namespace Demo;

public static class Game
{
	static Game()
	{
		ChangeScreen(new SetupScreen());
	}

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
	public static readonly Vector2Int screenSize = new(320 * 2, 180 * 2);
	public static Scene scene = new();
	public static GameScreen screen { get; private set; }
	[MemberNotNull(nameof(screen))]
	public static void ChangeScreen(GameScreen screen)
	{
		scene.RemoveAllChildren();

		Game.screen = screen;
		screen.Start();
	}

	public static List<PlayerSkin> skins = new();
	public static void LoadSkins(Folder folder)
	{
		var aseLoader = new MGE.Loaders.AsepriteTextureLoader();

		foreach (var file in folder.GetFiles("*.ase"))
		{
			var tex = (Texture)aseLoader.Load(file, string.Empty);
			var skin = new PlayerSkin(file.name, tex);
			skins.Add(skin);
			Log.Info("Loaded skin: " + file);
		}
	}
}
