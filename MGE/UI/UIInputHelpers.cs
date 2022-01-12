using System;
using System.Collections.Generic;

namespace MGE.UI;

[Obsolete(null, true)]
internal static class UIInputHelpers
{
	[Obsolete]
	private static bool CommonTouchCheck(this UIWidget w)
	{
		return w.visible && w.active && w.enabled && w.containsClick;
	}

	[Obsolete]
	private static bool CommonMouseCheck(this UIWidget w)
	{
		return w.visible && w.active && w.enabled && w.containsMouse;
	}

	[Obsolete]
	public static bool FallsThrough(this UIWidget w, Vector2Int p)
	{
		// Only containers can fall through
		if (!(w is UIGrid || w is UIStackPanel || w is UIPanel || w is UISplitPane || w is UIScrollViewer)) return false;

		// Real containers are solid only if backround is set
		if (w.background is not null) return false;

		var asScrollViewer = w as UIScrollViewer;
		if (asScrollViewer is not null)
		{
			// Special case
			if (asScrollViewer._horizontalScrollingOn && asScrollViewer._horizontalScrollbarFrame.Contains(p) || asScrollViewer._verticalScrollingOn && asScrollViewer._verticalScrollbarFrame.Contains(p)) return false;
		}

		return true;
	}

	[Obsolete]
	public static void ProcessTouchDown(this List<UIWidget> widgets)
	{
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.CommonTouchCheck())
			{
				// Since OnTouchDown may reset Desktop, we need to save break condition before calling it
				var doBreak = !w.FallsThrough(UICanvas.clickPosition);
				w.OnClick();
				if (doBreak) break;
			}

			if (w.isModal) break;
		}
	}

	[Obsolete]
	public static void ProcessTouchUp(this List<UIWidget> widgets)
	{
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.isClicked)
			{
				w.OnTouchUp();
			}
		}
	}

	[Obsolete]
	public static void ProcessTouchDoubleClick(this List<UIWidget> widgets)
	{
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.CommonTouchCheck())
			{
				w.OnDoubleClick();
				if (w.desktop != null && !w.FallsThrough(w.desktop.clickPosition)) break;
			}

			if (w.isModal) break;
		}
	}

	[Obsolete]
	public static void ProcessMouseMovement(this List<UIWidget> widgets)
	{
		// First run: call on OnMouseLeft on all widgets if it is required
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];
			if (!w.containsMouse && w.isHoveredWithin)
			{
				w.OnMouseLeft();
			}
		}

		// Second run: OnMouseEnter/OnMouseMoved
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.CommonMouseCheck())
			{
				var isMouseOver = w.containsMouse;
				var wasMouseOver = w.isHoveredWithin;

				if (isMouseOver && !wasMouseOver)
				{
					w.OnMouseEntered();
				}

				if (isMouseOver && wasMouseOver)
				{
					w.OnMouseMoved();
				}

				if (!w.FallsThrough(UICanvas.mousePosition)) break;
			}

			if (w.isModal) break;
		}
	}

	[Obsolete]
	public static void ProcessDragMovement(this List<UIWidget> widgets)
	{
		// First run: call on OnTouchLeft on all widgets if it is required
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];
			if (!w.containsClick && w.isClicked)
			{
				w.OnClickLeft();
			}
		}

		// Second run: OnTouchEnter/OnTouchMoved
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.CommonTouchCheck())
			{
				var isTouchOver = w.containsClick;
				var wasTouchOver = w.isClicked;

				if (isTouchOver && !wasTouchOver)
				{
					w.OnDragStart();
				}

				if (isTouchOver && wasTouchOver)
				{
					w.OnDragMoved();
				}

				if (!w.FallsThrough(UICanvas.clickPosition)) break;
			}

			if (w.isModal) break;
		}
	}
}
