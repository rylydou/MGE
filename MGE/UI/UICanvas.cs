using System;

namespace MGE.UI;

public class UICanvas : UIBox
{
	public bool enableDebug = false;

	public UIWidget? hoveredWidget;

	public UIWidget? focusedWidget;
	public float focusTransitionDuration = 1f / 15;
	float _focusTransitionTime;
	Rect _previousFocusRect;

	public UICanvas()
	{
		canvas = this;
	}

	public void UpdateInputs(Vector2 mousePosition, Mouse mouse, Keyboard keyboard)
	{
		hoveredWidget = FindHoveredWidget(this, mousePosition);
	}

	static UIWidget? FindHoveredWidget(UIWidget widget, Vector2 mousePosition)
	{
		UIWidget? result = null;

		var current = widget;
		while (true)
		{
			// The the current widget is intractable...
			if (current.isIntractable)
			{
				// ...then it is the new result... until we find a more specific widget
				result = current;
			}

			if (current is UIBox box)
			{
				// Loop over the widgets children...
				foreach (var child in box.children)
				{
					// ..if the mouse is inside the widget...
					if (child.absoluteRect.Contains(mousePosition))
					{
						// ...then run this loop again checking this widget
						current = child;

						continue;
					}
				}
			}

			// The cursor is not hovering over any of its children so this is the hovered widget
			break;
		}

		return result;
	}

	public void Update(float delta)
	{
		_focusTransitionTime += delta;
	}

	public void RenderCanvas(Batch2D batch)
	{
		DoRender(batch);

		if (focusedWidget is not null)
		{
			var previousRect = _previousFocusRect.Expanded(3);
			var currentRect = focusedWidget.absoluteRect.Expanded(3);
			var time = _focusTransitionTime / focusTransitionDuration;
			var rect = Rect.LerpClamped(previousRect, currentRect, time);

			batch.SetRect(rect, 1, new(0x0A84FFFF));
		}
	}
}
