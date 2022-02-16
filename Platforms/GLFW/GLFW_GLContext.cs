using System;
using MGE;

namespace MGE.GLFW
{
	internal class GLFW_GLContext : ISystemOpenGL.Context
	{
		internal readonly IntPtr window;
		internal bool disposed;

		internal GLFW_GLContext(IntPtr window)
		{
			this.window = window;
		}

		public override bool IsDisposed => disposed;

		public override void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				GLFW.SetWindowShouldClose(window, true);
			}
		}
	}
}
