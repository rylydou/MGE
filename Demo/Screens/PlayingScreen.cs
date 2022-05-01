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
		ground.mapSize = Game.gameScreenSize / 8 + 0.5f;
		Game.scene.AddChild(ground);

		// Spawn players
		foreach (var player in Game.players)
		{
			if (player is null) continue;

			var playerNode = playerPrefab.CreateInstance<Player>(player);
			player.player = playerNode;
			Game.scene.AddChild(playerNode);
		}
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
