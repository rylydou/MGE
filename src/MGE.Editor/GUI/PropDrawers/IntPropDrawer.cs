using System;

namespace MGE.Editor.GUI.PropDrawers
{
	public class IntPropDrawer : PropDrawer<int>
	{
		protected override void DrawProp(int value, Action<int> setValue)
		{
			EditorGUI.NumberFeild(value).onSubmitted += setValue;
		}
	}
}
