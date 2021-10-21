using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow : EditorContainer
	{
		public virtual Orientation preferedOrientation { get => Orientation.Vertical; }

		public readonly Label label;
		public abstract string title { get; }

		Container _content = new Box(Orientation.Vertical, 4);
		public override Container content => _content;

		protected EditorWindow() : base(new ScrolledWindow())
		{
			label = new Label(title);

			root.Add(_content);
		}

		protected override void Draw()
		{
			EditorGUI.verticalExpand = true;
			EditorGUI.Header(title);
		}
	}
}
