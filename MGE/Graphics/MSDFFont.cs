using System;
using System.Collections.Generic;
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
		[Save] public int baseline;
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
	static Material msdfMat;

	static MSDFFont()
	{
		var shader = new Shader(App.graphics.CreateShaderSourceMSDF());
		msdfMat = new Material(shader);
		msdfMat["u_pxRange"].SetFloat(4f);
	}

	public Texture texture;
	public MSDFFontDef def;

	public MSDFFont(Texture texture, MSDFFontDef def)
	{
		this.texture = texture;
		this.def = def;
	}

	public void DrawString(Batch2D batch, IEnumerable<char> text, Vector2 position, Color color, float fontSize)
	{
		batch.SetMaterial(msdfMat);

		var scale = fontSize / def.info.size;

		var x = 0;
		var y = 0;

		var prevChar = ' ';
		foreach (var ch in text)
		{
			if (char.IsControl(ch))
			{
				switch (ch)
				{
					case '\n':
						x = 0;
						y += def.common.lineHeight;
						break;
				}
			}
			else
			{
				var glyph = def.chars.FirstOrDefault(c => c.id == ch);
				var unknown = false;
				if (glyph is null)
				{
					unknown = true;
					glyph = def.chars[0];
				}

				var kerning = def.kernings.FirstOrDefault(k => k.first == prevChar && k.second == ch);
				if (kerning is not null)
				{
					x += kerning.amount;
				}

				var clip = new RectInt(glyph.x, glyph.y, glyph.width, glyph.height);

				var xx = glyph.xoffset;
				var yy = glyph.yoffset;

				var pos = new Vector2(position.x + (x + xx) * scale, position.y + (y + yy) * scale);
				var rect = new Rect(pos.x, pos.y, glyph.width * scale, glyph.height * scale);

				if (unknown) batch.HollowRect(rect, 2, color);
				else batch.Image(texture, clip, rect, color);

				x += glyph.xadvance;
			}

			prevChar = ch;
		}

		batch.SetMaterial(batch.defaultMaterial);
	}
}
