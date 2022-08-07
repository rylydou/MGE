namespace Game;

public class PlayerData
{
	public int index => Array.IndexOf(Main.players, this);
	public Color color => Main.playerColors[index];

	public int skinIndex;
	public PlayerSkin skin { get => Main.skins[skinIndex]; }

	public PlayerControls controls;

	public Player? player;

	public bool isReady;

	public PlayerData()
	{
		skinIndex = RNG.shared.RandomInt(Main.skins.Count - 1);
		controls = new PlayerControls(0);
	}
}

public class PlayerSkin
{
	public string name;
	public SpriteSheet spriteSheet;

	public PlayerSkin(string name, SpriteSheet spriteSheet)
	{
		this.name = name;
		this.spriteSheet = spriteSheet;
	}
}
