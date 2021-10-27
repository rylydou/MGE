using Gtk;

namespace MGE.Editor.GUI.Windows
{
	[EditorWindow("Real Hierarchy")]
	public class RealHierarchyWindow : ContextWindow
	{
		ScrolledWindow hierarchyContainer = new();
		TreeView hierarchyView;
		TreeStore hierarchyStore = new(typeof(string), typeof(string), typeof(int));

		public RealHierarchyWindow() : base()
		{
			hierarchyView = new(hierarchyStore) { HeadersVisible = false, EnableTreeLines = true, RubberBanding = true, Vexpand = true, };

			hierarchyView.AppendColumn("Name", new CellRendererText(), "text", 0);
			hierarchyView.AppendColumn("Type", new CellRendererText(), "text", 1);
			hierarchyView.AppendColumn("ID", new CellRendererText(), "text", 2);

			hierarchyContainer.Add(hierarchyView);
		}

		protected override void Draw()
		{
			EditorGUI.Button("Refresh").onPressed += () => Reload();
			EditorGUI.Add(hierarchyContainer);
		}

		public void Reload()
		{
			hierarchyStore.Clear();

			foreach (var child in context.root)
			{
				var nodeIter = hierarchyStore.AppendValues(child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(ref nodeIter, child);
			}
		}

		void AddNodeHierarchy(ref TreeIter iter, GameNode node)
		{
			foreach (var child in node)
			{
				var nodeIter = hierarchyStore.AppendValues(iter, child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(ref nodeIter, child);
			}
		}
	}
}
