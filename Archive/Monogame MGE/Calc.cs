namespace MGE
{
	public class Calc
	{
		public static Vector2 ClosestPointOnLine(Vector2 lineA, Vector2 lineB, Vector2 closestTo)
		{
			var v = lineB - lineA;
			var w = closestTo - lineA;
			var time = Vector2.Dot(w, v) / Vector2.Dot(v, v);
			time = Math.Clamp(time, 0, 1);

			return lineA + v * time;
		}
	}
}