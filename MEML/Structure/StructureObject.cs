using System.Collections.Generic;

namespace MEML;

public class MemlObject : MemlValue<Dictionary<string, MemlValue>>
{
	public MemlObject() : base(MemlType.Object, new Dictionary<string, MemlValue>()) { }

	public override MemlValue this[string key]
	{
		get
		{
			if (value.TryGetValue(key, out var item))
				return item;
			return MemlValueNull._null;
		}
		set
		{
			base.value[key] = value;
		}
	}

	public override IEnumerable<string> keys => value.Keys;
	public override IEnumerable<MemlValue> values => value.Values;
	public override IEnumerable<KeyValuePair<string, MemlValue>> pairs => value;
	public override int count => value.Count;
}
