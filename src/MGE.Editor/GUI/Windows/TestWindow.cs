using System.Diagnostics;
using System.Reflection;
using MGE.Editor.GUI;

namespace MGE.Editor.GUI.Windows
{
	public class TestWindow : EditorWindow
	{
		TestStruct obj = new TestStruct();

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

			var props = typeof(TestStruct).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in props)
			{
				Label(prop.Name);
				var val = prop.GetValue(obj);
				if (val is string str)
				{
					// TextFeild(prop.PropertyType as);
				}
			}

			base.Update();
		}
	}
}
