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
			if (context.selection is null)
			{
				EditorGUI.verticalExpand = true;
				EditorGUI.Header("Nothing Selected");
				return;
			}

			EditorGUI.Value(context.selection, val => { });
		}
	}
}
