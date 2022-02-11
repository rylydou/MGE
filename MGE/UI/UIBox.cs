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

	Vec2<UIAlignment> _alignment;
	public Vec2<UIAlignment> alignment
	{
		get => _alignment;
		set
		{
			if (_alignment == value) return;
			_alignment = value;

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

	protected override void OnMeasureChanged()
	{
		base.OnMeasureChanged();

		UpdateLayout();
	}

	internal void UpdateLayout()
	{
		if (widgets.Count == 0) return;

		void _(int dir)
		{
			if (dir == (int)direction)
			{
				// Since this is the current direction then distrobute the space among all the widgets
				if (resizing[dir] == UIResizing.HugContents)
				{
					var usedSpace = 0;

					var pos = contentRect[dir];
					foreach (var widget in widgets)
					{
						widget._rect[dir] = pos;

						var offset = widget._rect[dir + 2] + spacing;
						pos += offset;
						usedSpace += offset;
					}

					fixedSize = fixedSize.With(dir, usedSpace);
				}
				else
				{
					// Get filled widgets and remaining space
					var filledWidgets = new List<UIWidget>();
					var remainingSpace = contentRect[dir + 2];

					remainingSpace -= spacing * (widgets.Count - 1);

					foreach (var widget in widgets)
					{
						if (widget.resizing[dir] == UIResizing.FillContainer)
						{
							filledWidgets.Add(widget);
						}
						else
						{
							remainingSpace -= widget.rect[dir + 2];
						}
					}

					var pos = contentRect[dir];
					foreach (var widget in widgets)
					{
						widget._rect[dir] = pos;

						if (widget.resizing[dir] == UIResizing.FillContainer)
						{
							widget._rect[dir + 2] = Math.Max(remainingSpace / filledWidgets.Count, 0);
						}

						widget.ParentChangedMeasure();

						pos += widget._rect[dir + 2];
						pos += spacing;
					}
				}
			}
			else
			{
				var size = 0;

				foreach (var widget in widgets)
				{
					widget._rect[dir] = contentRect[dir];

					if (widget._rect[dir + 2] > size)
					{
						size = widget._rect[dir + 2];
					}

					if (widget.resizing[dir] == UIResizing.FillContainer /* && resizing[dir] != UIResizing.HugContents */)
					{
						widget._rect[dir + 2] = contentRect[dir + 2];
					}

					widget.ParentChangedMeasure();
				}

				fixedSize = fixedSize.With(dir, size /* + padding.GetAxis(dir) */);
			}
		}

		_(0); // Horizontal
		_(1); // Vertical
	}
}
