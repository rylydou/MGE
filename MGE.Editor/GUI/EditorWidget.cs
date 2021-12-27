using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorWidget
	{
		public Container root { get; protected set; }

		public virtual Container content { get => root; }

		protected EditorWidget(Container root)
		{
			this.root = root;
		}

		public virtual void Redraw()
		{
			var children = content.Children;
			foreach (var child in children)
			{
				content.Remove(child);
			}

			EditorGUI.PushContainer(content);
			var containerIsBin = EditorGUI.containerInfo.isBin;

			Draw();

			if (!containerIsBin) EditorGUI.PopContainer();

			root.ShowAll();
		}

		protected abstract void Draw();

		public static implicit operator Widget(EditorWidget widget) => widget.root;
	}
}
