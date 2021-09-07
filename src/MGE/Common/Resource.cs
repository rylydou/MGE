using System;

namespace MGE
{
	public abstract class Resource : IDisposable
	{
		public bool isDisposed { get; private set; }

		protected Resource()
		{
			isDisposed = false;
		}

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
}
