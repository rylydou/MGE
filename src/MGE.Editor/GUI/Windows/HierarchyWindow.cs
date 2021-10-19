using System.Diagnostics;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	public class HierarchyWindow : ContextWindow
	{
		public override string title => "Inspector";

		ScrolledWindow hierarchyContainer = new();
		TreeView hierarchyView;
		TreeStore hierarchyStore = new(typeof(string), typeof(string));

		public HierarchyWindow() : base()
		{
			foreach (var child in context.root._nodes)
			{
				var nodeIter = hierarchyStore.AppendValues(child.name, child.GetType().ToString());
				AddNodeHierarchy(ref nodeIter, child);
			}

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
		}

		void AddNodeHierarchy(ref TreeIter iter, TestNode node)
		{
			foreach (var child in node._nodes)
			{
				var nodeIter = hierarchyStore.AppendValues(iter, child.name, child.GetType().ToString());
				AddNodeHierarchy(ref nodeIter, child);
			}
		}

		protected override void Draw()
		{
			EditorGUI.Add(hierarchyContainer);
		}
	}
}
