using System;

namespace MGE
{
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	sealed class IndirectChildOfAttribute : System.Attribute
	{
		public Type type { get; init; }

		public IndirectChildOfAttribute(Type type)
		{
			this.type = type;
		}
	}
}