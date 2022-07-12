namespace Game;

public class Ground : Solid
{
	public Vector2Int mapSize;
	public float tileSize;

	GridCollider2D? tilemap;

	readonly Tileset _tileset = App.content.Get<Tileset>("Tilesets/Grass.ase");

	Vector2 _worldMousePosition;
	Vector2 _tileMousePosition;
	Vector2 _lastTileMousePosition;

	protected override void Ready()
	{
		tilemap = new GridCollider2D(mapSize, tileSize)
		{
			dontDraw = true,
		};
		collider = tilemap;

		var noise = new Noise();

		var zoom = 4f;
		var topBias = +1.00f;
		var bottomBias = -0.00f;

		for (int y = 0; y < mapSize.y; y++)
		{
			for (int x = 0; x < mapSize.x; x++)
			{
				var v = noise.GetNoise(x * zoom, y * zoom);
				var bias = Mathf.Lerp(topBias, bottomBias, (float)y / mapSize.y);
				tilemap.data[x, y] = v > bias;
			}
		}
	}

	protected override void Update(float delta)
	{
		var screenScale = App.window.size / Main.screenSize;
		_worldMousePosition = App.window.renderMouse / screenScale;
		_tileMousePosition = _worldMousePosition / tileSize;
		_tileMousePosition = new(Mathf.Clamp(_tileMousePosition.x, 0, mapSize.x - 1), Mathf.Clamp(_tileMousePosition.y, 0, mapSize.y - 1));

		if (!App.input.keyboard.alt)
		{
			if (App.input.mouse.Down(MouseButtons.Left))
				SetTiles(_tileMousePosition, _lastTileMousePosition, !App.input.keyboard.shift);
			else if (App.input.mouse.Down(MouseButtons.Right))
				SetTiles(_tileMousePosition, _lastTileMousePosition, false);
		}

		_lastTileMousePosition = _tileMousePosition;
	}

	void SetTiles(Vector2 from, Vector2 to, bool value)
	{
		foreach (var item in Calc.GetTilesOnLine(from, to))
		{
			tilemap![item.x, item.y] = value;
		}
	}

	protected override void Render(Batch2D batch)
	{
		var pos = (Vector2Int)_tileMousePosition * tileSize;
		var guideColor = new Color(0x5A5353FF);

		batch.SetBox(new(0, pos.y + tileSize / 2, mapSize.x * tileSize, 1), guideColor);
		batch.SetBox(new(pos.x + tileSize / 2, 0, 1, mapSize.y * tileSize), guideColor);

		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				if (!tilemap!.data[x, y]) continue;

				var connection = Tileset.Connection.None;

				if (y == 0 || tilemap.data[x, y - 1])
					connection |= Tileset.Connection.Top;
				if (y == mapSize.y - 1 || tilemap.data[x, y + 1])
					connection |= Tileset.Connection.Bottom;
				if (x == 0 || tilemap.data[x - 1, y])
					connection |= Tileset.Connection.Left;
				if (x == mapSize.x - 1 || tilemap.data[x + 1, y])
					connection |= Tileset.Connection.Right;

				var tile = _tileset.tiles[connection];

				var clipX = tile.x;
				var clipY = tile.y;

				if (tile.properties.HasFlag(Tileset.Tile.Properties.ScrollX))
				{
					clipX += Math.Abs(HashCode.Combine(x, y, 0)) % (tile.width - _tileset.tileSize.x);
				}

				if (tile.properties.HasFlag(Tileset.Tile.Properties.ScrollY))
				{
					clipY += Math.Abs(HashCode.Combine(x, y, 1)) % (tile.height - _tileset.tileSize.y);
				}

				var clip = new RectInt(clipX, clipY, 8, 8);

				batch.DrawImage(_tileset.texture, clip, new Vector2(x, y) * tileSize, Color.white);
			}
		}

		batch.SetRect(new(pos, tileSize), 1, new Color(0xE6482EFF));
	}
}
