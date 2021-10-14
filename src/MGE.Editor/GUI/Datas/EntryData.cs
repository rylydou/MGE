using System;
using Gtk;
using MGE.Editor.GUI.Data;

namespace MGE.Editor.GUI.Datas
{
	public class EntryData<T> : WidgetData<Entry>
	{
		public Action<T> onSubmitted = data => { };

		public EntryData(Entry widget) : base(widget) { }
	}
}
