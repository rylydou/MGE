using System.Reflection;

namespace MGE
{
	public abstract class Resource : System.IDisposable
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

			System.GC.SuppressFinalize(this);
			System.GC.KeepAlive(this);
		}

		protected abstract void Dispose(bool manual);

		public static void DisposeAll(object obj)
		{
			// get all fields, including backing fields for properties
			foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				// check if it should be released
				if (typeof(Resource).IsAssignableFrom(field.FieldType))
				{
					// and release it
					((Resource)field.GetValue(obj)).Dispose();
				}
			}
		}
	}
}