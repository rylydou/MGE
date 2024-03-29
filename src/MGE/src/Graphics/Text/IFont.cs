using System;
using System.Collections.Generic;
using System.Linq;

namespace MGE;

public enum TextAlignment
{
	Left,
	Right,
	Center,
}

public interface IFont
{
	// float baseline { get; }
	float lineHeight { get; }
	float ascender { get; }
	float descender { get; }

	bool TryGetGlyphMetrics(char ch, out float width, out float advance);
	bool TryGetKerning(char first, char second, out float amount);
	float GetScale(float fontSize);

	void RenderChar(Batch2D batch, char ch, float x, float y, float scale, Color color);

	void BeginRender(Batch2D batch);
	void AfterRender(Batch2D batch);

	public StringBuildInfo BuildString(ReadOnlySpan<char> text)
	{
		var str = new StringBuildInfo();
		var line = new LineBuildInfo();
		str.lines.Add(line);

		var prevCh = '\0';
		var nextAdvance = 0f;

		foreach (var ch in text)
		{
			if (ch == '\n')
			{
				line = new LineBuildInfo();
				str.lines.Add(line);

				prevCh = '\0';
				nextAdvance = 0;

				continue;
			}

			line.width += nextAdvance;

			if (!TryGetGlyphMetrics(ch, out var width, out var advance)) continue;

			// If the character is whitespace then don't worry about kerning or adding it to the list
			if (char.IsWhiteSpace(ch))
			{
				prevCh = '\0';
				nextAdvance = advance; // Don't forget about advancing
				continue;
			}

			if (TryGetKerning(prevCh, ch, out var amount)) line.width += amount;

			line.chars.Add(new(ch, line.width));
			line.width += width;

			prevCh = ch;
			nextAdvance = advance - width;
		}

		str.width = str.lines.Max(l => l.width);

		return str;
	}
}

public class StringBuildInfo
{
	public float width;

	public List<LineBuildInfo> lines = new();

	public StringBuildInfo() { }
}

public class LineBuildInfo
{
	public float width;

	public List<GlyphBuildInfo> chars = new();

	public LineBuildInfo() { }
}

public class GlyphBuildInfo
{
	public char ch;
	public float x;

	public GlyphBuildInfo(char ch, float x)
	{
		this.ch = ch;
		this.x = x;
	}
}
