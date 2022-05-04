using System.Collections;
using System.Collections.Generic;

namespace MEML;

/// <summary>
/// A data structure encapsulating an Array
/// </summary>
public class StructureArray : StructureValue<List<StructureValue>>, IEnumerable
{
	public StructureArray() : base(StructureType.Array, new List<StructureValue>()) { }

	public StructureArray(params StructureValue[] items) : base(StructureType.Array, new List<StructureValue>())
	{
		foreach (var item in items)
		{
			value.Add(item);
		}
	}

	public StructureArray(IList<string> list) : base(StructureType.Array, new List<StructureValue>())
	{
		for (int i = 0; i < list.Count; i++)
			value.Add(list[i]);
	}

	public override StructureValue this[int index]
	{
		get => value[index];
		set => base.value[index] = value;
	}

	public override void Add(StructureValue value)
	{
		base.value.Add(value);
	}

	public override void Remove(StructureValue value)
	{
		base.value.Remove(value);
	}

	public bool Contains(StructureValue value)
	{
		return base.value.Contains(value);
	}

	public override int count => value.Count;
	public override IEnumerable<StructureValue> values => value;

	public IEnumerator GetEnumerator()
	{
		return value.GetEnumerator();
	}
}
