using System.Collections.Generic;

namespace MEML;

public class StructureObject : StructureValue<Dictionary<string, StructureValue>>
{
	public StructureObject() : base(StructureType.Object, new Dictionary<string, StructureValue>()) { }

	public override StructureValue this[string key]
	{
		get
		{
			if (Value.TryGetValue(key, out var value))
				return value;
			return StructureValueNull._null;
		}
		set
		{
			Value[key] = value;
		}
	}

	public override IEnumerable<string> keys => Value.Keys;
	public override IEnumerable<StructureValue> values => Value.Values;
	public override IEnumerable<KeyValuePair<string, StructureValue>> pairs => Value;
	public override int count => Value.Count;
}
