using OpenTK.Graphics.OpenGL;

namespace MGE
{
	public class GFX
	{
		public static void Draw(Texture texture1, Rect destination, RectInt? source = null, Color? color = null)
		{
		}

		public static void Begin(int width, int height)
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-width / 2f, width / 2f, -height / 2f, height / 2f, 0.0f, 0.1f);
		}
	}
}
