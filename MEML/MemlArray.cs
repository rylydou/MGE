using System.Collections;
using System.Collections.Generic;

namespace MGE
{
	/// <summary>
	/// A data structure encapsulating a Meml Array
	/// </summary>
	public class MemlArray : MemlValue<List<MemlValue>>, IEnumerable
	{
		public MemlArray() : base(MemlType.Array, new List<MemlValue>())
		{
		}

		public MemlArray(IList<string> list) : base(MemlType.Array, new List<MemlValue>())
		{
			for (int i = 0; i < list.Count; i++)
				Value.Add(list[i]);
		}

		public override MemlValue this[int index]
		{
			get => Value[index];
			set => Value[index] = value;
		}

		public override void Add(MemlValue value)
		{
			Value.Add(value);
		}

		public override void Remove(MemlValue value)
		{
			Value.Remove(value);
		}

		public bool Contains(MemlValue value)
		{
			return Value.Contains(value);
		}

		public override int count => Value.Count;
		public override IEnumerable<MemlValue> values => Value;

		// public override int GetHashedValue()
		// {
		// 	unchecked
		// 	{
		// 		int hash = 17;
		// 		foreach (var value in Value)
		// 			hash = hash * 23 + value.GetHashedValue();
		// 		return hash;
		// 	}
		// }

		public IEnumerator GetEnumerator()
		{
			return Value.GetEnumerator();
		}
	}
}
