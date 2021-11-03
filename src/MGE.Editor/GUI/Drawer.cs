using System;
using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class Drawer : EditorWidget
	{
		public abstract Type type { get; }
		public Action<object> onValueChanged = value => { };

		protected Drawer(Container root) : base(root) { }
	}

	public abstract class Drawer<T> : Drawer where T : notnull
	{
		public override Type type { get => typeof(T); }
		public T value { get; protected set; }

		protected void ValueChanged() => onValueChanged(value);

		protected Drawer(T value, bool multiline) : base(new Box(multiline ? Orientation.Vertical : Orientation.Horizontal, 4))
		{
			this.value = value;

			Redraw();
		}
	}
}
