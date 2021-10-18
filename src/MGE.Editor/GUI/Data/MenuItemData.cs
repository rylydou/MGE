using Gtk;

namespace MGE.Editor.GUI.Data
{
	public class MenuItemData : WidgetData<MenuItem>
	{
		public System.Action onClicked = () => { };

		public MenuItemData(MenuItem widget) : base(widget) { }
	}
}
