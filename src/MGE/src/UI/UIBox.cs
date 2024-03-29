using System;
using System.Collections.Generic;

namespace MGE.UI;

public class UIBox : UIContainer
{
	[Prop] UIFlow _direction;
	public UIFlow direction
	{
		get => _direction;
		set
		{
			if (_direction == value) return;
			_direction = value;

			UpdateLayout(0);
			UpdateLayout(1);
		}
	}

	[Prop] Vector2<UIAlignment> _alignment;
	public Vector2<UIAlignment> alignment
	{
		get => _alignment;
		set
		{
			for (int i = 0; i < 2; i++)
			{
				if (_alignment[i] != value[i])
				{
					_alignment[i] = value[i];
					UpdateLayout(i);
				}
			}
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

			UpdateLayout((int)_direction);
		}
	}

	protected override void OnChildMeasureChanged(int dir, UIWidget widget)
	{
		UpdateLayout(dir);
	}

	protected override void OnMeasureChanged(int dir)
	{
		UpdateLayout(dir);
	}

	internal void UpdateLayout(int dir)
	{
		// if (parent is null) return;
		if (children.Count == 0) return;

		// If this direction is the current flow direction...
		if (dir == (int)direction)
		{
			// ...then distribute the widgets
			UpdateLayout_Flow(dir);
		}
		else
		{
			// ...otherwise then position the widgets (and resize them if they need it)
			UpdateLayout_Block(dir);
		}
	}

	internal void UpdateLayout_Flow(int dir)
	{
		// If the sizing is hug...
		if (sizing[dir] == UISizing.Hug)
		{
			// ...then then there will not be remaining space so treat the sizing of fill widgets like fix and ignore alignment
			UpdateLayout_Flow_Hug(dir);
		}
		else
		{
			// ...otherwise then there will be remaining space so treat the fill widgets like fill and take alignment into account, as long as there are no fill widgets
			UpdateLayout_Flow_FixFill(dir);
		}
	}

	internal void UpdateLayout_Flow_Hug(int dir)
	{
		// Loop over all widgets and position them
		var usedSpace = 0; // the space used by non fill widgets, will be used for sizing container
		var position = padding[dir]; // an offset/counter to position the widgets
		foreach (var child in children)
		{
			// Set the widget's position
			child.SetPosition(dir, position);

			var widgetSize = 0;

			switch (child.sizing[dir])
			{
				// If the widget is hug then use its calculated size
				case UISizing.Hug:
					widgetSize = child.actualSize[dir];
					break;
				// If the widget fix then use its fixed size
				case UISizing.Fix:
					widgetSize = child.fixedSize[dir];
					// Update fixed size just in case
					child.SetSize(dir, child.fixedSize[dir]);
					break;
				// If the widget is fill then use its fixed size and update the widget to its fixed size
				case UISizing.Fill:
					// TODO  Try hug first, then fallback onto fix
					widgetSize = child.fixedSize[dir];
					// Update the fill widget to its fixed size
					child.SetSize(dir, widgetSize);
					break;
			}

			var stride = widgetSize + spacing;

			position += stride;
			usedSpace += stride;
		}

		if (usedSpace > 0)
		{
			// Remove the unnecessary extra spacing
			usedSpace -= spacing;
		}

		// Add padding
		usedSpace += padding.GetAlongAxis(dir);

		// Set size of box
		SetMySize(dir, usedSpace);
	}

