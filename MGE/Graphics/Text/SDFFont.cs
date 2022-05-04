using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MEML;

namespace MGE;

public class SDFFont : IFont
{
	readonly struct GlyphInfo
	{
		public readonly int x;
		public readonly int y;
		public readonly int width;
		public readonly int height;
		public readonly int xOffset;
		public readonly int yOffset;

		public GlyphInfo(StructureValue value)
		{
			this.x = value["x"];
			this.y = value["y"];
			this.width = value["width"];
			this.height = value["height"];
			this.xOffset = value["xoffset"];
			this.yOffset = value["yoffset"];
		}
	}

	readonly Texture _texture;
	readonly Material _material;

	readonly int _baseline;
	public int baseline => _baseline;

	readonly int _lineHeight;
	public int lineHeight => _lineHeight;

	readonly int _baseGlyphSize;

	readonly Dictionary<char, GlyphInfo> _glyphs = new();
	readonly Dictionary<char, GlyphMetrics> _mertics = new();
	readonly Dictionary<(char first, char second), int> _kernings = new();

	public SDFFont(Texture texture, StructureValue def)
	{
		this._texture = texture;

		var shader = new Shader(App.graphics.CreateShaderSourceMSDF());
		_material = new Material(shader);
		_material["u_pxRange"].SetFloat(def["distanceField"]["distanceRange"]);

		_baseline = def["common"]["base"];
		_lineHeight = def["common"]["lineHeight"];
		_baseGlyphSize = def["info"]["size"];

		foreach (var item in def["chars"].values)
		{
			var ch = item["char"];

			var metric = new GlyphMetrics(
				width: item["width"],
				advance: item["xadvance"]
			);
			_mertics.Add(ch, metric);

			var glyph = new GlyphInfo(item);
			_glyphs.Add(ch, glyph);
		}

		foreach (var item in def["kernings"].values)
		{
			_kernings.Add((item["first"], item["second"]), item["amount"]);
		}
	}

	public bool TryGetGlyphMetrics(char ch, [MaybeNullWhen(false)] out GlyphMetrics metric)
	{
		return _mertics.TryGetValue(ch, out metric);
	}

	public bool TryGetKerning(char first, char second, out int amount)
	{
		return _kernings.TryGetValue((first, second), out amount);
	}

	public void BeginRender(Batch2D batch, float fontSize, out float scale)
	{
		batch.SetMaterial(_material);
		// batch.SetTexture(_texture);

		scale = fontSize / _baseGlyphSize;
	}

	public void AfterRender(Batch2D batch)
	{
		batch.SetMaterial(batch.defaultMaterial);
	}

	public void RenderChar(Batch2D batch, char ch, float x, float y, float scale, Color color)
	{
		var glyph = _glyphs[ch];

		var clip = new RectInt(glyph.x, glyph.y, glyph.width, glyph.height);

		var xx = glyph.xOffset * scale;
		var yy = glyph.yOffset * scale;

		var pos = new Vector2(x + xx, y + yy);
		var rect = new Rect(pos.x, pos.y, glyph.width * scale, glyph.height * scale);

		batch.Image(_texture, clip, rect, color);
	}
}
