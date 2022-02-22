using System;

namespace MGE;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
class MemberAttribute : Attribute
{
	public string? name = null;
	public readonly int? order = null;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class PropAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class SaveAttribute : Attribute
{
}
