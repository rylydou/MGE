using Gtk;

namespace MGE.Editor.GUI.Data
{
	public class MenuItemData : WidgetData<MenuItem>
	{
		public System.Action onPressed = () => { };

		public MenuItemData(MenuItem widget) : base(widget) { }
	}
}
