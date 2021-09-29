using System;

namespace MGE.Editor.GUI.PropDrawers
{
	public class BoolPropDrawer : PropDrawer<bool>
	{
		protected override void DrawProp(bool value, Action<bool> setValue)
		{
			EditorGUI.Checkbox(value).onToggled += setValue;
		}
	}
}
