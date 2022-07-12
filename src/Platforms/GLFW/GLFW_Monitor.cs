using System;

namespace MGE.GLFW
{
	internal class GLFW_Monitor : Monitor
	{
		public readonly IntPtr pointer;

		string _name;
		bool _isPrimary;
		RectInt _bounds;
		Vector2 _contentScale;

		public override string name => _name;
		public override bool isPrimary => _isPrimary;
		public override RectInt bounds => _bounds;
		public override Vector2 contentScale => _contentScale;

		public GLFW_Monitor(IntPtr pointer)
		{
			this.pointer = pointer;

			_name = GLFW.GetMonitorName(this.pointer);
			FetchProperties();
		}

		public void FetchProperties()
		{
			GLFW.GetMonitorContentScale(pointer, out _contentScale.x, out _contentScale.y);
			GLFW.GetMonitorWorkarea(pointer, out _bounds.x, out _bounds.y, out _bounds.width, out _bounds.height);

			_isPrimary = GLFW.GetPrimaryMonitor() == pointer;
		}
	}
}
