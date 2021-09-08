using System;
using System.Reflection;

namespace MGE.Graphics
{
	public abstract class GraphicsResource : IDisposable
	{
		public bool isDisposed { get; private set; }

		protected GraphicsResource()
		{
			isDisposed = false;
		}

		~GraphicsResource()
		{
			Dispose(false);
			throw new Exception($"Graphics Resource leaked: {this}");
		}

		public void Dispose()
		{
			// safely handle multiple calls to dispose
			if (isDisposed) return;
			isDisposed = true;
			// dipose this resource
			Dispose(true);
			// prevent the destructor from being called
			GC.SuppressFinalize(this);
			// make sure the garbage collector does not eat our object before it is properly disposed
			GC.KeepAlive(this);
		}

		protected abstract void Dispose(bool manual);

		public static void DisposeAll(object obj)
		{
			// get all fields, including backing fields for properties
			foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				// check if it should be released
				if (typeof(GraphicsResource).IsAssignableFrom(field.FieldType))
				{
					// and release it
					(field.GetValue(obj) as GraphicsResource)?.Dispose();
				}
			}
		}
	}
}