	// Handles situations where there will be a fixed amount of space.
	// - The fill widgets will actually be treated like fill.
	// - There will be remaining space so aligning the widgets needs to be taken into account
	internal void UpdateLayout_Flow_FixFill(int dir)
	{
		var fillWidgets = new List<UIWidget>(); // list of the fill widgets

		var usedSpace = 0;
		// Add space used by the spacing between children
		usedSpace += spacing * (children.Count - 1);

		// var remainingSpace = actualSize[dir] - padding.GetAlongAxis(dir); // remaining space used by widgets not including fill widgets, will be used for sizing fill widgets

		// Subtract space used by spacing
		// remainingSpace -= spacing * (children.Count - 1);

		// Find all fill widgets
		// Calculate the used space ignoring fill widgets (used to distribute the remaining space among the fill widgets later)
		foreach (var child in children)
		{
			// If the widget is fill...
			if (child.sizing[dir] == UISizing.Fill)
			{
				// ...then add it to the list of fill widgets
				fillWidgets.Add(child);
			}
			else
			{
				// ...otherwise then allocate space for it (add it from the used space)
				// remainingSpace -= child.actualSize[dir];
				usedSpace += child.actualSize[dir];
			}
		}


		// Loop over all the widgets and position them
		// If there are filled widgets then ignore alignment
		var offset = fillWidgets.Count > 0 ? padding[dir] : alignment[dir] switch
		{
			UIAlignment.Start => padding[dir],
			UIAlignment.End => actualSize[dir] - padding[dir + 2] - usedSpace,
			UIAlignment.Center => (actualSize[dir] - usedSpace) / 2 + padding[dir] - padding[dir + 2],
			_ => throw new NotSupportedException($"{alignment} alignment not supported along the flow axis"),
		};
		var remainingSpace = actualSize[dir] - padding.GetAlongAxis(dir) - usedSpace;
		var fillWidgetSize = fillWidgets.Count == 0 ? 0 : Mathf.Max(remainingSpace / fillWidgets.Count, 0);

		foreach (var child in children)
		{
			// Position the widget
			child.SetPosition(dir, offset);

			// Update fix and fill widgets to their expected size
			switch (child.sizing[dir])
			{
				case UISizing.Fix:
					child.SetSize(dir, child.fixedSize[dir]);
					break;

				case UISizing.Fill:
					child.SetSize(dir, fillWidgetSize);
					break;
			}

			// Notify the widget that the parent changed measure
			// widget.ParentChangedMeasure();

			offset += child.actualSize[dir];
			offset += spacing;
		}
	}

	internal void UpdateLayout_Block(int dir)
	{
		// If the box is hug...
		if (sizing[dir] == UISizing.Hug)
		{
			// ...then loop over the widgets to find the desired size of the box...
			var largestWidgetSize = 0; // the size of the largest widget
			foreach (var child in children)
			{
				// ...set the widget's position...
				// child.SetPosition(dir, padding[dir]);

				// ...if the widget is fix or hug then or the container is hug (causes fill widgets to act like fix)...
				if (sizing[dir] == UISizing.Hug || child.sizing[dir] != UISizing.Fill)
				{
					var widgetSize = child.sizing[dir] switch
					{
						// If the widget is hug then use its calculated size
						UISizing.Hug => child.actualSize[dir],
						// If the widget is fill or fix then use its fixed size
						_ => child.fixedSize[dir],
					};

					// Update the largest size if the widget size is larger
					if (widgetSize > largestWidgetSize)
					{
						largestWidgetSize = widgetSize;
					}
				}
			}

			SetMySize(dir, largestWidgetSize + padding.GetAlongAxis(dir));
		}

		// Loop over all the widgets...
		foreach (var child in children)
		{
			// Update fix and fill widgets to their appropriate size
			switch (child.sizing[dir])
			{
				case UISizing.Fix:
					child.SetSize(dir, child.fixedSize[dir]);
					break;

				case UISizing.Fill:
					child.SetSize(dir, actualSize[dir] - padding.GetAlongAxis(dir));
					break;
			}

			// ...set the widget's position...
			var childPosition = alignment[dir] switch
			{
				UIAlignment.Start => 0,
				UIAlignment.Center => (actualSize[dir] - padding.GetAlongAxis(dir) - child.actualSize[dir]) / 2,
				UIAlignment.End => actualSize[dir] - padding.GetAlongAxis(dir) - child.actualSize[dir],
				_ => throw new NotSupportedException($"{alignment} alignment not supported along the block axis"),
			};
			childPosition += padding[dir];
			child.SetPosition(dir, childPosition);
		}
	}
}
