using System.Collections.Generic;
using MGE.Graphics;

namespace MGE
{
	public class Font
	{
		public static Font current = new("Font.png", new(10, 24));

		public readonly Texture texture;
		public readonly Vector2Int charSize;
		public int spaceBtwChars = 2;

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

		public void DrawText(SpriteBatch sb, string text, Vector2 position)
		{
			var offset = 0;
			foreach (var ch in text)
			{
				if (char.IsWhiteSpace(ch)) { offset++; continue; }
				if (!chars.TryGetValue(ch, out var rect)) { Debug.Log($"Unknown Char: {ch} ({(ushort)ch})"); continue; }

				sb.DrawTextureRegion(texture, new((charSize.x + spaceBtwChars) * offset + position.x, position.y, charSize), rect);
				offset++;
			}
		}
	}
}
