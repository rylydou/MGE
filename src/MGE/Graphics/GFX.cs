using System;
using System.Collections.Generic;
using MGE.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MGE
{
	public static class GFX
	{
		public static List<Vector3> verts = new List<Vector3>();

		public static void Init()
		{

		}

		public static void NewFrame()
		{

		}

		public static void Clear()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit);
		}

		public static void Finalise()
		{
		}

		public static void StartBatch()
		{
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


		}

		public static void EndBatch()
		{
			GL.LoadIdentity();
		}

		public static void Draw(Texture texture, Vector2 position, Color? color = null)
		{
			if (!color.HasValue) color = Color.white;
		}

		public static void Draw(Texture texture, Rect destination, RectInt? source = null, Color? color = null)
		{
			if (!color.HasValue) color = Color.white;
		}

		public static void CheckError()
		{
			var error = GL.GetError();
			if (error == ErrorCode.NoError) return;
			throw new Exception($"Graphics Error {error} ({error:d})");
		}

		// public static void Begin(int width, int height)
		// {
		// 	GL.MatrixMode(MatrixMode.Projection);
		// 	GL.LoadIdentity();
		// 	GL.Ortho(-width / 2f, width / 2f, -height / 2f, height / 2f, 0.0f, 0.1f);
		// }
	}
}
