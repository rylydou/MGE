using System;

namespace MGE.Editor.GUI.ObjectDrawers
{
	public class FlagsDrawer : ObjectDrawer<Enum>
	{
		public FlagsDrawer(Enum value) : base(value, false) { }

		protected override void Draw()
		{
			EditorGUI.Text(value.ToString());
		}
	}
}
