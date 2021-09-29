using System.Diagnostics;
using System.Reflection;
using MGE.Editor.GUI;

namespace MGE.Editor.GUI.Windows
{
	public class TestWindow : EditorWindow
	{
		TestNode obj = new TestNode();

		protected override void Update()
		{
			// Label("Enabled");
			// Checkbox(true).onToggled += state => Trace.WriteLine("Enabled " + state);

			// Label("Visible");
			// Checkbox(true).onToggled += state => Trace.WriteLine("Visible " + state);

			// Label("Name");
			// TextFeild("Unnamed").onSubmitted += text => Trace.WriteLine("Name " + text);

			// Label("Position");
			// StartHorizontal();
			// Label("X");
			// NumberFeild(0f).onSubmitted += num => Trace.WriteLine("X " + num);
			// Label("Y");
			// NumberFeild(0f).onSubmitted += num => Trace.WriteLine("Y " + num);
			// End();

			// Button("Delete").onPressed += () => Trace.WriteLine("Delete Pressed");

			var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in props)
			{
				Label(EditorCache.GetPropertyName(prop.Name));

				var val = prop.GetValue(obj);

				if (val is bool b) Checkbox(b).onToggled += s => prop.SetValue(obj, s);
				else if (val is string str) TextFeild(str).onSubmitted += text => prop.SetValue(obj, text);
				else if (val is int i) NumberFeild(i).onSubmitted += num => prop.SetValue(obj, num);
				else if (val is float f) NumberFeild(f).onSubmitted += num => prop.SetValue(obj, num);
				else if (val is double d) NumberFeild(d).onSubmitted += num => prop.SetValue(obj, num);
			}

			base.Update();
		}
	}
}
