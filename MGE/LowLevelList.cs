using System.Collections;
using System.Collections.Generic;

namespace MGE
{
	public class LowLevelList<T> : IEnumerable<T>
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

		public T[] array;
		int _count;
		public int Count => _count;

		public LowLevelList()
		{
			array = new T[256];
		}

		public LowLevelList(uint capacity)
		{
			array = new T[Math.NextPowerOf2(capacity)];
		}

		public T this[int index] { get => array[index]; set => array[index] = value; }

		public void Add(T item)
		{
			EnsureCapacity(_count + 1);
			array[_count++] = item;
		}
		public void Add(T[] items)
		{
			EnsureCapacity(_count + items.Length);
			items.ForEach(item => this.array[_count++] = item);
		}
		public void Add(IEnumerable<T> items, int count)
		{
			EnsureCapacity(_count + count);
			items.ForEach(item => this.array[_count++] = item);
		}

		public void Clear() => _count = 0;

		public void CopyTo(T[] array, int arrayIndex) => this.array.CopyTo(array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => new LowLevelListEnumerator(this);
		IEnumerator IEnumerable.GetEnumerator() => new LowLevelListEnumerator(this);

		public bool EnsureCapacity(int capacity)
		{
			if (array.Length < capacity)
			{
				var tmp = new T[Math.NextPowerOf2((uint)(capacity + 1u))];
				array.CopyTo(tmp, 0);
				array = tmp;
				Debug.Log("Expanded to " + tmp.Length);
				return true;
			}
			return false;
		}
	}
}
