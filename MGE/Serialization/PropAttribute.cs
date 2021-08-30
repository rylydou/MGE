using System;

namespace MGE.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	sealed class PropAttribute : Attribute
	{
		public string name { get; init; }
		public int order { get; init; }

		public PropAttribute(string name = null, int order = 0)
		{
			this.name = name;
			this.order = order;
		}
	}
}