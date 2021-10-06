using System.Diagnostics.CodeAnalysis;
using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow
	{
		public Label label { get; private set; }
		string _title;
		public string title
		{
			get => _title;
			set
			{
				label.Text = value;
				_title = value;
			}
		}

		public ScrolledWindow root { get; private set; }
		public Container content { get; private set; }

		protected EditorWindow(string title)
		{
			this.label = new Label(title);
			_title = title;

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
			EditorGUI.verticalExpand = true;
			EditorGUI.Header(title);
		}
	}
}
