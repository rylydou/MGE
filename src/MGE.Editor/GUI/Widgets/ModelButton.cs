using System;

namespace MGE.Editor.GUI.Widgets
{
	public class ModelButton
	{
		public string title;
		public string? tooltip;
		public Action onClicked;
		public bool onLeftSide;

		public ModelButton(string title, Action onClicked, string? tooltip = null, bool onLeftSide = false)
		{
			this.title = title;
			this.onClicked = onClicked;
			this.tooltip = tooltip;
			this.onLeftSide = onLeftSide;
		}
	}
}
