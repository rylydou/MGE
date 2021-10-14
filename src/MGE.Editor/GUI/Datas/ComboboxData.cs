using System;
using Gtk;

namespace MGE.Editor.GUI.Data
{
	public class ComboboxData : WidgetData<ComboBox>
	{
		public Action<int> onItemIndexChanged = index => { };
		public Action<string> onItemChanged = item => { };

		public ComboboxData(ComboBox widget) : base(widget) { }
	}
}
