using System.Diagnostics;
using MGE.Editor.GUI;

namespace MGE.Editor.GUI.Windows
{
	public class TestWindow : EditorWindow
	{
		protected override void Update()
		{
			Label("Enabled");
			Checkbox(true).onToggled += state => Trace.WriteLine("Enabled " + state);

			Label("Visible");
			Checkbox(true).onToggled += state => Trace.WriteLine("Visible " + state);

			Label("Name");
			TextFeild("Unnamed").onTextSubmitted += text => Trace.WriteLine("Name " + text);

			Label("Position");
			StartHorizontal();
			Label("X");
			TextFeild("0.0").onTextSubmitted += text => Trace.WriteLine("X " + text);
			Label("Y");
			TextFeild("0.0").onTextSubmitted += text => Trace.WriteLine("Y " + text);
			End();

			Button("Delete").onPressed += () => Trace.WriteLine("Delete Pressed");

			base.Update();
		}
	}
}
