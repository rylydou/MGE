using System;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics
{
	public static class GFX
	{
		internal static void CheckError()
		{
			var error = GL.GetError();

			if (error != ErrorCode.NoError)
			{
				throw new Exception($"Open GL Error: {error} - {error.ToString()}");
			}
		}
	}
}
