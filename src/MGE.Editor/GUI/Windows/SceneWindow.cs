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
			EditorGUI.StartHorizontal();

			EditorGUI.Header("TODO Toolbar");

			EditorGUI.End();

			EditorGUI.yExpand = true;
			EditorGUI.Add(sceneViewport);
		}
	}
}
