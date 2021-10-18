using System;
using System.Reflection;

namespace MGE.Editor.GUI.PropDrawers
{
	public class FallbackPropDrawer : PropDrawer
	{
		public override Type type => throw new NotImplementedException();

		public override void DrawProp(object? value, Action<object?> setValue)
		{
			if (value is null)
			{
				EditorGUI.Text("(null)");
				return;
			}

			var props = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in props)
			{
				if (prop.Name == "_nodes") continue;

				EditorGUI.StartProperty(Editor.GetPropertyName(prop.Name));

				var propDrawer = Editor.GetPropDrawer(prop.PropertyType);

				propDrawer.DrawProp(prop.GetValue(value), val => { prop.SetValue(value, val); setValue.Invoke(null); });

				EditorGUI.End();
			}
		}
	}
}
