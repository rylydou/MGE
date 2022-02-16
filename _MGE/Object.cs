using System;

namespace MGE;

public abstract class Object : IEquatable<Object>
{
	static uint nextInstanceID = 1;

	internal readonly uint instanceID;

	protected Object()
	{
		instanceID = nextInstanceID++;
	}

	public override bool Equals(object? other) => other is Object obj && Equals(obj);
	public bool Equals(Object? other) => other is null ? false : other.instanceID == instanceID;

	public override int GetHashCode() => instanceID.GetHashCode();

	public override string? ToString() => GetType().Name;
}
