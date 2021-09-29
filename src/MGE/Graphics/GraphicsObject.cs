using System;

namespace MGE.Graphics
{
	public abstract class GraphicsObject : GraphicsResource, IEquatable<GraphicsObject>
	{
		public readonly int handle;

		protected GraphicsObject(int handle)
		{
			this.handle = handle;
		}

		public override string ToString() => string.Format("{0}({1})", GetType().Name, handle);

		public override int GetHashCode() => handle.GetHashCode();

		public bool Equals(GraphicsObject? other) => other is not null && handle.Equals(other.handle);
		public override bool Equals(object? obj) => obj is GraphicsObject go && Equals(go);
	}
}
