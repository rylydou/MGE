﻿// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Numerics;

// namespace MGE
// {
// 	/// <summary>
// 	/// Sprite Font is a Font rendered to a Texture at a given size, which is useful for drawing Text with sprite batchers
// 	/// </summary>
// 	public class SpriteFont
// 	{
// 		/// <summary>
// 		/// A single Sprite Font Character
// 		/// </summary>
// 		public class Character
// 		{
// 			/// <summary>
// 			/// The Unicode Value
// 			/// </summary>
// 			public char unicode;

// 			/// <summary>
// 			/// The rendered Character Image
// 			/// </summary>
// 			public Subtexture image;

// 			/// <summary>
// 			/// The Offset to draw the Character at
// 			/// </summary>
// 			public Vector2 offset;

// 			/// <summary>
// 			/// The Amount to Advance the rendering by, horizontally
// 			/// </summary>
// 			public float advance;

// 			/// <summary>
// 			/// The Kerning value for following Characters
// 			/// </summary>
// 			public Dictionary<char, float> kerning = new Dictionary<char, float>();

// 			public Character(char unicode, Subtexture image, Vector2 offset, float advance)
// 			{
// 				this.unicode = unicode;
// 				this.image = image;
// 				this.offset = offset;
// 				this.advance = advance;
// 			}
// 		}

// 		/// <summary>
// 		/// A list of all the Characters in the Sprite Font
// 		/// </summary>
// 		public readonly Dictionary<char, Character> charset = new Dictionary<char, Character>();

// 		/// <summary>
// 		/// The Font Family Name
// 		/// </summary>
// 		public string familyName;

// 		/// <summary>
// 		/// The Font Style Name
// 		/// </summary>
// 		public string styleName;

// 		/// <summary>
// 		/// The Size of the Sprite Font
// 		/// </summary>
// 		public int size;

// 		/// <summary>
// 		/// The Font Ascent
// 		/// </summary>
// 		public float ascent;

// 		/// <summary>
// 		/// The Font Descent
// 		/// </summary>
// 		public float descent;

// 		/// <summary>
// 		/// The Line Gap of the Font. This is the vertical space between lines
// 		/// </summary>
// 		public float lineGap;

// 		/// <summary>
// 		/// The Height of the Font (Ascent - Descent)
// 		/// </summary>
// 		public float height;

// 		/// <summary>
// 		/// The Line Height of the Font (Height + LineGap). This is the total height of a single line, including the line gap
// 		/// </summary>
// 		public float lineHeight;

// 		public SpriteFont(string? familyName = null, string? styleName = null)
// 		{
// 			this.familyName = familyName ?? "Unknown";
// 			this.styleName = styleName ?? "Unknown";
// 		}

// 		public SpriteFont(File fontFile, int size, string charset, TextureFilter filter = TextureFilter.Linear) : this(new FontSize(new Font(fontFile), size, charset), filter)
// 		{

// 		}

// 		public SpriteFont(Stream stream, int size, string charset, TextureFilter filter = TextureFilter.Linear) : this(new FontSize(new Font(stream), size, charset), filter)
// 		{

// 		}

// 		public SpriteFont(Font font, int size, string charset, TextureFilter filter = TextureFilter.Linear) : this(new FontSize(font, size, charset), filter)
// 		{

// 		}

// 		public SpriteFont(FontSize fontSize, TextureFilter filter = TextureFilter.Linear)
// 		{
// 			familyName = fontSize.font.familyName;
// 			styleName = fontSize.font.styleName;
// 			size = fontSize.size;
// 			ascent = fontSize.ascent;
// 			descent = fontSize.descent;
// 			lineGap = fontSize.lineGap;
// 			height = fontSize.height;
// 			lineHeight = fontSize.lineHeight;

// 			var packer = new Packer();
// 			{
// 				var bufferSize = (fontSize.size * 2) * (fontSize.size * 2);
// 				var buffer = (bufferSize <= 16384 ? stackalloc Color[bufferSize] : new Color[bufferSize]);

// 				foreach (var ch in fontSize.Charset.Values)
// 				{
// 					var name = ch.unicode.ToString();

// 					// pack bmp
// 					if (fontSize.Render(ch.unicode, buffer, out int w, out int h))
// 						packer.AddPixels(name, w, h, buffer);

// 					// create character
// 					var sprChar = new Character(ch.unicode, new Subtexture(), new Vector2(ch.offsetX, ch.offsetY), ch.advance);
// 					charset.Add(ch.unicode, sprChar);

// 					// get all kerning
// 					foreach (var ch2 in fontSize.Charset.Values)
// 					{
// 						var kerning = fontSize.GetKerning(ch.unicode, ch2.unicode);
// 						if (Math.Abs(kerning) > 0.000001f)
// 							sprChar.kerning[ch2.unicode] = kerning;
// 					}
// 				}

// 				packer.Pack();
// 			}

// 			// link textures
// 			var output = packer.Pack();
// 			if (output is not null)
// 			{
// 				for (int i = 0; i < output.pages.Count; i++)
// 				{
// 					var texture = new Texture(output.pages[i]);
// 					texture.filter = filter;

// 					foreach (var entry in output.entries.Values)
// 					{
// 						if (entry.page != i)
// 							continue;

// 						if (charset.TryGetValue(entry.name[0], out var character))
// 							character.image.Reset(texture, entry.source, entry.frame);
// 					}

// 				}
// 			}
// 		}

// 		/// <summary>
// 		/// Measures the Width of the given text
// 		/// </summary>
// 		public float WidthOf(string text)
// 		{
// 			return WidthOf(text.AsSpan());
// 		}

// 		/// <summary>
// 		/// Measures the Width of the given text
// 		/// </summary>
// 		public float WidthOf(ReadOnlySpan<char> text)
// 		{
// 			var width = 0f;
// 			var line = 0f;

// 			for (int i = 0; i < text.Length; i++)
// 			{
// 				if (text[i] == '\n')
// 				{
// 					if (line > width)
// 						width = line;
// 					line = 0;
// 					continue;
// 				}

// 				if (!charset.TryGetValue(text[i], out var ch))
// 					continue;

// 				line += ch.advance;
// 			}

// 			return Math.Max(width, line);
// 		}

// 		public float HeightOf(string text)
// 		{
// 			if (string.IsNullOrEmpty(text))
// 				return 0;

// 			return HeightOf(text.AsSpan());
// 		}

// 		public float HeightOf(ReadOnlySpan<char> text)
// 		{
// 			if (text.Length <= 0)
// 				return 0;

// 			var height = this.height;

// 			for (int i = 0; i < text.Length; i++)
// 			{
// 				if (text[i] == '\n')
// 					height += lineHeight;
// 			}

// 			return height;
// 		}

// 		public Vector2 SizeOf(string text)
// 		{
// 			return new Vector2(WidthOf(text.AsSpan()), HeightOf(text.AsSpan()));
// 		}

// 		public Vector2 SizeOf(ReadOnlySpan<char> text)
// 		{
// 			return new Vector2(WidthOf(text), HeightOf(text));
// 		}
// 	}
// }
