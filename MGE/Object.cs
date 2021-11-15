using System;

namespace MGE;

public abstract class Object : IEquatable<Object>
{
	public readonly Guid guid;

	protected Object()
	{
		guid = Guid.NewGuid();
	}

	public override bool Equals(object? other) => other is Object obj && Equals(obj);
	public bool Equals(Object? other) => other?.guid.Equals(guid) ?? false;

	public override int GetHashCode() => guid.GetHashCode();

	public override string? ToString() => $"{GetType().Name}#{guid}";
}