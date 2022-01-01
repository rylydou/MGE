using System;

namespace MGE;

public class CancellableEventArgs : EventArgs
{
	public bool Cancel { get; set; }

	public CancellableEventArgs() { }
}

public class CancellableEventArgs<T> : EventArgs
{
	public T Data { get; private set; }
	public bool Cancel { get; set; }

	public CancellableEventArgs(T data)
	{
		Data = data;
	}
}
