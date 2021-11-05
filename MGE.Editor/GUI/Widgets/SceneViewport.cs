using System;
using Cairo;
using Gdk;
using Gtk;
using Key = Gdk.Key;

namespace MGE.Editor.GUI.Widgets
{
	public class SceneViewport : DrawingArea
	{
		public int x;
		public int y;

		public float scale = 64f;

		public SceneViewport()
		{
			this.CanFocus = true;
			this.FocusOnClick = true;
		}

		protected override bool OnDrawn(Context ctx)
		{
			Clear(ref ctx, 0.05f, 0.05f, 0.05f);

			// Grid Lines
			SetColor(ref ctx, 1f, 1f, 1f, 0.05f);

			// Horizontal
			for (int i = x; i < y + Allocation.Height * (1f / scale); i++)
			{
				DrawBox(ref ctx, 0, i, Allocation.Width, 0.1f);
			}

			// Vertical
			for (int i = y; i < y + Allocation.Width * (1f / scale); i++)
			{
				DrawBox(ref ctx, i, 0, 0.1f, Allocation.Height);
			}

			return true;
		}

		protected override bool OnScrollEvent(EventScroll evnt)
		{
			scale += (Math.Sign(evnt.DeltaY));

			return true;
		}

		protected override bool OnKeyPressEvent(EventKey evnt)
		{
			switch (evnt.Key)
			{
				case Key.A:
					x--;
					break;
				case Key.D:
					x++;
					break;
				case Key.W:
					y--;
					break;
				case Key.S:
					y++;
					break;
			}

			return true;
		}

		void SetColor(ref Context ctx, float r, float g, float b, float a = 1f) => ctx.SetSourceRGBA(r, g, b, a);
		void Clear(ref Context ctx, float r, float g, float b)
		{
			SetColor(ref ctx, r, g, b);
			ctx.Rectangle(0, 0, Allocation.Width, Allocation.Height);
			ctx.Fill();
		}
		void DrawBox(ref Context ctx, float x, float y, float width, float height)
		{
			ctx.Rectangle((x + this.x) * scale, (y + this.y) * scale, width * scale, height * scale);
			ctx.Fill();
		}
	}
}
