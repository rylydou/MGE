using System;
using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class ObjectDrawer : EditorContainer
	{
		public abstract Type type { get; }
		public Action<object> onValueChanged = value => { };

		protected ObjectDrawer(Container root) : base(root) { }
	}

	public abstract class ObjectDrawer<T> : ObjectDrawer where T : notnull
	{
		public override Type type { get => typeof(T); }
		public T value { get; protected set; }

		protected void ValueChanged() => onValueChanged(value);

		protected ObjectDrawer(T value, bool multiline) : base(new Box(multiline ? Orientation.Vertical : Orientation.Horizontal, 4))
		{
			this.value = value;

			Redraw();
		}
	}
}
