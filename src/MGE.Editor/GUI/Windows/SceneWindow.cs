namespace MGE.Editor.GUI.Windows
{
	public class SceneWindow : EditorWindow
	{
		SceneViewport sceneViewport = new SceneViewport() { Hexpand = true, Vexpand = true };

		public SceneWindow() : base("Scene")
		{
		}

		protected override void Update()
		{
			EditorGUI.StartHorizontal();

			EditorGUI.Header("TODO Toolbar");

			EditorGUI.End();

			EditorGUI.Add(sceneViewport);
		}
	}
}
