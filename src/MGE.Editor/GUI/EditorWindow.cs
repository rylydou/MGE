using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWindow : EditorContainer
	{
		public readonly Label label;
		public abstract string title { get; }

		Container _context = new Box(Orientation.Vertical, 4);
		public override Container content => _context;

		protected EditorWindow() : base(new ScrolledWindow())
		{
			label = new Label(title);

			root.Add(_context);
		}

		protected override void Draw()
		{
			EditorGUI.verticalExpand = true;
			EditorGUI.Header(title);
		}
	}
}
