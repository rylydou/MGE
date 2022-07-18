namespace Game;

public class StageTilemap : Platform
{
	public Stage stage;
	public Tileset tileset;
	public GridCollider2D? tilemap;

	public StageTilemap(Stage stage, int tilsetId)
	{
		this.stage = stage;
		this.tileset = Main.tilesets[tilsetId];
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
				if (!tilemap!.data[x, y]) continue;

				var connection = Tileset.Connection.None;

				if (y == 0 || tilemap.data[x, y - 1])
					connection |= Tileset.Connection.Top;
				if (y == stage.height - 1 || tilemap.data[x, y + 1])
					connection |= Tileset.Connection.Bottom;
				if (x == 0 || tilemap.data[x - 1, y])
					connection |= Tileset.Connection.Left;
				if (x == stage.width - 1 || tilemap.data[x + 1, y])
					connection |= Tileset.Connection.Right;

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
