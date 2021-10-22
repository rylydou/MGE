using System.Reflection;

namespace MGE.Editor.GUI.ObjectDrawers
{
	public class DefaultDrawer : ObjectDrawer<object>
	{
		public DefaultDrawer(object value) : base(value, true) { }

		protected override void Draw()
		{
			var props = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var prop in props)
			{
				EditorGUI.tooltip = prop.Name;
				EditorGUI.StartProperty(Editor.GetPropertyName(prop.Name));

				var propDrawer = EditorGUI.GetDrawer(prop.PropertyType);

				propDrawer.DrawProp(prop.GetValue(value)!, value => { prop.SetValue(this.value, value); ValueChanged(); });

				EditorGUI.End();
			}
		}
	}
}
