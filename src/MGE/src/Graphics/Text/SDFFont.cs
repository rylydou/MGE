using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MEML;

namespace MGE;

public class SDFFont : IFont
{
	readonly struct GlyphInfo
	{
		public readonly float x;
		public readonly float y;
		public readonly float width;
		public readonly float height;
		public readonly float xOffset;
		public readonly float yOffset;
		public readonly float advance;

		public GlyphInfo(MemlValue meml)
		{
			var atlasBounds = meml["atlasBounds"];
			var planeBounds = meml["planeBounds"];
			this.x = atlasBounds["left"];
			this.y = atlasBounds["top"];
			this.width = atlasBounds["right"] - atlasBounds["left"];
			this.height = atlasBounds["bottom"] - atlasBounds["top"];
			this.xOffset = planeBounds["left"];
			this.yOffset = planeBounds["top"];
			this.advance = meml["advance"];
		}
	}

	public readonly Texture _texture;
	public readonly Material _material;

	readonly float _lineHeight;
	public float lineHeight => _lineHeight;

	readonly float _ascender;
	public float ascender => _ascender;

	readonly float _descender;
	public float descender => _descender;

	readonly float _baseGlyphSize;

	readonly Dictionary<char, GlyphInfo> _glyphs = new();
	// readonly Dictionary<char, (int width, int advance)> _mertics = new();
	readonly Dictionary<(char first, char second), int> _kernings = new();

	public SDFFont(Texture texture, MemlValue def)
	{
		_texture = texture;
		_texture.filter = TextureFilter.Linear;

		var shader = new Shader(App.graphics.CreateShaderSourceMSDF());
		_material = new Material(shader);
		_material["u_pxRange"].SetFloat(def["atlas"]["distanceRange"]);

		_baseGlyphSize = def["atlas"]["size"];
		_lineHeight = def["metrics"]["lineHeight"] * _baseGlyphSize;
		_ascender = def["metrics"]["ascender"] * _baseGlyphSize;
		_descender = def["metrics"]["descender"] * _baseGlyphSize;

		foreach (var item in def["glyphs"].values)
		{
			var ch = (char)item["unicode"].@ushort;

			// _mertics.Add((char)ch.@ushort, (width: item["width"], advance: item["xadvance"]));

			var glyph = new GlyphInfo(item);
			_glyphs.Add(ch, glyph);
		}

		// foreach (var item in def["kernings"].values)
		// {
		// 	_kernings.Add((item["first"], item["second"]), item["amount"]);
		// }
	}

	public bool TryGetGlyphMetrics(char ch, [MaybeNullWhen(false)] out float width, out float advance)
	{
		if (_glyphs.TryGetValue(ch, out var glyph))
		{
			width = glyph.width;
			advance = glyph.advance * _baseGlyphSize;
			return true;
		}

		width = 0;
		advance = 0;
		return false;
	}

	public bool TryGetKerning(char first, char second, out float amount)
	{
		amount = 0;
		return false;
		// return _kernings.TryGetValue((first, second), out amount);
	}

	public float GetScale(float fontSize) => fontSize / _baseGlyphSize;

	public void BeginRender(Batch2D batch)
	{
		batch.SetMaterial(_material);
		// batch.SetTexture(_texture);
	}

	public void AfterRender(Batch2D batch)
	{
		batch.SetMaterial(null);
	}

	public void RenderChar(Batch2D batch, char ch, float x, float y, float scale, Color color)
	{
		var glyph = _glyphs[ch];

		var clip = new Rect(glyph.x, glyph.y, glyph.width, glyph.height);

		var xx = glyph.xOffset * _baseGlyphSize * scale;
		var yy = glyph.yOffset * _baseGlyphSize * scale;

		var pos = new Vector2(x + xx, y + yy);
		var rect = new Rect(pos.x, pos.y + _baseGlyphSize * scale, glyph.width * scale, glyph.height * scale);

		batch.DrawImage(_texture, clip, rect, color);
		// batch.SetBox(x, y, 4, 4, color.contrastColor);
		// batch.SetRect(rect, 1, color);
	}
}
