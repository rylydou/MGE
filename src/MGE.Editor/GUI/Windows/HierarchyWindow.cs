using System.Diagnostics;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	public class HierarchyWindow : ContextWindow
	{
		public override string title => "Hierarchy";

		ScrolledWindow hierarchyContainer = new();
		TreeView hierarchyView;
		TreeStore hierarchyStore = new(typeof(string), typeof(string), typeof(int));

		public HierarchyWindow() : base()
		{
			foreach (var child in context.root)
			{
				var nodeIter = hierarchyStore.AppendValues(child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(nodeIter, child);
			}

			hierarchyStore.RowChanged += (sender, args) =>
			{
				var node = GameNode.nodeDatabase[(int)hierarchyStore.GetValue(args.Iter, 2)];

				var startNode = node.parent;

				var desinationPath = args.Path;
				var desinationNode = context.root;
				if (desinationPath.Up() && desinationPath.Depth > 0)
				{
					hierarchyStore.GetIter(out var iter, desinationPath);
					desinationNode = GameNode.nodeDatabase[(int)hierarchyStore.GetValue(iter, 2)];
				}

				Trace.WriteLine($">>> Moving node {node.id} from {startNode?.id} to {desinationNode.id}");

				node.Detach();
				desinationNode.AttachNode(node);
			};

			hierarchyView = new(hierarchyStore) { HeadersVisible = false, Reorderable = true, EnableTreeLines = true, RubberBanding = true, Vexpand = true, };

			hierarchyView.AppendColumn("Name", new CellRendererText(), "text", 0);
			hierarchyView.AppendColumn("Type", new CellRendererText(), "text", 1);
			// hierarchyView.AppendColumn("ID", new CellRendererText(), "text", 2);

			hierarchyView.CursorChanged += (sender, args) =>
			{
				var path = hierarchyView.Selection.GetSelectedRows()[0];
				hierarchyStore.GetIter(out var iter, path);
				var id = (int)hierarchyStore.GetValue(iter, 2);

				context.selection = GameNode.nodeDatabase[id];
				context.onSelectionChanged();
			};

			hierarchyContainer.Add(hierarchyView);
		}

		void AddNodeHierarchy(TreeIter iter, GameNode node)
		{
			foreach (var child in node)
			{
				var nodeIter = hierarchyStore.AppendValues(iter, child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(nodeIter, child);
			}
		}

		protected override void Draw()
		{
			EditorGUI.Add(hierarchyContainer);
		}
	}
}
