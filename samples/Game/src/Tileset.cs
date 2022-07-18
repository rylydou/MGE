namespace Game;

public class Tileset : IEquatable<Tileset>
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

	[Prop] public string id = "null";
	[Prop] public string name = "Unnamed tileset";
	[Prop] public Vector2Int tileSize;

	[Prop] public IPhysicalTileBehavior physicalBehavior = new SolidTile();

	public Texture texture = null!;

	public AutoDictionary<Connection, Tile> tiles = new(t => t.connection);

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

	public override bool Equals(object? obj)
	{
		return obj is Tileset tileset && Equals(tileset);
	}

	public bool Equals(Tileset? tileset)
	{
		return tileset is not null && GetHashCode() == tileset.GetHashCode();
	}

	public override int GetHashCode()
	{
		return id.GetHashCode();
	}
}

public class TilesetLoader : IContentLoader
{
	public object Load(File file, string filename)
	{
		var tileset = file.ReadMeml<Tileset>();
		var ase = new Aseprite(file + ".ase");
		tileset.texture = new(ase.frames[0].bitmap);

		foreach (var slice in ase.slices)
		{
			var tile = new Tileset.Tile()
			{
				x = slice.originX,
				y = slice.originY,
				width = slice.width,
				height = slice.height,
				connection = Enum.Parse<Tileset.Connection>(slice.name),
			};

			if (!string.IsNullOrEmpty(slice.userDataText))
			{
				tile.properties = Enum.Parse<Tileset.Tile.Properties>(slice.userDataText);
			}

			tileset.tiles.Add(tile);
		}

		return tileset;
	}
}
