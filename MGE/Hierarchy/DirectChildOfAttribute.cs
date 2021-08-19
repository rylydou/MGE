using System;

namespace MGE
{
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	sealed class DirectChildOfAttribute : System.Attribute
	{
		public Type type { get; init; }

		public DirectChildOfAttribute(Type type)
		{
			this.type = type;
		}
	}
}