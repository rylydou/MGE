using MGE.Editor.GUI.Widgets;

namespace MGE.Editor.GUI.Windows
{
	public class SceneWindow : EditorWindow
	{
		SceneViewport sceneViewport = new SceneViewport();

		public SceneWindow() : base("Scene")
		{
		}

		protected override void Update()
		{
			EditorGUI.verticalExpand = true;
			EditorGUI.Add(sceneViewport);
		}
	}
}
