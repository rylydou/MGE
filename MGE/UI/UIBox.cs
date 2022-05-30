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

	// TODO  Implement alignments
	// [Prop] Vector2<UIAlignment> _alignment;
	// public Vector2<UIAlignment> alignment
	// {
	// 	get => _alignment;
	// 	set
	// 	{
	// 		throw new System.NotImplementedException();

	// 		if (_alignment == value) return;
	// 		_alignment = value;

	// 		UpdateLayout();
	// 	}
	// }

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
		if (widgets.Count == 0) return;

		// If this direction is the current flow direction...
		if (dir == (int)direction)
		{
			// ...then distribute the widgets
			// If the sizing is hug...
			if (sizing[dir] == UISizing.Hug)
			{
				// ...then then there will not be remaining space so treat the sizing of fill widgets like fix

				// Loop over all widgets and position them
				var usedSpace = 0; // the space used by non fill widgets, will be used for sizing container
				var position = padding[dir]; // an offset/counter to position the widgets
				foreach (var widget in widgets)
				{
					// Set the widget's position
					widget.SetPosition(dir, position);

					var widgetSize = 0;

					switch (widget.sizing[dir])
					{
						// If the widget is hug then use its calculated size
						case UISizing.Hug:
							widgetSize = widget.actualSize[dir];
							break;
						// If the widget fix then use its fixed size
						case UISizing.Fix:
							widgetSize = widget.fixedSize[dir];
							break;
						// If the widget is fill then use its fixed size and update the widget to its fixed size
						case UISizing.Fill:
							widgetSize = widget.fixedSize[dir];

							// TODO  Try hug first, then fallback onto fix
							// Update the fill widget to it's fix size
							widget.SetSize(dir, widgetSize);
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
			else
			{
				// ...otherwise then there will be remaining space so treat the fill widgets like fill

				var fillWidgets = new List<UIWidget>(); // list of the fill widgets
				var remainingSpace = actualSize[dir] - padding.GetAlongAxis(dir); // remaining space used by widgets not including fill widgets, will be used for sizing fill widgets

				// Subtract space used by spacing
				remainingSpace -= spacing * (widgets.Count - 1);

				// Find all fill widgets
				// Calculate the remaining space not ignoring fill widgets (used to distribute space among the fill widgets later)
				foreach (var widget in widgets)
				{
					// If the widget is fill...
					if (widget.sizing[dir] == UISizing.Fill)
					{
						// ...then add it to the list of fill widgets
						fillWidgets.Add(widget);
					}
					else
					{
						// ...otherwise then allocate space for it (subtract it from the remaining space)
						remainingSpace -= widget.actualSize[dir];
					}
				}

				// Loop over all the widgets and position them
				var pos = padding[dir]; // an offset/counter to position the widgets
				var fillWidgetSize = fillWidgets.Count == 0 ? 0 : Mathf.Max(remainingSpace / fillWidgets.Count, 0);
				foreach (var widget in widgets)
				{
					// Position the widget
					widget.SetPosition(dir, pos);

					// If the widget fill..
					if (widget.sizing[dir] == UISizing.Fill)
					{
						// ...then set the size of the widget
						widget.SetSize(dir, fillWidgetSize);
					}

					// Notify the widget that the parent changed measure
					// widget.ParentChangedMeasure();

					pos += widget.actualSize[dir];
					pos += spacing;
				}
			}
		}
		else
		{
			// ...otherwise then position the widgets

			// If the box is hug...
			if (sizing[dir] == UISizing.Hug)
			{
				// ...then loop over the widgets to find the desired size of the box...
				var largestWidgetSize = 0; // the size of the largest widget
				foreach (var widget in widgets)
				{
					// ...set the widget's position...
					widget.SetPosition(dir, padding[dir]);

					// ...if the widget is fix or hug then or the container is hug (causes fill widgets to act like fix)...
					if (sizing[dir] == UISizing.Hug || widget.sizing[dir] != UISizing.Fill)
					{
						var widgetSize = widget.sizing[dir] switch
						{
							// If the widget is hug then use its calculated size
							UISizing.Hug => widget.actualSize[dir],
							// If the widget is fill or fix then use its fixed size
							_ => widget.fixedSize[dir],
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
			foreach (var widget in widgets)
			{
				// ...set the widget's position...
				widget.SetPosition(dir, padding[dir]);

				// ...if the widget is fill...
				if (widget.sizing[dir] == UISizing.Fill)
				{
					// ...then make it fill the widget
					widget.SetSize(dir, actualSize[dir] - padding.GetAlongAxis(dir));
				}
			}
		}
	}
}
