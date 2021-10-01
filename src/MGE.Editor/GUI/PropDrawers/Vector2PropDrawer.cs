using System;

namespace MGE.Editor.GUI.PropDrawers
{
	public class Vector2PropDrawer : PropDrawer<Vector2>
	{
		protected override void DrawProp(Vector2 value, Action<Vector2> setValue)
		{
			EditorGUI.Label("X");
			EditorGUI.NumberFeild(value.x);
			EditorGUI.Label("Y");
			EditorGUI.NumberFeild(value.y);
		}
	}
}
