namespace MGE;

public interface IFont
{
	int baseline { get; }
	int lineHeight { get; }

	bool TryGetGlyphMetrics(char ch, out GlyphMetrics glyph);
	bool TryGetKerning(char first, char second, out int amount);

	void RenderChar(Batch2D batch, char ch, float x, float y, float scale, Color color);

	void BeginRender(Batch2D batch, float fontSize, out float scale);
	void AfterRender(Batch2D batch);
}

public readonly struct GlyphMetrics
{
	public readonly int width;
	public readonly int advance;

	public GlyphMetrics(int width, int advance)
	{
		this.width = width;
		this.advance = advance;
	}
}
