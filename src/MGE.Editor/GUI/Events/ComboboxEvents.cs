using System;
using Pango;

namespace MGE.Editor.GUI.Events
{
	public class ComboboxEvents
	{
		public Action<int> onItemIndexChanged = index => { };
		public Action<string> onItemChanged = item => { };
	}
}
