using System.Diagnostics;
using MGE.Editor.GUI;

namespace MGE.Editor.GUI.Windows
{
	public class TestWindow : EditorWindow
	{
		protected override void Update()
		{
			Label("Enabled");
			var enabled = Checkbox(true);
			enabled += state => Trace.WriteLine("Enabled " + state);

			Label("Visible");
			var visible = Checkbox(true);
			visible += state => Trace.WriteLine("Visible " + state);

			Label("Name");
			var name = TextFeild("Unnamed");
			name += text => Trace.WriteLine("Name " + text);

			base.Update();
		}
	}
}
