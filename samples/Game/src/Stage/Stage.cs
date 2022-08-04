#nullable disable

using System.Runtime.CompilerServices;
using PaletteIndex = System.Byte;

namespace Game;

public class Stage : Node
{
	public const float TILE_SIZE = 8f;

	[Save] public int width = 80;
	[Save] public int height = 45;

	[Save] PaletteIndex[] _tileData = new PaletteIndex[80 * 45];
	[Save] List<string> _tileDataPalette = new() { null };

	#region Tilemap

	string[,] _tilemap = new string[80, 45];

	Dictionary<string, StageTilemap> _instancedTilemaps = new();
	// Dictionary<int, int> _tilemapTileStats = new();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetTile(int x, int y, in string id)
	{
		var prevTileId = _tilemap[x, y];

		_tilemap[x, y] = id;

		if (id is null)
		{
			if (prevTileId is not null)
			{
				// _tilemapTileStats[prevTileId]--;
			}
		}
		else
		{
			// If the tilemap is not instanced then instance it into the scene
			if (!_instancedTilemaps.ContainsKey(id))
			{
				// _tilemapTileStats.Add(id, 0);

				SpawnTilemap(id);
			}
			// _tilemapTileStats[id]++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetTile(int x, int y)
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

	public PaletteIndex GetTileFromPallet(string id)
	{
		if (id is null)
		{
			return 0;
		}

		var index = _tileDataPalette.FindIndex(t => t == id);

		// Add the tile to the palette
		if (index == -1)
		{
			Debug.Assert(_tileDataPalette.Count < PaletteIndex.MaxValue, "Ran out of space for tiles in stage tile palette");

			_tileDataPalette.Add(id);
			return (PaletteIndex)(_tileDataPalette.Count - 1);
		}

		return (PaletteIndex)index;
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
		// var i = 0;
		foreach (var item in _tileDataPalette)
		{
			if (item is null) continue;

			SpawnTilemap(item);
			// i++;
		}
		// // `int i = 1` to skip the first palette index that is always zero
		// for (int i = 1; i < PaletteIndex.MaxValue; i++)
		// {
		// 	var id = _tileDataPalette[i];

		// 	// Stop if reached end? palette layout ex: [ 0 # # # ... 0 0 0 ]
		// }
	}

	void SpawnTilemap(string id)
	{
		Debug.Assert(id is not null, "Id cannot be null");
		Debug.Assert(!_instancedTilemaps.ContainsKey(id), "Tilemap is already instanced");

		var tilemap = new StageTilemap(this, id);
		tilemapsGroupNode.AddChild(tilemap);

		_instancedTilemaps.Add(id, tilemap);
	}
}
