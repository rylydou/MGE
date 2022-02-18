using System;
using System.Numerics;
using MGE;

namespace MGE.GLFW
{
	internal class GLFW_Monitor : Monitor
	{
		public readonly IntPtr Pointer;

		string _name;
		bool _isPrimary;
		RectInt _bounds;
		Vector2 _contentScale;

		public override string name => name;
		public override bool isPrimary => isPrimary;
		public override RectInt bounds => bounds;
		public override Vector2 contentScale => contentScale;

		public GLFW_Monitor(IntPtr pointer)
		{
			Pointer = pointer;

			_name = GLFW.GetMonitorName(Pointer);
			FetchProperties();
		}

		public void FetchProperties()
		{
			GLFW.GetMonitorContentScale(Pointer, out _contentScale.x, out _contentScale.y);
			GLFW.GetMonitorWorkarea(Pointer, out _bounds.x, out _bounds.y, out _bounds.width, out _bounds.height);

			_isPrimary = GLFW.GetPrimaryMonitor() == Pointer;
		}
	}
}
