using System;
using System.Collections.Generic;

namespace MGE.Editor.GUI.Widgets
{
	public class Breadcrums : EditorWidget
	{
		public List<(string, Action)> items = new();

		public Breadcrums() : base(new Gtk.ScrolledWindow() { VscrollbarPolicy = Gtk.PolicyType.Never, Hexpand = true, }) { }

		protected override void Draw()
		{
			EditorGUI.StartHorizontal();

			foreach (var item in items)
			{
				EditorGUI.horizontalExpand = false;
				EditorGUI.Link(item.Item1).onPressed += item.Item2;
			}

			EditorGUI.End();
		}
	}
}