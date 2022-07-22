namespace Game.Screens;

public class EditorScreen : GameScreen
{
	public Stage stage = new();

	public Tileset selectedTile = Main.tilesets.First();

	public override void Start()
	{
		Main.scene.AddChild(stage);
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		var kb = App.input.keyboard;
		var mouse = App.input.mouse;

		var screenScale = App.window.size / Main.screenSize;
		var mouseTilePosition = (Vector2Int)(App.window.mouse / screenScale / Stage.TILE_SIZE);

		// if (Mathf.Abs(mouse.wheel.y) > 0.5f)
		// {
		// 	selectedTileIndex += (int)Mathf.Sign(mouse.wheel.y);
		// }

		// selectedTileIndex = Math.Min(selectedTileIndex, Main.tilesets.Count - 1);

		if (mouseTilePosition.x >= 0 && mouseTilePosition.y >= 0 && mouseTilePosition.x < stage.width && mouseTilePosition.y < stage.height)
		{
			if (mouse.leftDown)
			{
				stage.SetTile(mouseTilePosition.x, mouseTilePosition.y, selectedTile.GetHashCode());
			}
			else if (mouse.rightDown)
			{
				stage.SetTile(mouseTilePosition.x, mouseTilePosition.y, 0);
			}
		}
	}

	public override void Render(Batch2D batch)
	{
		batch.Draw(selectedTile.texture, new(6), Color.white);

		var screenScale = App.window.size / Main.screenSize;
		var mouseTilePosition = (Vector2Int)(App.window.mouse / screenScale / Stage.TILE_SIZE);

		batch.SetRect(new((Vector2)mouseTilePosition * Stage.TILE_SIZE * screenScale, Stage.TILE_SIZE * screenScale), 1, Color.white);
	}
}
