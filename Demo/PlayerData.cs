namespace Demo;

public class PlayerData
{
	public Skin skin;
	public Controls? controls;

	public Player? player;

	public bool isReady;

	public PlayerData()
	{
		skin = Game.skins["_Template"];
	}
}
