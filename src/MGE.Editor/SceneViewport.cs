using Cairo;
using Gdk;
using Gtk;

namespace MGE.Editor
{
	public class SceneViewport : DrawingArea
	{
		protected override bool OnDrawn(Context cr)
		{
			const int CheckSize = 64;
			const int Spacing = 0;

			int i, j, xcount, ycount;

			// At the start of a draw handler, a clip region has been set on
			// the Cairo context, and the contents have been cleared to the
			// widget's background color.

			var alloc = Allocation;
			// Start redrawing the Checkerboard
			xcount = 0;
			i = Spacing;
			while (i < alloc.Width)
			{
				j = Spacing;
				ycount = xcount % 2; // start with even/odd depending on row
				while (j < alloc.Height)
				{
					if (ycount % 2 != 0)
						cr.SetSourceRGB(17f / 255, 17f / 255, 17f / 255);
					else
						cr.SetSourceRGB(21f / 255, 21f / 255, 21f / 255);
					// If we're outside the clip, this will do nothing.
					cr.Rectangle(i, j, CheckSize, CheckSize);
					cr.Fill();

					j += CheckSize + Spacing;
					++ycount;
				}
				i += CheckSize + Spacing;
				++xcount;
			}

			cr.SelectFontFace("Segoe UI Semibold", FontSlant.Normal, FontWeight.Normal);

			cr.MoveTo(32, 32);
			cr.SetFontSize(4);
			cr.ShowText("Hello World");

			// return true because we've handled this event, so no
			// further processing is required.

			return true;
		}
	}
}
