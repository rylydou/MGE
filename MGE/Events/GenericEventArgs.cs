namespace MGE;

public sealed class GenericEventArgs<T> : EventArgs
{
	public T Data { get; private set; }

	public GenericEventArgs(T value)
	{
		Data = value;
	}
}
