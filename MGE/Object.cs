

namespace MGE
{
	public abstract class Object : System.IEquatable<Object>
	{
		static int _nextID;

		[Prop] public string name;
		[Prop] public readonly int id;

		protected Object()
		{
			id = _nextID++;
			name = GetType().Name;
		}

		public static bool operator ==(Object left, Object right) => left.id == right.id;
		public static bool operator !=(Object left, Object right) => left.id != right.id;

		public static implicit operator bool(Object obj) => obj is not null;

		public bool Equals(Object other) => other.id == id;
		public override bool Equals(object obj)
		{
			if (obj is Object o) Equals(o);
			return false;
		}

		public override int GetHashCode() => id;

		public override string ToString() => name;
	}
}
