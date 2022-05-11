using MGE;

namespace Demo.Screens;

public class PlayingScreen : GameScreen
{
	public Prefab playerPrefab = App.content.Get<Prefab>("Scene/Player/Player.node");

	public override void Start()
	{
		// Spawn ground
		var ground = new Ground();
		ground.tileSize = 8;
		ground.mapSize = Game.screenSize / 8 + 0.5f;
		Game.scene.AddChild(ground);

		// Spawn jumpthough
		var jumpthough = new JumpThough();
		jumpthough.tileSize = 8;
		jumpthough.mapSize = Game.screenSize / 8 + 0.5f;
		Game.scene.AddChild(jumpthough);

		// Spawn players
		foreach (var player in Game.players)
		{
			if (player is null) continue;

			var playerNode = playerPrefab.CreateInstance<Player>(player);
			player.player = playerNode;
			Game.scene.AddChild(playerNode);
		}

		var gun = App.content.Get<Prefab>("Scene/Items/Shotgun/Shotgun.node");
		Game.scene.AddChild(gun.CreateInstance());
		Game.scene.AddChild(gun.CreateInstance());
		Game.scene.AddChild(gun.CreateInstance());
		Game.scene.AddChild(gun.CreateInstance());
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
