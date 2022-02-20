using System;
using System.Collections.Generic;
using StbTrueTypeSharp;

namespace MGE
{
	/// <summary>
	/// Contains the Data of a Font at a given size
	/// </summary>
	public class FontSize
	{
		/// <summary>
		/// A single Font Character
		/// </summary>
		public class Character
		{
			/// <summary>
			/// The Unicode Value of the Character
			/// </summary>
			public char unicode;

			/// <summary>
			/// The Associated Glyph of the Character
			/// </summary>
			public int glyph;

			/// <summary>
			/// The Width (in pixels) of the Character
			/// </summary>
			public int width;

			/// <summary>
			/// The Height (in pixels) of the Character
			/// </summary>
			public int height;

			/// <summary>
			/// The Horizontal Advance (in pixels) of the Character
			/// </summary>
			public float advance;

			/// <summary>
			/// The X-Offset (in pixels) of the Character
			/// </summary>
			public float offsetX;

			/// <summary>
			/// The Y-Offset (in pixels) of the Character
			/// </summary>
			public float offsetY;

			/// <summary>
			/// Whether the Character has a Glyph. If not, this character cannot be rendered
			/// </summary>
			public bool hasGlyph;
		};

		/// <summary>
		/// The Font associated with this Font Size
		/// </summary>
		public readonly Font font;

		/// <summary>
		/// The Size of the Font
		/// </summary>
		public readonly int size;

		/// <summary>
		/// The Ascent of the Font. This is the Font.Ascent * our Scale
		/// </summary>
		public readonly float ascent;

		/// <summary>
		/// The Descent of the Font. This is the Font.Descent * our Scale
		/// </summary>
		public readonly float descent;

		/// <summary>
		/// The LineGap of the Font. This is the Font.LineGap * our Scale
		/// </summary>
		public readonly float lineGap;

		/// <summary>
		/// The Height of the Font. This is the Font.Height * our Scale
		/// </summary>
		public readonly float height;

		/// <summary>
		/// The LineHeight of the Font. This is the Font.LineHeight * our Scale
		/// </summary>
		public readonly float lineHeight;

		/// <summary>
		/// The Scale of the Font Size
		/// </summary>
		public readonly float scale;

		/// <summary>
		/// The Character Set of the Font Size
		/// </summary>
		public readonly Dictionary<char, Character> Charset = new Dictionary<char, Character>();

		public FontSize(Font font, int size, string charset)
		{
			if (font.disposed) throw new Exception("Cannot get Font data as it is disposed");

			this.font = font;
			this.size = size;
			scale = font.GetScale(size);
			ascent = font.ascent * scale;
			descent = font.descent * scale;
			lineGap = font.lineGap * scale;
			height = ascent - descent;
			lineHeight = height + lineGap;

			for (int i = 0; i < charset.Length; i++)
			{
				// get font info
				var unicode = charset[i];
				var glyph = font.GetGlyph(unicode);

				if (glyph > 0)
				{
					unsafe
					{
						int advance, offsetX, x0, y0, x1, y1;

						StbTrueType.stbtt_GetGlyphHMetrics(font.fontInfo, glyph, &advance, &offsetX);
						StbTrueType.stbtt_GetGlyphBitmapBox(font.fontInfo, glyph, scale, scale, &x0, &y0, &x1, &y1);

						int w = (x1 - x0);
						int h = (y1 - y0);

						// define character
						var ch = new Character
						{
							unicode = unicode,
							glyph = glyph,
							width = w,
							height = h,
							advance = advance * scale,
							offsetX = offsetX * scale,
							offsetY = y0
						};
						ch.hasGlyph = (w > 0 && h > 0 && StbTrueType.stbtt_IsGlyphEmpty(font.fontInfo, ch.glyph) == 0);
						Charset[unicode] = ch;
					}
				}
			}
		}

		/// <summary>
		/// Gets the Kerning Value between two Unicode characters at the Font Size, or 0 if there is no Kerning
		/// </summary>
		public float GetKerning(char unicode0, char unicode1)
		{
			if (Charset.TryGetValue(unicode0, out var char0) && Charset.TryGetValue(unicode1, out var char1))
			{
				if (font.disposed)
					throw new Exception("Cannot get Font data as it is disposed");

				return StbTrueType.stbtt_GetGlyphKernAdvance(font.fontInfo, char0.glyph, char1.glyph) * scale;
			}

			return 0f;
		}

		/// <summary>
		/// Renders the Unicode character to a Bitmap at the Font Size, or null if the character doesn't exist
		/// </summary>
		public Bitmap? Render(char unicode)
		{
			if (Charset.TryGetValue(unicode, out var ch) && ch.hasGlyph)
			{
				var bitmap = new Bitmap(ch.width, ch.height);

				Render(unicode, new Span<Color>(bitmap.pixels), out _, out _);

				return bitmap;
			}

			return null;
		}

		/// <summary>
		/// Renders the Unicode character to a buffer at the Font Size, and returns true on success
		/// </summary>
		public unsafe bool Render(char unicode, Span<Color> buffer, out int width, out int height)
		{
			if (Charset.TryGetValue(unicode, out var ch) && ch.hasGlyph)
			{
				if (buffer.Length < ch.width * ch.height)
					throw new Exception("Buffer provided isn't large enough to store rendered data");

				if (font.disposed)
					throw new Exception("Cannot get Font data as it is disposed");

				fixed (Color* ptr = buffer)
				{
					// we actually use the bitmap buffer as our temporary buffer, and fill the pixels out backwards after
					// kinda weird but it works & saves creating more memory

					var input = (byte*)ptr;
					StbTrueType.stbtt_MakeGlyphBitmap(font.fontInfo, input, ch.width, ch.height, ch.width, scale, scale, ch.glyph);

					for (int i = ch.width * ch.height - 1; i >= 0; i--)
						ptr[i] = new Color(input[i], input[i], input[i], input[i]);
				}

				width = ch.width;
				height = ch.height;
				return true;
			}

			width = height = 0;
			return false;
		}
	}
}
