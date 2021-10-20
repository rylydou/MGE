using System.Diagnostics;

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

			Trace.WriteLine("Drawing inspector");

			var propDrawer = Editor.GetPropDrawer(context.selection.GetType());
			propDrawer.DrawProp(context.selection, val => { });
		}
	}
}
