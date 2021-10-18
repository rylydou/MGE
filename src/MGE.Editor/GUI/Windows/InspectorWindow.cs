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
				EditorGUI.Text("Nothing Selected");
				return;
			}

			Trace.WriteLine(context.selection.GetType());

			var propDrawer = Editor.GetPropDrawer(context.selection.GetType());
			propDrawer.DrawProp(context.selection, val => { });
		}
	}
}
