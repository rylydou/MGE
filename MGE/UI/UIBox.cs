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

	UIHorizontalAlignment _horizontalAlignment;
	public UIHorizontalAlignment horizontalAlignment
	{
		get => _horizontalAlignment;
		set
		{
			if (_horizontalAlignment == value) return;
			_horizontalAlignment = value;

			UpdateLayout();
		}
	}

	UIVerticalAlignment _verticalAlignment;
	public UIVerticalAlignment verticalAlignment
	{
		get => _verticalAlignment;
		set
		{
			if (_verticalAlignment == value) return;
			_verticalAlignment = value;

			UpdateLayout();
		}
	}

	int _spacing;
	public int spacing
	{
		get => _spacing;
		set
		{
			if (_spacing == value) return;
			_spacing = value;

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
		if (widgets.Count == 0) return;

		if (horizontalResizing == UIResizing.HugContents) throw new System.NotImplementedException();

		// Horizontal
		{
			// Get filled widgets and remaining space
			var filledWidgets = new List<UIWidget>();
			var remainingSpace = contentRect.width;

			remainingSpace -= spacing * (widgets.Count - 1);

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

			var x = contentRect.x;
			foreach (var widget in widgets)
			{
				widget._rect.x = x;

				if (widget.horizontalResizing == UIResizing.FillContainer)
				{
					widget.RequestWidth(Math.Max(remainingSpace / filledWidgets.Count, 0));
				}

				x += widget._rect.width;
				x += spacing;
			}
		}
	}
}
