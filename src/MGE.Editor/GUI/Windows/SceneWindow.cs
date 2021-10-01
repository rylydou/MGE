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
			EditorGUI.StartToolbar();

			EditorGUI.horizontalExpand = false;
			EditorGUI.Button("Move");
			EditorGUI.horizontalExpand = false;
			EditorGUI.Button("Rotate");
			EditorGUI.horizontalExpand = false;
			EditorGUI.Button("Scale");

			EditorGUI.End();

			EditorGUI.verticalExpand = true;
			EditorGUI.Add(sceneViewport);
		}
	}
}
