using Gtk;
using MGE.Editor.GUI.Widgets;

namespace MGE.Editor.GUI.Windows
{
	[EditorWindow("Scene", Orientation.Horizontal)]
	public class SceneWindow : EditorWindow
	{
		SceneViewport sceneViewport = new SceneViewport();

		public SceneWindow() : base() { }

		protected override void Draw()
		{
			EditorGUI.verticalExpand = true;
			EditorGUI.Add(sceneViewport);
		}
	}
}
