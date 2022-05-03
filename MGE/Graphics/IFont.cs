using System.Collections.Generic;

namespace MGE;

public interface IFont
{
	Vector2 MeasureString(IEnumerable<char> text, float fontSize);
}
