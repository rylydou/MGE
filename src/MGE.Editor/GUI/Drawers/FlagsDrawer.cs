using System;

namespace MGE.Editor.GUI.Drawers
{
	public class FlagsDrawer : Drawer<Enum>
	{
		public FlagsDrawer(Enum value) : base(value, false) { }

		protected override void Draw()
		{
			EditorGUI.Text(value.ToString());
		}
	}
}
