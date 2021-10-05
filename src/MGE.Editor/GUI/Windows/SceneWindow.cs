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
			EditorGUI.StartHorizontal(homogeneous: false);

			EditorGUI.Button("Move");
			EditorGUI.Button("Rotate");
			EditorGUI.Button("Scale");

			EditorGUI.End();

			EditorGUI.verticalExpand = true;
			EditorGUI.Add(sceneViewport);
		}
	}
}
