using System;

namespace MGE.Editor.GUI.PropDrawers
{
	public class FloatPropDrawer : PropDrawer<float>
	{
		protected override void DrawProp(float value, Action<float> setValue)
		{
			EditorGUI.NumberFeild(value).onSubmitted += setValue;
		}
	}
}
