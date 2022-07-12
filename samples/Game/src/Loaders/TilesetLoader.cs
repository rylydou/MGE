namespace Game.Loaders;

public class TilesetLoader : IContentLoader
{
	public Vector2Int tileSize;

	public object Load(File file, string filename)
	{
		var ase = new Aseprite(file);
		var tileset = new Tileset(new(ase.frames[0].bitmap), tileSize);

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
