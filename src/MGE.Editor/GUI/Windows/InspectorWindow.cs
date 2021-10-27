namespace MGE.Editor.GUI.Windows
{
	[EditorWindow("Inspector")]
	public class InspectorWindow : ContextWindow
	{
		public InspectorWindow() : base()
		{
			context.onSelectionChanged += () => Redraw();
		}

		protected override void Draw()
		{
			if (context.multipleSelectedObjects)
			{
				EditorGUI.verticalExpand = true;
				EditorGUI.Header("Multiple Selected Objects");
				return;
			}

			if (context.selectedObject is null)
			{
				EditorGUI.verticalExpand = true;
				EditorGUI.Header("Nothing Selected");
				return;
			}

			EditorGUI.Value(context.selectedObject, val => { });
		}
	}
}
