namespace Game.Screens;

public class PlayingScreen : GameScreen
{
	Prefab playerPrefab = App.content.Get<Prefab>("Nodes/Player/Player.node");
	Prefab cratePrefab = App.content.Get<Prefab>("Nodes/Items/Wooden Crate/Item.node");

	public Stage stage = new();

	public override void Start()
	{
		// Load stage
		stage = Folder.data.GetFile("test.stage").ReadMeml<Stage>();
		stage.UnpackTilemap();

		// Spawn in the stage
		Main.scene.AddChild(stage);
		stage.Spawn();

		// Spawn players
		foreach (var player in Main.players)
		{
			if (player is null) continue;

			var playerNode = playerPrefab.CreateInstance<Player>(player);
			player.player = playerNode;
			Main.scene.AddChild(playerNode);
		}

		for (int i = 0; i < 4; i++)
		{
			SpawnCrate();
		}
	}

	public void SpawnCrate()
	{
		Main.scene.AddChild(cratePrefab.CreateInstance());
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		var kb = App.input.keyboard;

		if (kb.Pressed(Keys.C))
		{
			SpawnCrate();
			return;
		}
	}

	public override void Render(Batch2D batch)
	{
	}
}
