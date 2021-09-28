using System;

namespace MGE.Editor.GUI.Events
{
	public class ListEvents<T>
	{
		public Action<T> onItemAdded = item => { };
		public Action<int> onItemRemoved = position => { };

		public Action<int, int> onItemReordered = (from, to) => { };
	}
}
