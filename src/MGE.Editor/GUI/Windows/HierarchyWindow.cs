using Gtk;

namespace MGE.Editor.GUI.Windows
{
	public class HierarchyWindow : EditorWindow
	{
		public override string title => "Inspector";

		ScrolledWindow hierarchyContainer = new();
		TreeView hierarchyView;
		TreeStore hierarchyStore = new(typeof(string), typeof(string));

		public HierarchyWindow() : base()
		{
			foreach (var node in TestNode.root._nodes)
			{
				hierarchyStore.AppendValues(node.name, node.GetType().ToString());
			}

			hierarchyView = new(hierarchyStore) { HeadersVisible = false, Reorderable = true, EnableTreeLines = true, RubberBanding = true, Vexpand = true };

			hierarchyView.AppendColumn("Name", new CellRendererText(), "text", 0);
			hierarchyView.AppendColumn("Type", new CellRendererText(), "text", 1);

			hierarchyContainer.Add(hierarchyView);
		}

		protected override void Draw()
		{
			EditorGUI.StartHorizontal();
			EditorGUI.IconButton("add");
			EditorGUI.IconButton("remove");
			EditorGUI.End();

			EditorGUI.Add(hierarchyContainer);
		}
	}
}
