namespace MGE;

public class Tilemap<T> : Node2D
{
	[Prop] public T[,] tiles = new T[0, 0];
	[Prop] public Vector2Int mapSize;
	[Prop] public Vector2 tileSize;

	protected override void Ready()
	{
		tiles = new T[mapSize.x, mapSize.y];
	}

	protected override void Render(Batch2D batch)
	{
		for (int y = 0; y < mapSize.y; y++)
		{
			for (int x = 0; x < mapSize.y; x++)
			{
				var rect = GetTileRect(x, y);
				batch.HollowRect(rect, 1, Color.red);
			}
		}
	}

	public T GetTile(int x, int y) => tiles[x, y];
	public T GetTile(Vector2Int position) => tiles[position.x, position.y];

	public Rect GetTileRect(int x, int y) => new(x * tileSize.x, y * tileSize.y, tileSize.x, tileSize.y);
	public Rect GetTileRect(Vector2Int tile) => new((Vector2)tile * tileSize, tileSize);

	public Vector2Int GetTilePosition(Vector2 position) => position / tileSize;
}
