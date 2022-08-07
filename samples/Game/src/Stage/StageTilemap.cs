namespace Game;

// TODO Update collider when map is changed
public class StageTilemap : Platform
{
	public Stage stage;
	public string id;
	public Tileset tileset;
	public GridCollider2D? tilemap;

	public StageTilemap(Stage stage, string id)
	{
		this.stage = stage;
		this.id = id;
		this.tileset = Main.tilesets[id];

		var map = new VirtualMap<bool>(stage.width, stage.height, false);

		for (int y = 0; y < stage.height; y++)
		{
			for (int x = 0; x < stage.width; x++)
			{
				map[x, y] = stage.GetTile(x, y) == id;
			}
		}

		tilemap = new GridCollider2D(new(stage.width, stage.height), Stage.TILE_SIZE)
		{
			data = map,
			dontDraw = true,
		};
		collider = tilemap;
	}

	public override bool IsSolid(Vector2 position, Vector2 direction)
	{
		return tileset.physicalBehavior.IsSolid((Vector2Int)position / tileset.tileSize, position, direction);
	}

	protected override void Render(Batch2D batch)
	{
		batch.SetTexture(tileset.texture);

		for (int x = 0; x < stage.width; x++)
		{
			for (int y = 0; y < stage.height; y++)
			{
				if (stage.GetTile(x, y) != id) continue;

				var connection = Tileset.Connection.None;

				if (y == 0 || stage.GetTile(x, y - 1) is not null)
				{
					connection |= Tileset.Connection.Top;
				}
				if (y == stage.height - 1 || stage.GetTile(x, y + 1) is not null)
				{
					connection |= Tileset.Connection.Bottom;
				}
				if (x == 0 || stage.GetTile(x - 1, y) is not null)
				{
					connection |= Tileset.Connection.Left;
				}
				if (x == stage.width - 1 || stage.GetTile(x + 1, y) is not null)
				{
					connection |= Tileset.Connection.Right;
				}

				var tile = tileset.tiles[connection];

				var clipX = tile.x;
				var clipY = tile.y;

				if (tile.properties.HasFlag(Tileset.Tile.Properties.ScrollX))
				{
					clipX += Math.Abs(Calc.Hash(x, y, 0, tileset.GetHashCode())) % (tile.width - tileset.tileSize.x);
				}

				if (tile.properties.HasFlag(Tileset.Tile.Properties.ScrollY))
				{
					clipY += Math.Abs(Calc.Hash(x, y, 1, tileset.GetHashCode())) % (tile.height - tileset.tileSize.y);
				}

				var clip = new RectInt(clipX, clipY, 8, 8);

				batch.DrawImage(tileset.texture, clip, new Vector2(x, y) * Stage.TILE_SIZE, Color.white);
			}
		}
	}
}
