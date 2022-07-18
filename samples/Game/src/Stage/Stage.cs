#nullable disable

using PaletteIndex = System.Byte;

namespace Game;

public class Stage : Node
{
	public const float TILE_SIZE = 8f;

	[Prop] public int width = 80;
	[Prop] public int height = 45;

	[Prop] PaletteIndex[] _tileData;
	[Prop] int[] _tileDataPalette = new int[PaletteIndex.MaxValue];

	public int[,] tilemapData;

	public void Unpack()
	{
		var w = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				tilemapData[x, y] = _tileDataPalette[_tileData[w]];
				w++;
			}
		}
	}

	public void Pack()
	{
		PaletteIndex GetTileFromPallet(int id)
		{
			if (id == 0)
			{
				return 0;
			}

			for (PaletteIndex i = 1; i < PaletteIndex.MaxValue; i++)
			{
				// Tile is already in the palette
				var lutTileId = _tileDataPalette[i];
				if (lutTileId == id)
				{
					return i;
				}

				// Add the tile to the palette
				if (lutTileId == 0)
				{
					_tileDataPalette[i] = id;
					return i;
				}
			}

			throw new Exception("Ran out of space for tiles in stage tile palette");
		}

		var w = -1;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				w++;

				var tileId = tilemapData[x, y];
				var tileKey = GetTileFromPallet(tileId);
				_tileData[w] = tileKey;
			}
		}
	}

	protected override void OnEnterScene()
	{
		base.OnEnterScene();

		Spawn();
	}

	public void Spawn()
	{
		var tilemapsNode = new Node()
		{
			name = "Tilemaps",
		};

		for (int i = 1; i < PaletteIndex.MaxValue; i++)
		{
			var tileKey = _tileData[i];

			// Reached end?
			if (tileKey == 0) break;

			var stageTilemap = new StageTilemap(this, tileKey);

			tilemapsNode.AddChild(stageTilemap);
		}

		this.AddChild(tilemapsNode);
	}
}
