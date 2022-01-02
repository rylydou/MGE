namespace MGE;

public class CancellableEventArgs : EventArgs
{
	public bool cancel { get; private set; }

	public CancellableEventArgs() { }

	public void Cancel()
	{
		cancel = true;
	}
}

public class CancellableEventArgs<T> : CancellableEventArgs
{
	public T data { get; init; }

	public CancellableEventArgs(T data)
	{
		this.data = data;
	}
}
