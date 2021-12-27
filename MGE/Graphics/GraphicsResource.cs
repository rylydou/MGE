using System;

namespace MGE.Graphics;

public abstract class GraphicsResource : IDisposable
{
	internal readonly int _handle;

	public bool isDisposed { get; private set; }

	protected GraphicsResource(int handle)
	{
		this._handle = handle;

		GC.SuppressFinalize(this);
	}

	~GraphicsResource()
	{
		throw new MGEException($"Graphics resource leaked: {this}");
	}

	public static explicit operator int(GraphicsResource res) => res._handle;

	public void Dispose()
	{
		if (isDisposed) return;
		isDisposed = true;

		Delete();

		GC.SuppressFinalize(this);
		GC.KeepAlive(this);
	}

	protected abstract void Delete();
}
