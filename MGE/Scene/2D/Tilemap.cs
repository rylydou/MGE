namespace MGE;

public class Tilemap<T> : Node2D where T : struct
{
	[Prop] public T[,] tiles = new T[0, 0];
	[Prop] public Vector2Int mapSize;
	[Prop] public Vector2 tileSize;

	protected override void Ready()
	{
		tiles = new T[mapSize.x, mapSize.y];
	}

	protected override void Update(float delta)
	{
		var kb = App.input.keyboard;
		if (kb.ctrlOrCommand && kb.Pressed(Keys.S))
		{
			Log.Info("Saved tilemap");
		}
	}

	protected override void Render(Batch2D batch)
	{
		for (int y = 0; y < mapSize.y; y++)
		{
			for (int x = 0; x < mapSize.x; x++)
			{
				var tile = GetTile(x, y);
				if (tile.Equals(default(T))) continue;

				var rect = GetTileRect(x, y);
				batch.HollowRect(rect, 1, Color.green);
			}
		}
	}

	public T GetTile(int x, int y) => tiles[x, y];
	public T GetTile(Vector2Int position) => tiles[position.x, position.y];

	public void SetTile(int x, int y, T value) => tiles[x, y] = value;
	public void SetTile(Vector2Int position, T value) => tiles[position.x, position.y] = value;

	public Rect GetTileRect(int x, int y) => new(x * tileSize.x, y * tileSize.y, tileSize.x, tileSize.y);
	public Rect GetTileRect(Vector2Int tile) => new((Vector2)tile * tileSize, tileSize);

	public Vector2Int WorldToTilePosition(Vector2 position) => position / tileSize;
}
