using System.Reflection;
using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow : EditorWidget
	{
		public readonly Label label;

		Container _content;
		public override Container content => _content;

		protected EditorWindow(bool scrolled = true) : base(scrolled ? new ScrolledWindow() : new Box(Orientation.Vertical, 4))
		{
			if (scrolled)
			{
				_content = new Box(Orientation.Vertical, 4);
				root.Add(_content);
			}
			else
			{
				_content = root;
			}

			var windowAttr = GetType().GetCustomAttribute<EditorWindowAttribute>();

			label = new Label(windowAttr?.name ?? GetType().ToString());

			Redraw();
		}

		protected override void Draw()
		{
			EditorGUI.verticalExpand = true;
			EditorGUI.Header(label.Text);
		}
	}
}
