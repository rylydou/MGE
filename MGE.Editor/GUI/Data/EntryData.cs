using Gtk;

namespace MGE.Editor.GUI.Data
{
	public class EntryData<T> : WidgetData<Entry>
	{
		public System.Action<T> onSubmitted = data => { };

		public EntryData(Entry widget) : base(widget) { }
	}
}
