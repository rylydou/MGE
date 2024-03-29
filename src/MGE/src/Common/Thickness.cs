using System;

namespace MGE.UI;

public struct Thickness : IEquatable<Thickness>
{
	public int left;
	public int top;
	public int right;
	public int bottom;

	public int this[int index]
	{
		get => index switch
		{
			0 => left,
			1 => top,
			2 => right,
			3 => bottom,
			_ => throw new IndexOutOfRangeException($"Invalid {nameof(Thickness)} index of {index}!"),
		};
		set
		{
			switch (index)
			{
				case 0: left = value; break;
				case 1: top = value; break;
				case 2: right = value; break;
				case 3: bottom = value; break;
				default: throw new IndexOutOfRangeException($"Invalid {nameof(Thickness)} index of {index}!");
			}
		}
	}

	public int width { get => left + right; }
	public int height { get => top + bottom; }

	public Thickness(int top, int bottom, int left, int right)
	{
		this.top = top;
		this.bottom = bottom;
		this.left = left;
		this.right = right;
	}

	public Thickness(int all)
	{
		this.top = all;
		this.bottom = all;
		this.left = all;
		this.right = all;
	}

	public int GetAlongAxis(int index) => index switch
	{
		0 => left + right,
		1 => top + bottom,
		_ => throw new ArgumentOutOfRangeException(nameof(index)),
	};

	public static RectInt operator -(RectInt a, Thickness b)
	{
		var result = a;
		result.x += b.left;
		result.y += b.top;

		result.width -= b.width;
		if (result.width < 0) result.width = 0;

		result.height -= b.height;
		if (result.height < 0) result.height = 0;

		return result;
	}

	public static bool operator ==(Thickness a, Thickness b) => a.Equals(b);
	public static bool operator !=(Thickness a, Thickness b) => !a.Equals(b);

	public override bool Equals(object? obj) => obj is Thickness thickness && Equals(thickness);
	public bool Equals(Thickness other) => top == other.top && bottom == other.bottom && left == other.left && right == other.right;

	public override int GetHashCode() => HashCode.Combine(top, bottom, left, right);
}
