using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	[EditorWindow("Hierarchy")]
	public class HierarchyWindow : ContextWindow
	{
		ScrolledWindow hierarchyContainer = new();
		TreeView hierarchyView;
		TreeStore hierarchyStore = new(typeof(string), typeof(string), typeof(int));

		public HierarchyWindow() : base(false)
		{
			foreach (var child in context.root)
			{
				var nodeIter = hierarchyStore.AppendValues(child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(nodeIter, child);
			}

			hierarchyStore.RowChanged += (sender, args) =>
			{
				var node = GameNode.nodeDatabase[(int)hierarchyStore.GetValue(args.Iter, 2)];

				var oldPosition = node.siblingIndex;
				var originalParent = node.parent ?? throw new InvalidOperationException("Cannot move root node");

				var newPosition = args.Path.Indices.Last();
				var newParentPath = new TreePath(args.Path.Indices);
				var newParent = context.root;
				if (newParentPath.Up() && newParentPath.Depth > 0)
				{
					hierarchyStore.GetIter(out var iter, newParentPath);
					newParent = GameNode.nodeDatabase[(int)hierarchyStore.GetValue(iter, 2)];
				}

				Trace.WriteLine($"Moving node #{node.id} from #{originalParent.id} to #{newParent.id} at {newPosition}");

				if (originalParent == newParent && newPosition >= oldPosition)
				{
					newPosition--;
				}
				node.Detach();
				newParent.AttachNode(node, newPosition);
			};

			hierarchyView = new(hierarchyStore) { HeadersVisible = false, Reorderable = true, EnableTreeLines = true, RubberBanding = false, Vexpand = true, LevelIndentation = 1, };
			hierarchyView.Selection.Mode = SelectionMode.Multiple;

			hierarchyView.AppendColumn("Name", new CellRendererText(), "text", 0);
			hierarchyView.AppendColumn("Type", new CellRendererText(), "text", 1);
			// hierarchyView.AppendColumn("ID", new CellRendererText(), "text", 2);

			hierarchyView.CursorChanged += (sender, args) =>
			{
				context.ClearSelection();

				var selectedRows = hierarchyView.Selection.GetSelectedRows();
				var selectedNodes = new List<GameNode>(selectedRows.Length);
				foreach (var row in selectedRows)
				{
					hierarchyStore.GetIter(out var iter, row);
					var id = (int)hierarchyStore.GetValue(iter, 2);
					selectedNodes.Add(GameNode.nodeDatabase[id]);
				}

				context.SetSelection(selectedNodes);
			};

			hierarchyContainer.Add(hierarchyView);
		}

		void AddNodeHierarchy(TreeIter iter, GameNode node)
		{
			EditorGUI.StartHorizontal();

			EditorGUI.IconButton("Add");
			EditorGUI.IconButton("Search");

			EditorGUI.End();

			EditorGUI.VerticalOverflow();
			EditorGUI.StartVertical();

			foreach (var child in node)
			{
				var nodeIter = hierarchyStore.AppendValues(iter, child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(nodeIter, child);
			}

			EditorGUI.End();
		}

		protected override void Draw()
		{
			EditorGUI.Add(hierarchyContainer);
		}
	}
}
