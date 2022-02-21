using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace MGE
{
	/// <summary>
	/// A data structure encapsulating a Meml Object
	/// </summary>
	public class MemlObject : MemlValue<Dictionary<string, MemlValue>>
	{
		public MemlObject() : base(MemlType.Object, new Dictionary<string, MemlValue>())
		{

		}

		public override MemlValue this[string key]
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

		public override IEnumerable<string> Keys => Value.Keys;
		public override IEnumerable<MemlValue> Values => Value.Values;
		public override IEnumerable<KeyValuePair<string, MemlValue>> Pairs => Value;
		public override int Count => Value.Count;

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

		public override MemlValue Clone()
		{
			var clone = new MemlObject();
			foreach (var (key, value) in Value)
				clone[key] = value.Clone();
			return clone;
		}
	}
}
