using System;
using System.Collections;
using System.Collections.Generic;

namespace MGE
{
	public class AutoDictionary<TKey, TValue> : ICollection<TValue>
	{
		private readonly Func<TValue, TKey> _getKey;
		private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

		public AutoDictionary(Func<TValue, TKey> getKey) => _getKey = getKey;

		public TValue this[TKey key] => _dictionary[key];

		public int Count => _dictionary.Count;
		public bool IsReadOnly => false;

		public ICollection<TKey> Keys => _dictionary.Keys;
		public ICollection<TValue> Values => _dictionary.Values;

		public void Add(TValue item) => _dictionary.Add(_getKey.Invoke(item), item);
		public bool Remove(TValue item) => _dictionary.Remove(_getKey.Invoke(item));
		public void Clear() => _dictionary.Clear();

		public bool Contains(TValue item) => _dictionary.ContainsKey(_getKey.Invoke(item));
		public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
		public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

		public void CopyTo(TValue[] array, int arrayIndex) => throw new NotSupportedException();

		public IEnumerator<TValue> GetEnumerator() => _dictionary.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}