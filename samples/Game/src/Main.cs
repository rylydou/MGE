using Game.Screens;

namespace Game;

public static class Main
{
	static Main()
	{
		ChangeScreen(new MainMenuScreen());
	}

	#region Screen

	public static Scene scene = new();
	public static GameScreen screen { get; private set; }
	[MemberNotNull(nameof(screen))]
	public static void ChangeScreen(GameScreen screen)
	{
		scene.RemoveAllChildren();

		Main.screen = screen;
		screen.Start();
	}

	public static Vector2 mouse;

	#endregion Screen

	#region Player

	public static Color[] playerColors = new Color[] {
		new(0xEF4A3AFF),
		new(0x3385B8FF),
		new(0x6BA841FF),
		new(0xCD6093FF),
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

	public static List<PlayerSkin> skins = new();
	public static void LoadSkins(Folder folder)
	{
		var aseLoader = new MGE.Loaders.AsepriteSpriteSheetLoader();

		foreach (var file in folder.GetFiles("*.ase"))
		{
			var spriteSheet = (SpriteSheet)aseLoader.Load(file, string.Empty);
			var skin = new PlayerSkin(file.name, spriteSheet);
			skins.Add(skin);
			Log.Info("Loaded skin: " + file);
		}
	}

	#endregion Player

	#region Rendering

	public static Rect renderRect;
	public static readonly Vector2Int screenSize = new(320 * 2, 180 * 2);

	#endregion Rendering

	#region Tileset

	public static readonly AutoDictionary<string, Tileset> tilesets = new(ts => ts.id);

	#endregion Tileset
}
