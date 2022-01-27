using System.Collections.Generic;

namespace MGE.UI;

public class UIFrame : UIContainer
{
	List<UIWidget> _widgets = new();
	public List<UIWidget> widgets
	{
		get => _widgets;
		set
		{
			if (_widgets == value) return;
			_widgets = value;

			UpdateLayout();
		}
	}

	public override void UpdateLayout()
	{
		foreach (var widget in _widgets)
		{
		}
	}
}
