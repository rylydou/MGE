namespace Game;

// TODO Update collider
public class StageTilemap : Platform
{
	public Stage stage;
	public int id;
	public Tileset tileset;
	public GridCollider2D? tilemap;

	public StageTilemap(Stage stage, int id)
	{
		this.stage = stage;
		this.id = id;
		this.tileset = Main.tilesets[id];

		tilemap = new GridCollider2D(new(stage.width, stage.height), Stage.TILE_SIZE)
		{
			data = new VirtualMap<bool>(stage.width, stage.height, false),
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
		for (int x = 0; x < stage.width; x++)
		{
			for (int y = 0; y < stage.height; y++)
			{
				if (stage.GetTile(x, y) != id) continue;

				var connection = Tileset.Connection.None;

				if (y == 0 || stage.GetTile(x, y - 1) != 0)
				{
					connection |= Tileset.Connection.Top;
				}
				if (y == stage.height - 1 || stage.GetTile(x, y + 1) != 0)
				{
					connection |= Tileset.Connection.Bottom;
				}
				if (x == 0 || stage.GetTile(x - 1, y) != 0)
				{
					connection |= Tileset.Connection.Left;
				}
				if (x == stage.width - 1 || stage.GetTile(x + 1, y) != 0)
				{
					connection |= Tileset.Connection.Right;
				}

				var tile = tileset.tiles[connection];

				var clipX = tile.x;
				var clipY = tile.y;

				if (tile.properties.HasFlag(Tileset.Tile.Properties.ScrollX))
				{
					clipX += Math.Abs(HashCode.Combine(x, y, 0)) % (tile.width - tileset.tileSize.x);
				}

				if (tile.properties.HasFlag(Tileset.Tile.Properties.ScrollY))
				{
					clipY += Math.Abs(HashCode.Combine(x, y, 1)) % (tile.height - tileset.tileSize.y);
				}

				var clip = new RectInt(clipX, clipY, 8, 8);

				batch.DrawImage(tileset.texture, clip, new Vector2(x, y) * Stage.TILE_SIZE, Color.white);
			}
		}
	}
}
