using Gtk;

namespace MGE.Editor.GUI
{
	public abstract class EditorContainer
	{
		public Container root { get; protected set; }

		public virtual Container content { get => root; }

		protected EditorContainer(Container root)
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

			Draw();

			EditorGUI.PopContainer();

			root.ShowAll();
		}

		protected abstract void Draw();
	}
}
