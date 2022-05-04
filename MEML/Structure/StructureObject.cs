using System.Collections.Generic;

namespace MEML;

public class StructureObject : StructureValue<Dictionary<string, StructureValue>>
{
	public StructureObject() : base(StructureType.Object, new Dictionary<string, StructureValue>()) { }

	public override StructureValue this[string key]
	{
		get
		{
			if (value.TryGetValue(key, out var item))
				return item;
			return StructureValueNull._null;
		}
		set
		{
			base.value[key] = value;
		}
	}

	public override IEnumerable<string> keys => value.Keys;
	public override IEnumerable<StructureValue> values => value.Values;
	public override IEnumerable<KeyValuePair<string, StructureValue>> pairs => value;
	public override int count => value.Count;
}
