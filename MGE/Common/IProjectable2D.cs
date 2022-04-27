namespace MGE;

/// <summary>
/// A 2D shape that can be projected onto an Axis
/// </summary>
public interface IProjectable2D
{
	/// <summary>
	/// Projects the shape onto the Axis
	/// </summary>
	void Project(in Vector2 axis, out float min, out float max);
}

public static class IProjectable2DExt
{
	/// <summary>
	/// Checks if two Projectable Shapes overlap on the given Axis, and returns the amount
	/// </summary>
	public static bool AxisOverlaps(this IProjectable2D a, in IProjectable2D b, in Vector2 axis, out float amount)
	{
		a.Project(axis, out float min0, out float max0);
		b.Project(axis, out float min1, out float max1);

		if (Mathf.Abs(min1 - max0) < Mathf.Abs(max1 - min0))
			amount = min1 - max0;
		else
			amount = max1 - min0;

		return (min0 < max1 && max0 > min1);
	}
}
