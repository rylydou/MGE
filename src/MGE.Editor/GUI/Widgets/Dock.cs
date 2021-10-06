using Gtk;

namespace MGE.Editor.GUI.Widgets
{
	/*
		TODO
		Context Menu to add a dock above, below, left, or right
	 */
	public class Dock : Notebook
	{
		public Dock()
		{
			// EnablePopup = true;
			Scrollable = true;
			GroupName = "any";
		}

		protected override bool OnPopupMenu()
		{
			var menu = new Menu();

			return true;
		}

		public void AddWindow(EditorWindow window)
		{
			AppendPage(window.root, window.label);

			SetTabDetachable(window.root, true);
			SetTabReorderable(window.root, true);
		}

		protected override Notebook OnCreateWindow(Widget page, int x, int y)
		{
			var window = new Window("MGE Editor Dock")
			{
				TypeHint = Gdk.WindowTypeHint.Dock,
				KeepAbove = true,
				DefaultSize = new Gdk.Size(360, 480),
				SkipTaskbarHint = true,
				SkipPagerHint = true,
			};
			window.Move(x, y);

			// TODO Show tabs in titlebar
			var titlebar = new HeaderBar() { ShowCloseButton = true, };

			var dock = new Dock();
			dock.Destroyed += (sender, args) => window.Destroy();
			window.Add(dock);

			window.Titlebar = titlebar;
			window.ShowAll();

			return dock;
		}

		protected override void OnPageRemoved(Widget child, uint page_num)
		{
			// BUG Destroys window when moving the only page inside window
			// if (NPages < 1) Destroy();
		}
	}
}
