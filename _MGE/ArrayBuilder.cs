namespace MGE;

public class ArrayBuilder<T>
{
	public T[] array;
	public int position;

	public ArrayBuilder(int length)
	{
		array = new T[length];
	}

	public void Add(T item)
	{
		array[position++] = item;
	}
}
