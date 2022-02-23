using System;

namespace MGE;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PropAttribute : Attribute
{
	public string? name = null;
	public readonly int? order = null;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SaveAttribute : Attribute
{
}
