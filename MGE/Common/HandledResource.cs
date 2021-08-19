namespace MGE
{
	public abstract class HandledResource : Resource
	{
		public int handle;

		protected HandledResource(int handle)
		{
			this.handle = handle;
		}

		~HandledResource()
		{
			Dispose(false);

#if DEBUG
			throw new System.Exception($"Handled Resource leaked: {this}#{handle}");
#endif
		}

		public override bool Equals(object obj) => obj is HandledResource && Equals((HandledResource)obj);

		public bool Equals(HandledResource other) => other != null && handle.Equals(other.handle);

		public override int GetHashCode() => handle.GetHashCode();

		public override string ToString() => $"{GetType().Name}{handle}";

		public static implicit operator int(HandledResource resource) => resource.handle;
	}
}