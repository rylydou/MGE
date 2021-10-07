using System.Diagnostics;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	public class HierarchyWindow : EditorWindow
	{
		TreeStore tree;
		TreeView treeView;

		public HierarchyWindow() : base("Hierarchy")
		{
			tree = new TreeStore(typeof(string), typeof(string));

			foreach (var node in TestNode.root.nodes)
			{
				tree.AppendValues(node.name, node.GetType().ToString());
			}

			treeView = new TreeView(tree) { HeadersVisible = false, Reorderable = true, EnableTreeLines = true, RubberBanding = true, };

			treeView.AppendColumn("Name", new CellRendererText(), "text", 0);
			treeView.AppendColumn("Type", new CellRendererText(), "text", 1);

			DoUpdate();
		}

		protected override void Update()
		{
			EditorGUI.Add(treeView);
		}
	}
}
