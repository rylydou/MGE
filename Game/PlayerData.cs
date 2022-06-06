namespace Game;

public class PlayerData
{
	public int skinIndex;
	public PlayerSkin skin { get => Main.skins[skinIndex]; }

	public Controls controls;

	public Player? player;

	public bool isReady;

	public PlayerData()
	{
		skinIndex = RNG.shared.RandomInt(Main.skins.Count - 1);
		controls = new Controls(0);
	}
}

public class PlayerSkin
{
	public string name;
	public Texture texture;

	public PlayerSkin(string name, Texture texture)
	{
		this.name = name;
		this.texture = texture;
	}
}
