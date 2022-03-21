using System.Collections.Generic;

namespace MGE.UI;

public class UIStyleset
{
	class UIStyle
	{
		public Dictionary<string, Color> colors = new();
		public Dictionary<string, UIStyle> styles = new();
	}

	Dictionary<string, UIStyle> _styles = new();

	public Color GetColor(string name, string[] states)
	{
		if (!_styles.TryGetValue(name, out var style)) return Color.clear;

		var currentColor = Color.clear;

		if (style.colors.TryGetValue(name, out var color)) currentColor = color;
	}
}
