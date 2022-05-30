using System;
using System.Collections.Generic;

namespace MGE.UI;

public class UIStyle
{
	public readonly UIStyle? parentStyle;

	[Prop, Save] Dictionary<string, Color> _styles = new();

	public UIStyle()
	{
		this.parentStyle = null;
	}

	UIStyle(UIStyle parentStyle)
	{
		this.parentStyle = parentStyle;
	}

	public bool SetStyle(string name, Color value)
	{
		return _styles.Set(name, value);
	}

	public Color GetStyle(string name)
	{
		if (!_styles.TryGetValue(name, out var value)) return value;

		if (parentStyle is null) throw new MGException("Style not found");

		return parentStyle.GetStyle(name);
	}

	public UIStyle CreateVariant()
	{
		return new UIStyle(this);
	}
}
