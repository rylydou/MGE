using MGE.Editor.GUI.Widgets;

namespace MGE.Editor.GUI.Windows
{
	public class SceneWindow : EditorWindow
	{
		public override string title => "Scene";

		SceneViewport sceneViewport = new SceneViewport();

		public SceneWindow() : base() { }

		protected override void Draw()
		{
			EditorGUI.verticalExpand = true;
			EditorGUI.Add(sceneViewport);
		}
	}
}
