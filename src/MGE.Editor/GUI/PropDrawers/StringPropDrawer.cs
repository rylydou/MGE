using System;

namespace MGE.Editor.GUI.PropDrawers
{
	public class StringPropDrawer : PropDrawer<string>
	{
		protected override void DrawProp(string value, Action<string> setValue)
		{
			EditorGUI.TextFeild(value).onSubmitted += setValue;
		}
	}
}
