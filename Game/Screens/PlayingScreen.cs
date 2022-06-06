using MGE;

namespace Game.Screens;

public class PlayingScreen : GameScreen
{
	public Prefab playerPrefab = App.content.Get<Prefab>("Scene/Player/Player.node");

	public override void Start()
	{
		// Spawn ground
		var ground = new Ground();
		ground.tileSize = 8;
		ground.mapSize = Main.screenSize / 8 + 0.5f;
		Main.scene.AddChild(ground);

		// Spawn jumpthough
		var jumpthough = new JumpThough();
		jumpthough.tileSize = 8;
		jumpthough.mapSize = Main.screenSize / 8 + 0.5f;
		Main.scene.AddChild(jumpthough);

		// Spawn players
		foreach (var player in Main.players)
		{
			if (player is null) continue;

			var playerNode = playerPrefab.CreateInstance<Player>(player);
			player.player = playerNode;
			Main.scene.AddChild(playerNode);
		}

		var gun = App.content.Get<Prefab>("Scene/Items/Wooden Crate/Item.node");
		Main.scene.AddChild(gun.CreateInstance());
		Main.scene.AddChild(gun.CreateInstance());
		Main.scene.AddChild(gun.CreateInstance());
		Main.scene.AddChild(gun.CreateInstance());
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
	}

	public override void Render(Batch2D batch)
	{
	}
}
