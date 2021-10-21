using System.Diagnostics;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	public class HierarchyWindow : ContextWindow
	{
		public override string title => "Inspector";

		ScrolledWindow hierarchyContainer = new();
		TreeView hierarchyView;
		TreeStore hierarchyStore = new(typeof(string), typeof(string), typeof(int));

		public HierarchyWindow() : base()
		{
			foreach (var child in context.root._nodes)
			{
				var nodeIter = hierarchyStore.AppendValues(child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(ref nodeIter, child);
			}

			hierarchyStore.RowChanged += (sender, args) =>
			{

			};

			hierarchyView = new(hierarchyStore) { HeadersVisible = false, Reorderable = true, EnableTreeLines = true, RubberBanding = true, Vexpand = true, };

			hierarchyView.AppendColumn("Name", new CellRendererText(), "text", 0);
			hierarchyView.AppendColumn("Type", new CellRendererText(), "text", 1);

			hierarchyView.ButtonPressEvent += (sender, args) =>
			{
				if (args.Event.Type == Gdk.EventType.ButtonPress && args.Event.Button == 3)
				{
					Trace.WriteLine("Right");

					EditorGUI.StartMenu();

					EditorGUI.MenuButton("Add Child");
					EditorGUI.MenuButton("Add Parent");
					EditorGUI.MenuSeparator();
					EditorGUI.MenuButton("Remove");

					EditorGUI.EndMenu();
				}
			};

			hierarchyContainer.Add(hierarchyView);

			hierarchyView.CursorChanged += (sender, args) =>
			{
				var path = hierarchyView.Selection.GetSelectedRows()[0];
				hierarchyStore.GetIter(out var iter, path);
				var id = (int)hierarchyStore.GetValue(iter, 2);

				context.selection = TestNode.nodes[id];
				context.onSelectionChanged();
			};
		}

		void AddNodeHierarchy(ref TreeIter iter, TestNode node)
		{
			foreach (var child in node._nodes)
			{
				var nodeIter = hierarchyStore.AppendValues(iter, child.name, child.GetType().ToString(), child.id);
				AddNodeHierarchy(ref nodeIter, child);
			}
		}

		protected override void Draw()
		{
			EditorGUI.Add(hierarchyContainer);
		}
	}
}
