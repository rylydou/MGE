using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MGE.Graphics;

namespace MGE;

public class Font
{
	public static Font current = new(Folder.assets.GetFile("Fonts/Mono.png"), new(10, 24));

	public readonly Texture texture;
	public readonly Vector2Int charSize;
	public int spaceBtwChars = 2;

	Vector2Int charsCount;
	Dictionary<char, RectInt> chars;

	public Font(string path, Vector2Int charSize, ushort offset = 32)
	{
		this.texture = Texture.LoadFromFile(path);
		this.charSize = charSize;

		charsCount = texture.size / charSize;
		chars = new(charsCount.x * charsCount.y);

		var ch = offset;
		for (int y = 0; y < charsCount.y; y++)
		{
			for (int x = 0; x < charsCount.x; x++)
			{
				var rect = new Rect(x * charSize.x, y * charSize.y, charSize);
				var chr = (char)++ch;
				chars.Add(chr, rect);
			}
		}
	}

	public void DrawText(IEnumerable<char> text, Vector2 position) => DrawText(text, position, Color.white);
	public void DrawText(IEnumerable<char> text, Vector2 position, Color color)
	{
		var offset = 0;

		foreach (var ch in text)
		{
			if (!DrawChar(ch, new((charSize.x + spaceBtwChars) * offset + position.x, position.y), color)) continue;
			offset++;
		}
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="ch"></param>
	/// <param name="position"></param>
	/// <param name="color"></param>
	/// <returns>true if the char has width</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool DrawChar(char ch, Vector2 position, Color color)
	{
		if (ch == ' ') return true;
		if (!chars.TryGetValue(ch, out var charRegion))
		{
			Debug.Log($"Unknown Char: '{ch}' #{(ushort)ch}");
			return false;
		}

		GFX.DrawTextureRegionAtDest(texture, charRegion, new(position, charSize), color);
		return true;
	}
}
