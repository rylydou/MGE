#nullable disable

using System.Runtime.CompilerServices;
using PaletteIndex = System.Byte;

namespace Game;

public class Stage : Node
{
	public const float TILE_SIZE = 8f;

	[Prop] public int width = 80;
	[Prop] public int height = 45;

	[Prop] PaletteIndex[] _tileData = new PaletteIndex[80 * 45];
	[Prop] int[] _tileDataPalette = new int[PaletteIndex.MaxValue];

	#region Tilemap

	int[,] _tilemap = new int[80, 45];

	Dictionary<int, StageTilemap> _instancedTilemaps = new();
	Dictionary<int, int> _tilemapTileStats = new();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetTile(int x, int y, int id)
	{
		var prevTileId = _tilemap[x, y];

		_tilemap[x, y] = id;

		if (id == 0)
		{
			if (prevTileId != 0)
			{
				_tilemapTileStats[prevTileId]--;
			}
		}
		else
		{
			// If the tilemap is not instanced then instance it into the scene
			if (!_instancedTilemaps.ContainsKey(id))
			{
				_tilemapTileStats.Add(id, 0);

				SpawnTilemap(id);
			}
			_tilemapTileStats[id]++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetTile(int x, int y)
	{
		return _tilemap[x, y];
	}

	public void UnpackTilemap()
	{
		var w = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				_tilemap[x, y] = _tileDataPalette[_tileData[w]];
				w++;
			}
		}
	}

	public void PackTilemap()
	{
		var w = -1;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				w++;

				var tileId = _tilemap[x, y];
				var tileKey = GetTileFromPallet(tileId);
				_tileData[w] = tileKey;
			}
		}
	}

	public PaletteIndex GetTileFromPallet(int id)
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

	#endregion Tilemap

	public Node tilemapsGroupNode;

	public Stage()
	{
		name = "Unnamed stage";

		tilemapsGroupNode = new Node()
		{
			name = "Tilemaps",
		};

		this.AddChild(tilemapsGroupNode);
	}

	public void Spawn()
	{
		// `int i = 1` to skip the first palette index that is always zero
		for (int i = 1; i < PaletteIndex.MaxValue; i++)
		{
			var id = _tileDataPalette[i];

			// Stop if reached end? palette layout ex: [ 0 # # # ... 0 0 0 ]
			if (id == 0) break;

			SpawnTilemap(id);
		}
	}

	void SpawnTilemap(int id)
	{
		Debug.Assert(id != 0, "Id cannot be zero");
		Debug.Assert(!_instancedTilemaps.ContainsKey(id), "Tilemap is already instanced");

		var tilemap = new StageTilemap(this, id);
		tilemapsGroupNode.AddChild(tilemap);

		_instancedTilemaps.Add(id, tilemap);
	}
}
