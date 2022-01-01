using System.Collections.Generic;

namespace MGE.UI;

internal static class UIInputHelpers
{
	private static bool CommonTouchCheck(this UIWidget w)
	{
		return w.Visible && w.Active && w.Enabled && w.ContainsTouch;
	}

	private static bool CommonMouseCheck(this UIWidget w)
	{
		return w.Visible && w.Active && w.Enabled && w.ContainsMouse;
	}

	public static bool FallsThrough(this UIWidget w, Vector2Int p)
	{
		// Only containers can fall through
		if (!(w is UIGrid || w is UIStackPanel || w is UIPanel || w is UISplitPane || w is UIScrollViewer)) return false;

		// Real containers are solid only if backround is set
		if (w.Background is not null) return false;

		var asScrollViewer = w as UIScrollViewer;
		if (asScrollViewer is not null)
		{
			// Special case
			if (asScrollViewer._horizontalScrollingOn && asScrollViewer._horizontalScrollbarFrame.Contains(p) || asScrollViewer._verticalScrollingOn && asScrollViewer._verticalScrollbarFrame.Contains(p)) return false;
		}

		return true;
	}

	public static void ProcessTouchDown(this List<UIWidget> widgets)
	{
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.CommonTouchCheck())
			{
				// Since OnTouchDown may reset Desktop, we need to save break condition before calling it
				var doBreak = w.Desktop is not null && !w.FallsThrough(w.Desktop.TouchPosition);
				w.OnTouchDown();
				if (doBreak) break;
			}

			if (w.IsModal) break;
		}
	}

	public static void ProcessTouchUp(this List<UIWidget> widgets)
	{
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.IsTouchInside)
			{
				w.OnTouchUp();
			}
		}
	}

	public static void ProcessTouchDoubleClick(this List<UIWidget> widgets)
	{
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.CommonTouchCheck())
			{
				w.OnTouchDoubleClick();
				if (w.Desktop != null && !w.FallsThrough(w.Desktop.TouchPosition)) break;
			}

			if (w.IsModal) break;
		}
	}

	public static void ProcessMouseMovement(this List<UIWidget> widgets)
	{
		// First run: call on OnMouseLeft on all widgets if it is required
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];
			if (!w.ContainsMouse && w.IsMouseInside)
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
				var isMouseOver = w.ContainsMouse;
				var wasMouseOver = w.IsMouseInside;

				if (isMouseOver && !wasMouseOver)
				{
					w.OnMouseEntered();
				}

				if (isMouseOver && wasMouseOver)
				{
					w.OnMouseMoved();
				}

				if (w.Desktop != null && !w.FallsThrough(w.Desktop.MousePosition)) break;
			}

			if (w.IsModal) break;
		}
	}

	public static void ProcessTouchMovement(this List<UIWidget> widgets)
	{
		// First run: call on OnTouchLeft on all widgets if it is required
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];
			if (!w.ContainsTouch && w.IsTouchInside)
			{
				w.OnTouchLeft();
			}
		}

		// Second run: OnTouchEnter/OnTouchMoved
		for (var i = widgets.Count - 1; i >= 0; --i)
		{
			var w = widgets[i];

			if (w.CommonTouchCheck())
			{
				var isTouchOver = w.ContainsTouch;
				var wasTouchOver = w.IsTouchInside;

				if (isTouchOver && !wasTouchOver)
				{
					w.OnTouchEntered();
				}

				if (isTouchOver && wasTouchOver)
				{
					w.OnTouchMoved();
				}

				if (w.Desktop != null && !w.FallsThrough(w.Desktop.TouchPosition)) break;
			}

			if (w.IsModal) break;
		}
	}
}
