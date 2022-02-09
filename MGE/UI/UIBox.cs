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

					if (widget.resizing[dir] == UIResizing.FillContainer && resizing[dir] != UIResizing.HugContents)
					{
						widget._rect[dir + 2] = contentRect[dir + 2];
					}

					widget.ParentChangedMeasure();
				}

				fixedSize = fixedSize.With(dir, size);
			}
		}

		_(0);
		_(1);

		if (false)
		{
			// Horizontal
			{
				if (direction == UIDirection.Horizontal)
				{
					if (resizing.horizontal == UIResizing.HugContents)
					{
						var usedSpace = 0;

						var x = contentRect.x;
						foreach (var widget in widgets)
						{
							widget._rect.x = x;

							var offset = widget.rect.width + spacing;
							x += offset;
							usedSpace += offset;
						}

						fixedSize = new(usedSpace, fixedSize.y);
					}
					else
					{
						// Get filled widgets and remaining space
						var filledWidgets = new List<UIWidget>();
						var remainingSpace = contentRect.width;

						remainingSpace -= spacing * (widgets.Count - 1);

						foreach (var widget in widgets)
						{
							if (widget.resizing.horizontal == UIResizing.FillContainer)
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

							if (widget.resizing.horizontal == UIResizing.FillContainer)
							{
								widget._rect.width = Math.Max(remainingSpace / filledWidgets.Count, 0);
							}

							widget.ParentChangedMeasure();

							x += widget._rect.width;
							x += spacing;
						}
					}
				}
				else
				{
					var width = 0;

					foreach (var widget in widgets)
					{
						widget._rect.x = contentRect.x;

						if (widget._rect.width > width)
						{
							width = widget._rect.width;
						}

						if (widget.resizing.horizontal == UIResizing.FillContainer && resizing.horizontal != UIResizing.HugContents)
						{
							widget._rect.width = contentRect.width;
						}

						widget.ParentChangedMeasure();
					}

					fixedSize = new(width, fixedSize.y);
				}
			}

			// Vertical
			{
				if (direction == UIDirection.Vertical)
				{
					if (resizing.vertical == UIResizing.HugContents)
					{
						var usedSpace = 0;

						var y = contentRect.y;
						foreach (var widget in widgets)
						{
							widget._rect.y = y;

							var offset = widget.rect.height + spacing;
							y += offset;
							usedSpace += offset;
						}

						fixedSize = new(fixedSize.x, usedSpace);
					}
					else
					{
						// Get filled widgets and remaining space
						var filledWidgets = new List<UIWidget>();
						var remainingSpace = contentRect.height;

						remainingSpace -= spacing * (widgets.Count - 1);

						foreach (var widget in widgets)
						{
							if (widget.resizing.vertical == UIResizing.FillContainer)
							{
								filledWidgets.Add(widget);
							}
							else
							{
								remainingSpace -= widget.rect.height;
							}
						}

						var y = contentRect.y;
						foreach (var widget in widgets)
						{
							widget._rect.y = y;

							if (widget.resizing.vertical == UIResizing.FillContainer)
							{
								widget._rect.height = Math.Max(remainingSpace / filledWidgets.Count, 0);
							}

							widget.ParentChangedMeasure();

							y += widget._rect.height;
							y += spacing;
						}
					}
				}
				else
				{
					var height = 0;

					foreach (var widget in widgets)
					{
						widget._rect.y = contentRect.y;

						if (widget._rect.height > height)
						{
							height = widget._rect.height;
						}

						if (widget.resizing.vertical == UIResizing.FillContainer && resizing.vertical != UIResizing.HugContents)
						{
							widget._rect.height = contentRect.height;
						}

						widget.ParentChangedMeasure();
					}

					fixedSize = new(fixedSize.x, height);
				}
			}
		}
	}
}
