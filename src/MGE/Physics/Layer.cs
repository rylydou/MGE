using System;

namespace MGE;

public struct Layer
{
	public static readonly string[] layers =
	{
		"Layer 1",
		"Layer 2",
		"Layer 3",
		"Layer 4",
		"Layer 5",
		"Layer 6",
		"Layer 7",
		"Layer 8",
		"Layer 9",
		"Layer 10",
		"Layer 11",
		"Layer 12",
		"Layer 13",
		"Layer 14",
		"Layer 15",
		"Layer 16",

		"Layer 17",
		"Layer 18",
		"Layer 19",
		"Layer 20",
		"Layer 21",
		"Layer 22",
		"Layer 23",
		"Layer 24",
		"Layer 25",
		"Layer 26",
		"Layer 27",
		"Layer 28",
		"Layer 29",
		"Layer 30",
		"Layer 31",
		"Layer 32",
	};

	public static readonly uint[] powersOf2 =
	{
		1,
		2,
		4,
		8,
		16,
		32,
		64,
		128,
		256,
		512,
		1024,
		2048,
		4096,
		8192,
		16384,
		32768,

		65536,
		131072,
		262144,
		524288,
		1048576,
		2097152,
		4194304,
		8388608,
		16777216,
		33554432,
		67108864,
		134217728,
		268435456,
		536870912,
		1073741824,
		2147483648,
	};

	public int GetLayerIndex(string layer)
	{
		for (int i = 0; i < 32; i++)
		{
			if (layers[i].Equals(layer, StringComparison.OrdinalIgnoreCase))
				return i;
		}
		return -1;
	}

	public uint GetLayerBitmask(string layer)
	{
		return GetLayerBitmask(GetLayerIndex(layer));
	}

	public uint GetLayerBitmask(int index)
	{
		return powersOf2[index];
	}

	[Prop] uint data = 0;

	public Layer() { }

	public Layer(params int[] layers)
	{
		foreach (var layer in layers)
			AddLayer(layer);
	}

	public Layer(params string[] layers)
	{
		foreach (var layer in layers)
			AddLayer(layer);
	}

	public void AddLayer(int index) => data |= GetLayerBitmask(index);
	public void AddLayer(string layer) => data |= GetLayerBitmask(layer);
	public void AddLayer(Layer layers) => data |= layers.data;

	public void RemoveLayer(int index) => data &= ~GetLayerBitmask(index);
	public void RemoveLayer(string layer) => data &= ~GetLayerBitmask(layer);
	public void RemoveLayer(Layer layers) => data &= ~layers.data;

	public bool CheckLayer(int index) => (data & GetLayerBitmask(index)) != 0;
	public bool CheckLayer(string layer) => (data & GetLayerBitmask(layer)) != 0;
	public bool CheckLayer(Layer layers) => (data & layers.data) != 0;
}
