using FontStashSharp;
using FontStashSharp.Interfaces;
using MGE.Graphics;

namespace MGE;

public class Font
{
	// class Texture2DManager : ITexture2DManager
	// {
	// 	public object CreateTexture(int width, int height) => new Texture(new Vector2Int(width, height));

	// 	public System.Drawing.Point GetTextureSize(object obj) => ((Texture)obj).size;

	// 	public void SetTextureData(object obj, System.Drawing.Rectangle bounds, byte[] data) => ((Texture)obj).SetData(bounds, data);
	// }

	// class FontStashRenderer : IFontStashRenderer
	// {
	// 	ITexture2DManager textureManager = new Texture2DManager();
	// 	public ITexture2DManager TextureManager => textureManager;

	// 	public void Draw(object texture, System.Numerics.Vector2 pos, System.Drawing.Rectangle? src, System.Drawing.Color color, float rotation, System.Numerics.Vector2 origin, System.Numerics.Vector2 scale, float depth)
	// 	{
	// 		if (rotation == 0)
	// 		{
	// 			// Debug.Log($"pos:{pos} src:{src} origin:{origin} realPos:{pos - origin}");
	// 			GFX.DrawTextureRegionScaled((Texture)texture, src.HasValue ? src.Value : throw new MGEException(), pos - origin, scale, color);
	// 			GFX.DrawRect(new(pos - origin, src.Value.Size.Width * scale.X, src.Value.Size.Height * scale.Y), Color.white);
	// 		}
	// 		else
	// 		{
	// 			throw new MGEException();
	// 			// GFX.DrawTextureRegionScaledAndRotated((Texture)texture, src, pos, scale, rotation, color);
	// 		}
	// 	}
	// }

	// public readonly static Font defaultFont = new(Folder.assets.GetFile("Fonts/Inter/Inter Regular.ttf"));

	// FontSystem fontSystem;
	// FontStashRenderer renderer = new();

	// public Font(File fontFile)
	// {
	// 	fontSystem = new();
	// 	fontSystem.AddFont(fontFile.ReadBytes());
	// }

	// public void DrawString(string text, Vector2 position, int fontSize, Color color)
	// {
	// 	var font = fontSystem.GetFont(fontSize);

	// 	font.DrawText(renderer, text, position, color);
	// }

	/* public static Font current = new("Font.png", new(10, 24));

	public readonly Texture texture;
	public readonly Vector2Int charSize;
	public int spaceBtwChars = 1;

	Vector2Int charsCount;
	Dictionary<char, RectInt> chars;

	public Font(string path, Vector2Int charSize, ushort offset = 32)
	{
		this.texture = Texture.LoadTexture(path);
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
		if (!chars.TryGetValue(ch, out var rect))
		{
			Debug.Log($"Unknown Char: '{ch}' #{(ushort)ch}");
			return false;
		}

		GFX.DrawTextureRegionAtDest(texture, new(position, charSize), rect, color);
		return true;
	} */
}
