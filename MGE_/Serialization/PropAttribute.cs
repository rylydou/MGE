using System;

namespace MGE
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class PropAttribute : Attribute
	{
		public string? name { get; init; }
		public int? order { get; init; }

		public PropAttribute() { }

		public PropAttribute(string name)
		{
			this.name = name;
		}
	}
}
