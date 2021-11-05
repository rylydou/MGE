using Gtk;

namespace MGE.Editor.GUI.Data
{
	public class ComboboxData : WidgetData<ComboBox>
	{
		public System.Action<int> onItemIndexChanged = index => { };
		public System.Action<string> onItemChanged = item => { };

		public ComboboxData(ComboBox widget) : base(widget) { }
	}
}
