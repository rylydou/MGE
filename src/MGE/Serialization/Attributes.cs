using System;

namespace MGE;

/// <summary>
/// [INTERNAL] Marks a value that should be saved and loaded to a file of the node but should not be visible in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal class HiddenPropAttribute : Attribute
{
	public string? name = null;
	public readonly int? order = null;
}

/// <summary>
/// Marks a field to be shown in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PropAttribute : Attribute
{
	public string? name = null;
	public readonly int? order = null;
}

/// <summary>
/// Marks a value that should be saved and loaded when the object is being written to a file.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SaveAttribute : Attribute
{
}
