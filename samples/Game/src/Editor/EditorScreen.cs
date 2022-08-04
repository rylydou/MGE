using MGE.UI;

namespace Game.Screens;

public class EditorScreen : GameScreen
{
	public Vector2 canvasSize = Main.screenSize;

	public Stage stage = new();

	public Tileset selectedTile = Main.tilesets.First();

	Vector2Int _prevTilePosition;

	public UICanvas canvas = new() { fixedSize = Main.screenSize };

	List<Tileset> _recentTilesets = new();

	public override void Start()
	{
		App.input.SetMouseCursorMode(CursorModes.Hidden);

		Main.scene.AddChild(stage);

		foreach (var tileset in Main.tilesets)
		{
			_recentTilesets.Add(tileset);
		}
	}

	public override void Tick(float delta)
	{
	}

	public override void Update(float delta)
	{
		// selectedTile = Main.tilesets.ElementAt(1);

		var kb = App.input.keyboard;
		var mouse = App.input.mouse;

		var cursor = (Vector2Int)(Main.mouse / Stage.TILE_SIZE);

		// if (Mathf.Abs(mouse.wheel.y) > 0.5f)
		// {
		// 	selectedTileIndex += (int)Mathf.Sign(mouse.wheel.y);
		// }

		// selectedTileIndex = Math.Min(selectedTileIndex, Main.tilesets.Count - 1);

		if (mouse.leftDown)
		{
			foreach (var pos in Calc.GetTilesOnLine(_prevTilePosition, cursor))
			{
				if (IsInBounds(pos.x, pos.y))
				{
					stage.SetTile(pos.x, pos.y, selectedTile.id);
				}
			}
		}
		else if (mouse.rightDown)
		{
			foreach (var pos in Calc.GetTilesOnLine(_prevTilePosition, cursor))
			{
				if (IsInBounds(pos.x, pos.y))
				{
					stage.SetTile(pos.x, pos.y, null);
				}
			}
		}
		else if (mouse.middlePressed)
		{
			if (IsInBounds(cursor.x, cursor.y))
			{
				var pickedTileId = stage.GetTile(cursor.x, cursor.y);

				// Don't pick air
				if (pickedTileId is not null)
				{
					SelectTileset(Main.tilesets[pickedTileId]);
				}
			}
		}

		if (kb.Pressed(Keys.D1))
			SelectTileset(_recentTilesets[0]);
		if (kb.Pressed(Keys.D2))
			SelectTileset(_recentTilesets[1]);
		if (kb.Pressed(Keys.D3))
			SelectTileset(_recentTilesets[2]);
		if (kb.Pressed(Keys.D4))
			SelectTileset(_recentTilesets[3]);

		if (kb.shift && kb.Pressed(Keys.Delete))
		{
			// Delete old stage
			stage.RemoveSelf();

			// Load stage
			stage = new();
			stage.UnpackTilemap();

			// Spawn in the stage
			Main.scene.AddChild(stage);
			stage.Spawn();
		}

		if (kb.ctrl && kb.Pressed(Keys.S))
		{
			stage.PackTilemap();

			if (Folder.data.GetFile("test.stage").exists) Folder.data.GetFile("test.stage").Delete();
			Folder.data.GetFile("test.stage").WriteMeml(stage);
		}

		if (kb.ctrl && kb.Pressed(Keys.L))
		{
			// Delete old stage
			stage.RemoveSelf();

			// Load stage
			stage = (Folder.data.GetFile("test.stage").ReadMeml<Stage>());
			stage.UnpackTilemap();

			// Spawn in the stage
			Main.scene.AddChild(stage);
			stage.Spawn();
		}

		_prevTilePosition = cursor;
	}

	public override void Render(Batch2D batch)
	{
		// var bg = new Color(0x3978A8ff);
		// var fg = new Color(0xDFF6F5ff);
		var bg = new Color(0xFAB23Aff);
		var fg = new Color(0x302C2Eff);

		var mouse = App.input.mouse;

		var tilePos = (Vector2Int)(Main.mouse / Stage.TILE_SIZE);
		var mouseTilePos = (Vector2)tilePos * Stage.TILE_SIZE;

		// UI
		canvas.RenderCanvas(batch);

		var i = 0;
		foreach (var tileset in _recentTilesets)
		{
			batch.Draw(tileset.texture, GetTilesetPreview(tileset), new(6 + i * (8 + 6), 6), Color.white);
			i++;
		}

		// Cursor
		if (App.window.mouseOver)
		{
			// Selection Box
			batch.SetRect(new(mouseTilePos, Stage.TILE_SIZE), 1, Color.white.WithAlpha(64));

			// Cursor
			batch.PushMatrix(Main.mouse - 0.5f, Vector2.zero, Vector2.one, 0);
			{
				if (mouse.leftDown || mouse.rightDown)
				{
					// Dot
					batch.SetBox(new(0, 0, 1, 1), Color.white);
				}
				else
				{
					// Tool text
					batch.DrawString(App.content.font, selectedTile.name, new(Stage.TILE_SIZE), Color.white, 8);

					// Crosshair
					batch.SetBox(new(-3, 0, 2, 1), Color.white);
					batch.SetBox(new(+2, 0, 2, 1), Color.white);

					batch.SetBox(new(0, -3, 1, 2), Color.white);
					batch.SetBox(new(0, +2, 1, 2), Color.white);
				}
			}
			batch.PopMatrix();
		}
	}

	bool IsInBounds(int x, int y)
	{
		return x >= 0 && y >= 0 && x < stage.width && y < stage.height;
	}

	Rect GetTilesetPreview(Tileset tileset)
	{
		var tile = tileset.tiles[Tileset.Connection.None];
		return new(tile.x, tile.y, tile.width, tile.height);
	}

	void SelectTileset(in string id)
	{
		var tileset = Main.tilesets[id];
		SelectTileset(tileset);
	}

	void SelectTileset(Tileset tileset)
	{
		selectedTile = tileset;

		_recentTilesets.Remove(tileset);
		_recentTilesets.Insert(0, tileset);
	}
}
