using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace MEML
{
	public class StructureObject : MemlValue<Dictionary<string, StructureValue>>
	{
		public StructureObject() : base(StructureType.Object, new Dictionary<string, StructureValue>())
		{

		}

		public override StructureValue this[string key]
		{
			get
			{
				if (Value.TryGetValue(key, out var value))
					return value;
				return MemlNull._null;
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

		// public override int GetHashedValue()
		// {
		// 	unchecked
		// 	{
		// 		int hash = 17;
		// 		foreach (var (key, value) in Value)
		// 		{
		// 			hash = hash * 23 + Calc.StaticStringHash(key);
		// 			hash = hash * 23 + value.GetHashedValue();
		// 		}
		// 		return hash;
		// 	}
		// }
	}
}
