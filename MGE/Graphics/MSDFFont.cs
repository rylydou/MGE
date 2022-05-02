using System;
using System.Linq;

namespace MGE;

public class MSDFFontDef
{
	public class Char
	{
		[Save] public int id;
		[Save] public int index;
		[Save] public string c = "";
		[Save] public int width;
		[Save] public int height;
		[Save] public int xoffset;
		[Save] public int yoffset;
		[Save] public int xadvance;
		[Save] public int chnl;
		[Save] public int x;
		[Save] public int y;
		[Save] public int page;
	}

	public class Info
	{
		[Save] public string face = "";
		[Save] public int size;
		[Save] public int bold;
		[Save] public int italic;
		[Save] public string[] charset = Array.Empty<string>();
		[Save] public int unicode;
		[Save] public int stretchH;
		[Save] public int smooth;
		[Save] public int aa;
		[Save] public RectInt padding;
		[Save] public Vector2Int spacing;
	}

	public class Common
	{
		[Save] public int lineHeight;
		[Save] public int b;
		[Save] public int scaleW;
		[Save] public int scaleH;
		[Save] public int pages;
		[Save] public int packed;
		[Save] public int alphaChnl;
		[Save] public int redChnl;
		[Save] public int greenChnl;
		[Save] public int blueChnl;
	}

	public class DistField
	{
		[Save] public string fieldType = "";
		[Save] public int distanceRange;
	}

	public class Kernings
	{
		[Save] public int first;
		[Save] public int second;
		[Save] public int amount;
	}

	[Save] public string[] pages = Array.Empty<string>();

	[Save] public Char[] chars = Array.Empty<Char>();
	[Save] public Info info = new();
	[Save] public Common common = new();
	[Save] public DistField distanceField = new();
	[Save] public Kernings[] kernings = Array.Empty<Kernings>();
}

public class MSDFFont
{
	public Texture texture;
	public MSDFFontDef def;

	public MSDFFont(Texture texture, MSDFFontDef def)
	{
		this.texture = texture;
		this.def = def;
	}

	public void DrawString(Batch2D batch, string text, Vector2 position, int scale)
	{
		var x = 0;

		for (int i = 0; i < text.Length; i++)
		{
			var ch = text[i];
			if (!char.IsLetterOrDigit(ch)) continue;

			var id = (int)ch;

			var glyph = def.chars.First(c => c.id == id);

			var rect = new RectInt(glyph.x, glyph.y, glyph.width, glyph.height);

			batch.Image(texture, rect, new(position.x + x, position.y), new(scale), Vector2.zero, 0, Color.white);

			x += rect.width;
		}
	}
}
