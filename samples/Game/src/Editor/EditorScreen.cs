using Game.Editor;

namespace Game;

public class EditorScreen : GameScreen
{
	public Stage stage = new();

	public List<IEditorItem> items = new();
	public int selection;

	public EditorHotbar toolbar;

	SpriteSheet _icons = App.content.Get<SpriteSheet>("Icons.ase");

	public Vector2 cursor = Main.screenSize / 2 + Stage.TILE_SIZE / 2;
	Vector2Int _tileCursor => cursor / Stage.TILE_SIZE;
	Vector2Int _prevTileCursor;

	public bool controllerMode = false;

	public float tooltipAge;

	public string tooltipText = "";
	public float tooltipLifetime;

	public void ShowTooltip(string text)
	{
		tooltipAge = 0;
		tooltipText = text;
		tooltipLifetime = 1;
	}

	public EditorScreen()
	{
		toolbar = new(this);
	}

	public override void Start()
	{
		App.input.SetMouseCursorMode(CursorModes.Hidden);

		Main.scene.AddChild(stage);

		// items.Add(Main.tilesets.First());
		foreach (var tileset in Main.tilesets)
		{
			items.Add(tileset);
		}
	}

	public override void Tick(float delta) { }

	public override void Update(float delta)
	{
		var kb = App.input.keyboard;
		var mouse = App.input.mouse;

		var con = App.input.controllers[0];

		if ((kb.alt && kb.Pressed(Keys.G)) || con.Pressed(Buttons.Select))
		{
			controllerMode = !controllerMode;
			if (controllerMode)
			{
				ShowTooltip("Switched to controller mode");
				App.input.SetMouseCursorMode(CursorModes.Normal);
			}
			else
			{
				ShowTooltip("Switched to mouse mode");
				App.input.SetMouseCursorMode(CursorModes.Hidden);
			}
		}

		toolbar.Update(delta);

		if (controllerMode)
		{
			var value = con.leftStick;
			var speed = Mathf.Lerp(96, 320, con.Axis(Axes.LeftTrigger) / 2 + 0.5f);
			value = value.normalized * Mathf.Pow(Vector2.ClampLength(value, 1f).length, 2.0f);
			cursor += value * speed * delta;
			cursor = new(Mathf.Clamp(cursor.x, 0, Main.screenSize.x - 1), Mathf.Clamp(cursor.y, 0, Main.screenSize.y - 1));
		}
		else
		{
			cursor = Main.mouse;
		}

		if (controllerMode)
		{
			if (con.Repeated(Buttons.LeftShoulder, 0.2f, 1f / 15))
			{
				if (selection > 0)
				{
					selection--;
				}
				else if (con.Pressed(Buttons.LeftShoulder))
				{
					selection--;
				}
			}

			if (con.Repeated(Buttons.RightShoulder, 0.2f, 1f / 15))
			{
				if (selection < items.Count - 1)
				{
					selection++;
				}
				else if (con.Pressed(Buttons.RightShoulder))
				{
					selection++;
				}
			}
		}
		else
		{
			if (Mathf.Abs(mouse.wheel.y) > 0.5f)
			{
				selection += (int)Mathf.Sign(mouse.wheel.y);
			}
		}

		if (selection < 0)
		{
			selection = items.Count - 1;
		}

		if (selection >= items.Count)
		{
			selection = 0;
		}

		if (items[selection] is Tileset selectedTileset)
		{
			UpdateTile(selectedTileset);
		}

		if (controllerMode ? con.Pressed(Buttons.Y) : mouse.middlePressed)
		{
			if (IsInBounds(_tileCursor.x, _tileCursor.y))
			{
				var pickedTileId = stage.GetTile(_tileCursor.x, _tileCursor.y);

				// Don't pick air
				if (pickedTileId is not null)
				{
					var tileset = Main.tilesets[pickedTileId];
					ShowTooltip(tileset.name);
					PickItem(tileset, controllerMode ? con.Axis(Axes.LeftTrigger) > 0.5f : kb.ctrl);
				}
			}
		}

		if (kb.ctrl && kb.Pressed(Keys.Delete))
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

		_prevTileCursor = _tileCursor;

		if (tooltipAge < tooltipLifetime)
		{
			tooltipAge += delta;
		}
	}

	public void UpdateTile(Tileset selectedTile)
	{
		var kb = App.input.keyboard;
		var mouse = App.input.mouse;
		var con = App.input.controllers[0];

		if (kb.ctrl && kb.Pressed(Keys.F))
		{
			for (int y = 0; y < stage.height; y++)
			{
				for (int x = 0; x < stage.width; x++)
				{
					stage.SetTile(x, y, selectedTile.id);
				}
			}
		}

		if (controllerMode ? con.Down(Buttons.A) : mouse.leftDown)
		{
			foreach (var pos in Calc.GetTilesOnLine(_prevTileCursor, _tileCursor))
			{
				if (IsInBounds(pos.x, pos.y))
				{
					stage.SetTile(pos.x, pos.y, selectedTile.id);
				}
			}
		}

		if (controllerMode ? con.Down(Buttons.B) : mouse.rightDown)
		{
			foreach (var pos in Calc.GetTilesOnLine(_prevTileCursor, _tileCursor))
			{
				if (IsInBounds(pos.x, pos.y))
				{
					stage.SetTile(pos.x, pos.y, null);
				}
			}
		}
	}

	public override void Render(Batch2D batch)
	{
		var mouse = App.input.mouse;
		var con = App.input.controllers[0];

		var tilePos = (Vector2Int)(cursor / Stage.TILE_SIZE);
		var mouseTilePos = (Vector2)tilePos * Stage.TILE_SIZE;

		// var i = 0;
		// foreach (var tileset in _tiles)
		// {
		// 	batch.Draw(tileset.texture, GetTilesetPreview(tileset), new(6 + i * (8 + 6), 6), Color.white);
		// 	i++;
		// }

		// Cursor

		toolbar.Render(batch);

		if (controllerMode || App.window.mouseOver)
		{
			// Selection Box
			batch.SetRect(new(mouseTilePos, Stage.TILE_SIZE), 1, Color.white.WithAlpha(64));

			// Cursor
			batch.PushMatrix(cursor - 0.5f, Vector2.zero, Vector2.one, 0);
			{
				if (tooltipAge < tooltipLifetime)
				{
					batch.DrawString(App.content.font, tooltipText, new(4), Color.white.WithAlpha(1 - tooltipAge / tooltipLifetime), 8);
				}

				if (controllerMode ? (con.Down(Buttons.A) || con.Down(Buttons.B)) : (mouse.leftDown || mouse.rightDown))
				{
					// Dot
					batch.SetBox(new(0, 0, 1, 1), Color.white);
				}
				else
				{
					// Tool text
					// batch.DrawString(App.content.font, selectedTile.name, new(Stage.TILE_SIZE), Color.white, 8);

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

	public bool PickItem(IEditorItem item, bool addNew = false)
	{
		var index = items.IndexOf(item);

		// Select the item if it already in the hotbar
		if (index >= 0)
		{
			selection = index;
			return true;
		}

		if (addNew)
		{
			// Add new item to hotbar
			selection = items.Count;
			items.Add(item);
		}

		// Swap out the selected item with the new item
		items[selection] = item;
		return false;
	}
}
