using System.Collections.Generic;

namespace MGE.UI;

public class UIBox : UIContainer
{
	UIDirection _direction;
	public UIDirection direction
	{
		get => _direction;
		set
		{
			if (_direction == value) return;
			_direction = value;

			UpdateLayout();
		}
	}

	protected override void OnChildMeasureChanged(UIWidget widget)
	{
		base.OnChildMeasureChanged(widget);

		UpdateLayout();
	}

	protected override void UpdateMeasure()
	{
		base.UpdateMeasure();

		UpdateLayout();
	}

	internal void UpdateLayout()
	{
		if (horizontalResizing == UIResizing.HugContents) throw new System.NotImplementedException();

		var filledWidgets = new List<UIWidget>();
		var remainingSpace = contentRect.width;

		foreach (var widget in widgets)
		{
			if (widget.horizontalResizing == UIResizing.FillContainer)
			{
				filledWidgets.Add(widget);
			}
			else
			{
				remainingSpace -= widget.rect.width;
			}
		}

		var filledWidgetSize = remainingSpace / filledWidgets.Count;

		foreach (var widget in filledWidgets)
		{
			widget.SetWidth(filledWidgetSize);
		}
	}
}
