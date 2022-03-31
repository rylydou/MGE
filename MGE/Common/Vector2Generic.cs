using System;
using System.Collections.Generic;

namespace MGE;

public struct Vector2<T> : IEquatable<Vector2<T>>
{
	public T horizontal;
	public T vertical;

	public Vector2(T horizontal, T vertical)
	{
		this.horizontal = horizontal;
		this.vertical = vertical;
	}

	public T this[int index]
	{
		get => index switch { 0 => horizontal, 1 => vertical, _ => throw new ArgumentOutOfRangeException(nameof(index)) };
		set
		{
			switch (index)
			{
				case 0: horizontal = value; break;
				case 1: vertical = value; break;
				default: throw new ArgumentOutOfRangeException(nameof(index));
			}
		}
	}

	public Vector2<T> With(int index, T value) => index switch
	{
		0 => new(value, vertical),
		1 => new(horizontal, value),
		_ => throw new ArgumentOutOfRangeException(nameof(index)),
	};

	public override bool Equals(object? obj) => obj is Vector2<T> vec && Equals(vec);
	public bool Equals(Vector2<T> other)
	{
		return
			EqualityComparer<T>.Default.Equals(horizontal, other.horizontal) &&
			EqualityComparer<T>.Default.Equals(vertical, other.vertical);
	}

	public static bool operator ==(Vector2<T> left, Vector2<T> right) => left.Equals(right);
	public static bool operator !=(Vector2<T> left, Vector2<T> right) => !(left == right);

	public override int GetHashCode()
	{
		return HashCode.Combine(horizontal, vertical);
	}
}
