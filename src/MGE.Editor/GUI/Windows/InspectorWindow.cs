namespace MGE.Editor.GUI.Windows
{
	public class InspectorWindow : ContextWindow
	{
		public override string title => "Inspector";

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
