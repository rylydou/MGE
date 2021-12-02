using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MGE
{
	public class ChunkedList<T> : IEnumerable<T> where T : struct
	{
		class LowLevelListEnumerator : IEnumerator<T>
		{
			public T Current => _list[_position];

			object IEnumerator.Current => _list[_position]!;

			readonly ChunkedList<T> _list;

			int _position;

			public LowLevelListEnumerator(ChunkedList<T> list)
			{
				_list = list;
			}

			public bool MoveNext()
			{
				if (_position++ < _list._position) return true;
				return false;
			}

			public void Reset() => _position = 0;

			public void Dispose() { }
		}

		public T[] array;
		int _position;
		public int Count => _position;

		public ChunkedList()
		{
			array = new T[128];
		}

		public ChunkedList(uint capacity)
		{
			array = new T[Math.NextPowerOf2(capacity)];
		}

		public T this[int index] { get => array[index]; set => array[index] = value; }

		/// <summary>
		/// Does not check if there is enough space to insert the item, only call if you are confident that there will be enough space
		/// </summary>
		/// <param name="item"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddUnsafe(T item) => array[_position++] = item;

		public void Add(T item)
		{
			EnsureSpaceFor(1);
			array[_position++] = item;
		}

		public void Add(T[] items)
		{
			EnsureSpaceFor(items.Length);
			items.ForEach(item => AddUnsafe(item));
		}

		public void Add(IEnumerable<T> items, int count)
		{
			EnsureSpaceFor(count);
			items.ForEach(item => AddUnsafe(item));
		}

		public void Clear() => _position = 0;

		public void CopyTo(T[] array, int arrayIndex) => this.array.CopyTo(array, arrayIndex);

		public IEnumerator<T> GetEnumerator() => new LowLevelListEnumerator(this);
		IEnumerator IEnumerable.GetEnumerator() => new LowLevelListEnumerator(this);

		public bool EnsureSpaceFor(int moreItemsCount) => EnsureCapacity(_position + moreItemsCount);
		public bool EnsureCapacity(int capacity)
		{
			if (array.Length < capacity)
			{
				var tmp = new T[Math.NextPowerOf2((uint)(capacity + 1u))];
				// Debug.Log($"Chunked list expanded from {array.Length} to {tmp.Length}");
				array.CopyTo(tmp, 0);
				array = tmp;
				return true;
			}
			return false;
		}
	}
}
