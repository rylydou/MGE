using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow
	{
		public string title;

		public ScrolledWindow root;
		public ListBox listBox;

		protected EditorWindow(string title)
		{
			this.title = title;

			root = new ScrolledWindow();
			listBox = new ListBox();
			root.Add(listBox);

			DoUpdate();
		}

		internal virtual void DoUpdate()
		{
			var children = listBox.Children;
			foreach (var child in children)
			{
				listBox.Remove(child);
			}

			EditorGUI.PushContainer(listBox);

			Update();

			EditorGUI.PopContainer();

			listBox.ShowAll();
		}

		protected virtual void Update()
		{
			EditorGUI.Header(title);
		}
	}
}
