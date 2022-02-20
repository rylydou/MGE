using System.Collections.Generic;

namespace MGE;

static class Extentions
{
	public static byte[] ToRGBA(this Color[] pixels)
	{
		var length = pixels.Length;
		var data = new byte[length * 4];

		for (int i = 0; i < length; i++)
		{
			var j = i * 4;
			var col = pixels[i];
			data[j + 0] = col.r;
			data[j + 1] = col.g;
			data[j + 2] = col.b;
			data[j + 3] = col.a;
		}

		return data;
	}

	public static Color[] ToRGBA(this byte[] data)
	{
		var dataLength = data.Length / 4;
		var pixels = new Color[dataLength];

		for (int i = 0; i < dataLength; i++)
		{
			var j = i * 4;
			pixels[i].r = data[j + 0];
			pixels[i].g = data[j + 1];
			pixels[i].b = data[j + 2];
			pixels[i].a = data[j + 3];
		}

		return pixels;
	}

	public static bool Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) where TKey : notnull
	{
		if (!dict.TryAdd(key, value))
		{
			dict[key] = value;
			return false;
		}
		return true;
	}
}
