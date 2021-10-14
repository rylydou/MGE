using System;
using Gtk;

namespace MGE.Editor.GUI
{
	public class InternalEntryData
	{
		public readonly Entry entry;
		public Func<string, string> onTextSubmitted = text => text;

		public InternalEntryData(Entry entry)
		{
			this.entry = entry;
		}
	}
}
