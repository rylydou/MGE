namespace MGE.Editor.GUI.Windows
{
	public class InspectorWindow : EditorWindow
	{
		public InspectorWindow() : base("Inspector")
		{
			Editor.onSelectionChanged += () => DoUpdate();
		}

		protected override void Update()
		{
			if (Editor.selectedObject is null)
			{
				EditorGUI.Text("Nothing Selected");
				return;
			}

			var propDrawer = Editor.GetPropDrawer(Editor.selectedObject.GetType());
			propDrawer.DrawProp(Editor.selectedObject, val => DoUpdate());
		}
	}
}
