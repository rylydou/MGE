using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MGE
{
	[DataContract]
	public class LootTable<T> where T : ILootItem, IEnumerable<T>
	{
		[DataMember] public List<T> items;
		List<T> _combinedItems = new List<T>();

		public int count => items.Count;

		public LootTable() { }

		public LootTable(IEnumerable<T> items)
		{
			foreach (var item in items)
				Add(item);
		}

		public bool Add(T item)
		{
			if (items.Contains(item)) return false;
			items.Add(item);
			var weight = item.GetWeight();
			for (int i = 0; i < weight; i++)
				_combinedItems.Add(item);
			return true;
		}

		public T GetRandom() => Random.Choose(_combinedItems);

		[OnDeserialized]
		void OnDeserialized(StreamingContext context)
		{
			foreach (var item in items)
			{
				var weight = item.GetWeight();
				for (int i = 0; i < weight; i++)
					_combinedItems.Add(item);
			}
		}

		public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
	}
}