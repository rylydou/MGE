using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MGE;

public class AutoDictionary<TKey, TValue> : ICollection<TValue> where TKey : notnull
{
	readonly Func<TValue, TKey> _getKey;
	readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

	public AutoDictionary(Func<TValue, TKey> getKey) => _getKey = getKey;

	public TValue this[TKey key] => _dictionary[key];

	public int Count => _dictionary.Count;
	public bool IsReadOnly => false;

	public ICollection<TKey> Keys => _dictionary.Keys;
	public ICollection<TValue> Values => _dictionary.Values;

	public void Add(TValue item) => _dictionary.Add(_getKey(item), item);
	public void Set(TValue item) => _dictionary.Set(_getKey(item), item);

	public bool Remove(TValue item) => _dictionary.Remove(_getKey(item));
	public void Clear() => _dictionary.Clear();

	public bool Contains(TValue item) => _dictionary.ContainsKey(_getKey(item));
	public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

	public void CopyTo(TValue[] array, int arrayIndex) => throw new NotSupportedException();

	public IEnumerator<TValue> GetEnumerator() => _dictionary.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
