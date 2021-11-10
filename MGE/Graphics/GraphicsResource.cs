using System;

namespace MGE.Graphics;

public abstract class GraphicsResource : IDisposable
{
	public readonly int handle;

	public bool isDisposed { get; private set; }

	protected GraphicsResource(int handle)
	{
		this.handle = handle;

		GC.SuppressFinalize(this);
	}

	~GraphicsResource()
	{
		Dispose(false);
		throw new Exception($"Graphics resource leaked: {this}");
	}

	public static explicit operator int(GraphicsResource res) => res.handle;

	public void Dispose()
	{
		if (isDisposed) return;
		isDisposed = true;

		Dispose(true);

		GC.SuppressFinalize(this);
		GC.KeepAlive(this);
	}

	protected abstract void Dispose(bool manual);
}
