using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace MGE
{
	public class GFX
	{
		static List<VertexPositionColorTexture> _verts = new List<VertexPositionColorTexture>();

		public static void StartBatch()
		{
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		public static void EndBatch()
		{
			GL.LoadIdentity();
		}

		public static void Draw(Texture texture, Rect destination, RectInt? source = null, Color? color = null)
		{
		}

		// public static void Begin(int width, int height)
		// {
		// 	GL.MatrixMode(MatrixMode.Projection);
		// 	GL.LoadIdentity();
		// 	GL.Ortho(-width / 2f, width / 2f, -height / 2f, height / 2f, 0.0f, 0.1f);
		// }
	}
}
