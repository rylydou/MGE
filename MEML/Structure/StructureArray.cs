using System.Collections;
using System.Collections.Generic;

namespace MEML;

/// <summary>
/// A data structure encapsulating an Array
/// </summary>
public class StructureArray : MemlValue<List<StructureValue>>, IEnumerable
{
	public StructureArray() : base(StructureType.Array, new List<StructureValue>()) { }

	public StructureArray(IList<string> list) : base(StructureType.Array, new List<StructureValue>())
	{
		for (int i = 0; i < list.Count; i++)
			Value.Add(list[i]);
	}

	public override StructureValue this[int index]
	{
		get => Value[index];
		set => Value[index] = value;
	}

	public override void Add(StructureValue value)
	{
		Value.Add(value);
	}

	public override void Remove(StructureValue value)
	{
		Value.Remove(value);
	}

	public bool Contains(StructureValue value)
	{
		return Value.Contains(value);
	}

	public override int count => Value.Count;
	public override IEnumerable<StructureValue> values => Value;

	public IEnumerator GetEnumerator()
	{
		return Value.GetEnumerator();
	}
}
