using System;
using Gtk;

namespace MGE.Editor.GUI.Widgets
{
	public class Model : Window
	{
		public Model(string title, string content, params ModelButton[] buttons) : base(title)
		{
			TypeHint = Gdk.WindowTypeHint.Dialog;
			Decorated = false;

			var mainLayout = new Box(Orientation.Vertical, 8);
			Add(mainLayout);

			EditorGUI.PushContainer(mainLayout);

			EditorGUI.verticalExpand = true;
			EditorGUI.Paragraph(content);

			EditorGUI.StartHorizontal();

			foreach (var button in buttons)
			{
				EditorGUI.Button(button.title).onPressed += () => button.onClicked.Invoke();
			}

			EditorGUI.End();

			EditorGUI.PopContainer();

			ShowAll();
		}
	}
}
