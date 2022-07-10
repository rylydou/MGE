namespace Game;

public class Tileset
{
	[Flags]
	public enum Connection : byte
	{
		None = 0,

		Top = 1,
		Bottom = 2,
		Left = 4,
		Right = 8,

		All = Top | Bottom | Left | Right,
	}

	public class Tile
	{
		[Flags]
		public enum Properties : byte
		{
			None = 0,

			FlipX = 1,
			FlipY = 2,

			ScrollX = 4,
			ScrollY = 8,
		}

		public int x;
		public int y;
		public int width;
		public int height;

		public Connection connection;

		public Properties properties;
	}

	public readonly Texture texture;
	public readonly Vector2Int tileSize;

	public readonly AutoDictionary<Connection, Tile> tiles = new(t => t.connection);

	public Tileset(Texture texture, Vector2Int tileSize)
	{
		this.texture = texture;
		this.tileSize = tileSize;
	}

	// public void DrawMap(Vector2 position, Vector2Int mapSize, Func<int, int, bool> isSolid, bool treatOutofBoundsAsSolid = true)
	// {
	// 	for (int y = 0; y < mapSize.y; y++)
	// 	{
	// 		for (int x = 0; x < mapSize.x; x++)
	// 		{
	// 			if (!isSolid(x, y)) return;
	// 		}
	// 	}
	// }
}
