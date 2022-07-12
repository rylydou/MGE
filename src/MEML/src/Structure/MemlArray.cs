using System.Collections;
using System.Collections.Generic;

namespace MEML;

/// <summary>
/// A data structure encapsulating an Array
/// </summary>
public class MemlArray : MemlValue<List<MemlValue>>, IEnumerable
{
	public MemlArray() : base(MemlType.Array, new List<MemlValue>()) { }

	public MemlArray(params MemlValue[] items) : base(MemlType.Array, new List<MemlValue>())
	{
		foreach (var item in items)
		{
			value.Add(item);
		}
	}

	public MemlArray(IList<string> list) : base(MemlType.Array, new List<MemlValue>())
	{
		for (int i = 0; i < list.Count; i++)
			value.Add(list[i]);
	}

	public override MemlValue this[int index]
	{
		get => value[index];
		set => base.value[index] = value;
	}

	public override void Add(MemlValue value)
	{
		base.value.Add(value);
	}

	public override void Remove(MemlValue value)
	{
		base.value.Remove(value);
	}

	public bool Contains(MemlValue value)
	{
		return base.value.Contains(value);
	}

	public override int count => value.Count;
	public override IEnumerable<MemlValue> values => value;

	public IEnumerator GetEnumerator()
	{
		return value.GetEnumerator();
	}
}
