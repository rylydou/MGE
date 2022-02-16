using System;

namespace MGE;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
sealed class PropAttribute : Attribute
{
	public string? name = null;
	public readonly int? order = null;
}
