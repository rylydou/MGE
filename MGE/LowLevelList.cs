using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MGE
{
	public class LowLevelList<T> : IList<T>
	{
		class LowLevelListEnumerator : IEnumerator<T>
		{
			public T Current => _list[_position];

			object IEnumerator.Current => _list[_position]!;

			readonly LowLevelList<T> _list;

			int _position;

			public LowLevelListEnumerator(LowLevelList<T> list)
			{
				_list = list;
			}

			public bool MoveNext()
			{
				if (_position++ < _list._count) return true;
				return false;
			}

			public void Reset() => _position = 0;

			public void Dispose() { }
		}

		public T[] items;
		int _count;
		public int Count => _count;

		public LowLevelList()
		{
			items = new T[256];
		}

		public LowLevelList(uint capacity)
		{
			items = new T[Math.NextPowerOf2(capacity)];
		}

		public T this[int index] { get => items[index]; set => items[index] = value; }

		public bool IsReadOnly => false;

		public void Add(T item)
		{
			EnsureCapacity(_count + 1);
			items[_count++] = item;
		}
		public void Add(T[] items)
		{
			EnsureCapacity(_count + items.Length);
			items.ForEach(item => this.items[_count++] = item);
		}
		public void Add(IEnumerable<T> items, int count)
		{
			EnsureCapacity(_count + count);
			items.ForEach(item => this.items[_count++] = item);
		}
		public void Insert(int index, T item) => throw new System.NotImplementedException();

		public bool Remove(T item) => throw new System.NotImplementedException();
		public void RemoveAt(int index) => throw new System.NotImplementedException();
		public void Clear() => _count = 0;

		public bool Contains(T item) => this.Contains<T>(item);
		public int IndexOf(T item) => this.IndexOf(item);

		public void CopyTo(T[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => new LowLevelListEnumerator(this);
		IEnumerator IEnumerable.GetEnumerator() => new LowLevelListEnumerator(this);

		public bool EnsureCapacity(int capacity)
		{
			if (items.Length < capacity)
			{
				var tmp = new T[Math.NextPowerOf2((uint)(capacity + 1u))];
				items.CopyTo(tmp, 0);
				items = tmp;
				return true;
			}
			return false;
		}
	}
}
