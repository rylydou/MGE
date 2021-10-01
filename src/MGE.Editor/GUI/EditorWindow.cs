using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow
	{
		public string title;

		public ScrolledWindow root;
		public Box content;

		protected EditorWindow(string title)
		{
			this.title = title;

			root = new ScrolledWindow();
			content = new Box(Orientation.Vertical, 4);
			root.Add(content);

			DoUpdate();
		}

		internal virtual void DoUpdate()
		{
			var children = content.Children;
			foreach (var child in children)
			{
				content.Remove(child);
			}

			EditorGUI.PushContainer(content);

			Update();

			EditorGUI.PopContainer();

			content.ShowAll();
		}

		protected virtual void Update()
		{
			EditorGUI.Header(title);
		}
	}
}
