using System.Collections.Generic;

namespace MGE.UI;

public class UIBox : UIContainer
{
	[Prop] UIDirection _direction;
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

	[Prop] Vector2<UIAlignment> _alignment;
	public Vector2<UIAlignment> alignment
	{
		get => _alignment;
		set
		{
			if (_alignment == value) return;
			_alignment = value;

			UpdateLayout();
		}
	}

	[Prop] int _spacing;
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
		// if (parent is null) return;
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

					usedSpace += padding.GetAxis(dir);

					if (_rect[dir + 2] != usedSpace)
					{
						_rect[dir + 2] = usedSpace;

						parent?.ChildMeasureChanged(this);
					}
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
							widget._rect[dir + 2] = Mathf.Max(remainingSpace / filledWidgets.Count, 0);
						}

						widget.ParentChangedMeasure();

						pos += widget._rect[dir + 2];
						pos += spacing;
					}
				}
			}
			else
			{
				var hugContentSize = 0;

				foreach (var widget in widgets)
				{
					widget._rect[dir] = contentRect[dir];

					if (widget.resizing[dir] == UIResizing.FillContainer)
					{
						widget._rect[dir + 2] = contentRect[dir + 2];
					}
					else if (widget._rect[dir + 2] > hugContentSize)
					{
						hugContentSize = widget._rect[dir + 2];
					}

					widget.ParentChangedMeasure();
				}

				if (resizing[dir] == UIResizing.HugContents)
				{
					hugContentSize += padding.GetAxis(dir);

					if (_rect[dir + 2] != hugContentSize)
					{
						_rect[dir + 2] = hugContentSize;

						parent?.ChildMeasureChanged(this);
					}
				}
			}
		}

		_(0); // Horizontal
		_(1); // Vertical
	}
}
