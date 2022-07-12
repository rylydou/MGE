using System.Collections.Generic;

namespace MGE;

[System.Flags]
public enum PointSectors
{
	Center = 0,
	Top = 1,
	TopRight = 5,
	Right = 4,
	BottomRight = 6,
	Bottom = 2,
	BottomLeft = 10,
	Left = 8,
	TopLeft = 9,
};

public static class Physics
{
	#region CollisionObject2D

	public static bool Check(Body2D a, Body2D b) => a.collider is not null && b.collider is not null && a != b && b.collidable && a.collider.Collide(b.collider);

	public static bool Check(Body2D a, Body2D b, Vector2 at)
	{
		Vector2 position = a.position;
		a.position = at;
		int num = Physics.Check(a, b) ? 1 : 0;
		a.position = position;
		return num != 0;
	}

	public static bool Check(Body2D a, IEnumerable<Body2D> b)
	{
		foreach (Body2D b1 in b)
		{
			if (Physics.Check(a, b1))
				return true;
		}
		return false;
	}

	public static bool Check(Body2D a, IEnumerable<Body2D> b, Vector2 at)
	{
		Vector2 position = a.position;
		a.position = at;
		int num = Physics.Check(a, b) ? 1 : 0;
		a.position = position;
		return num != 0;
	}

	public static Body2D? First(Body2D a, IEnumerable<Body2D> b)
	{
		foreach (Body2D b1 in b)
		{
			if (Physics.Check(a, b1))
				return b1;
		}
		return null;
	}

	public static Body2D? First(Body2D a, IEnumerable<Body2D> b, Vector2 at)
	{
		var position = a.position;
		a.position = at;
		var entity = Physics.First(a, b);
		a.position = position;
		return entity;
	}

	public static List<Body2D> All(Body2D a, IEnumerable<Body2D> b, List<Body2D> into)
	{
		foreach (Body2D b1 in b)
		{
			if (Physics.Check(a, b1))
				into.Add(b1);
		}
		return into;
	}

	public static List<Body2D> All(
		Body2D a,
		IEnumerable<Body2D> b,
		List<Body2D> into,
		Vector2 at)
	{
		Vector2 position = a.position;
		a.position = at;
		List<Body2D> entityList = Physics.All(a, b, into);
		a.position = position;
		return entityList;
	}

	public static List<Body2D> All(Body2D a, IEnumerable<Body2D> b) => Physics.All(a, b, new List<Body2D>());

	public static List<Body2D> All(Body2D a, IEnumerable<Body2D> b, Vector2 at) => Physics.All(a, b, new List<Body2D>(), at);

	public static bool CheckPoint(Body2D a, Vector2 point) => a.collider is not null && a.collider.PointCheck(point);

	public static bool CheckPoint(Body2D a, Vector2 point, Vector2 at)
	{
		Vector2 position = a.position;
		a.position = at;
		int num = Physics.CheckPoint(a, point) ? 1 : 0;
		a.position = position;
		return num != 0;
	}

	public static bool CheckLine(Body2D a, Vector2 from, Vector2 to) => a.collider is not null && a.collider.LineCheck(from, to);

	public static bool CheckLine(Body2D a, Vector2 from, Vector2 to, Vector2 at)
	{
		Vector2 position = a.position;
		a.position = at;
		int num = Physics.CheckLine(a, from, to) ? 1 : 0;
		a.position = position;
		return num != 0;
	}

	public static bool CheckRect(Body2D a, Rect rect) => a.collider is not null && a.collider.RectCheck(rect);

	public static bool CheckRect(Body2D a, Rect rect, Vector2 at)
	{
		Vector2 position = a.position;
		a.position = at;
		int num = Physics.CheckRect(a, rect) ? 1 : 0;
		a.position = position;
		return num != 0;
	}

	#endregion CollisionObject2D

	#region Line

	public static bool LineVsLine(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
	{
		var b = end1 - start1;
		var d = end2 - start2;
		var bDotDPerp = b.x * d.y - b.y * d.x;

		if (bDotDPerp == 0) return false;

		var c = start2 - start1;
		var t = (c.x * d.y - c.y * d.x) / bDotDPerp;
		if (t < 0 || t > 1) return false;

		var u = (c.x * b.y - c.y * b.x) / bDotDPerp;
		if (u < 0 || u > 1) return false;
		return true;
	}

	public static bool LineVsLine(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 intersection)
	{
		intersection = Vector2.zero;

		var b = end1 - start1;
		var d = end2 - start2;
		var bDotDPerp = b.x * d.y - b.y * d.x;

		if (bDotDPerp == 0) return false;

		var c = start2 - start1;
		var t = (c.x * d.y - c.y * d.x) / bDotDPerp;
		if (t < 0 || t > 1) return false;

		var u = (c.x * b.y - c.y * b.x) / bDotDPerp;
		if (u < 0 || u > 1) return false;

		intersection = start1 + t * b;

		return true;
	}

	#endregion

	#region Circle

	public static bool CircleVsLine(Vector2 position, float radius, Vector2 start, Vector2 end) => Vector2.DistanceSqr(position, Calc.ClosestPointOnLine(start, end, position)) < radius * radius;
	public static bool CircleVsPoint(Vector2 position, float radius, Vector2 point) => Vector2.DistanceSqr(position, point) < radius * radius;
	public static bool CircleVsRect(Vector2 position, float radius, float x, float y, float width, float height) => RectVsCircle(x, y, width, height, position, radius);
	public static bool CircleVsRect(Vector2 position, float radius, Rect rect) => RectVsCircle(rect, position, radius);

	#endregion

	#region Rect

	public static bool RectVsCircle(Rect rect, Vector2 cPosition, float cRadius) => RectVsCircle(rect.x, rect.y, rect.width, rect.height, cPosition, cRadius);
	public static bool RectVsCircle(float x, float y, float width, float height, Vector2 position, float radius)
	{
		if (Physics.RectVsPoint(x, y, width, height, position)) return true;

		Vector2 edgeFrom;
		Vector2 edgeTo;
		var sector = GetSector(x, y, width, height, position);

		if ((sector & PointSectors.Top) != 0)
		{
			edgeFrom = new Vector2(x, y);
			edgeTo = new Vector2(x + width, y);
			if (CircleVsLine(position, radius, edgeFrom, edgeTo)) return true;
		}

		if ((sector & PointSectors.Bottom) != 0)
		{
			edgeFrom = new Vector2(x, y + height);
			edgeTo = new Vector2(x + width, y + height);
			if (CircleVsLine(position, radius, edgeFrom, edgeTo)) return true;
		}

		if ((sector & PointSectors.Left) != 0)
		{
			edgeFrom = new Vector2(x, y);
			edgeTo = new Vector2(x, y + height);
			if (CircleVsLine(position, radius, edgeFrom, edgeTo)) return true;
		}

		if ((sector & PointSectors.Right) != 0)
		{
			edgeFrom = new Vector2(x + width, y);
			edgeTo = new Vector2(x + width, y + height);
			if (CircleVsLine(position, radius, edgeFrom, edgeTo)) return true;
		}

		return false;
	}

	public static bool RectVsLine(Rect rect, Vector2 start, Vector2 end) => RectVsLine(rect.x, rect.y, rect.width, rect.height, start, end);
	public static bool RectVsLine(float x, float y, float width, float height, Vector2 start, Vector2 end)
	{
		var fromSector = Physics.GetSector(x, y, width, height, start);
		var toSector = Physics.GetSector(x, y, width, height, end);

		if (fromSector == PointSectors.Center || toSector == PointSectors.Center) return true;
		else if ((fromSector & toSector) != 0) return false;
		else
		{
			Vector2 edgeFrom;
			Vector2 edgeTo;
			var both = fromSector | toSector;

			if ((both & PointSectors.Top) != 0)
			{
				edgeFrom = new Vector2(x, y);
				edgeTo = new Vector2(x + width, y);
				if (Physics.LineVsLine(edgeFrom, edgeTo, start, end)) return true;
			}

			if ((both & PointSectors.Bottom) != 0)
			{
				edgeFrom = new Vector2(x, y + height);
				edgeTo = new Vector2(x + width, y + height);
				if (Physics.LineVsLine(edgeFrom, edgeTo, start, end)) return true;
			}

			if ((both & PointSectors.Left) != 0)
			{
				edgeFrom = new Vector2(x, y);
				edgeTo = new Vector2(x, y + height);
				if (Physics.LineVsLine(edgeFrom, edgeTo, start, end)) return true;
			}

			if ((both & PointSectors.Right) != 0)
			{
				edgeFrom = new Vector2(x + width, y);
				edgeTo = new Vector2(x + width, y + height);
				if (Physics.LineVsLine(edgeFrom, edgeTo, start, end)) return true;
			}
		}
		return false;
	}

	public static bool RectVsPoint(Rect rect, Vector2 point) => RectVsPoint(rect.x, rect.y, rect.width, rect.height, point);
	public static bool RectVsPoint(float x, float y, float width, float height, Vector2 point) => point.x >= x && point.y >= y && point.x < x + width && point.y < y + height;

	#endregion

	#region Sectors

	public static PointSectors GetSector(Rect rect, Vector2 point)
	{
		var sector = PointSectors.Center;

		if (point.x < rect.left)
			sector |= PointSectors.Left;
		else if (point.x >= rect.right)
			sector |= PointSectors.Right;

		if (point.y < rect.top)
			sector |= PointSectors.Top;
		else if (point.y >= rect.bottom)
			sector |= PointSectors.Bottom;

		return sector;
	}

	public static PointSectors GetSector(float x, float y, float width, float height, Vector2 point)
	{
		var sector = PointSectors.Center;

		if (point.x < x)
			sector |= PointSectors.Left;
		else if (point.x >= x + width)
			sector |= PointSectors.Right;

		if (point.y < y)
			sector |= PointSectors.Top;
		else if (point.y >= y + height)
			sector |= PointSectors.Bottom;

		return sector;
	}

	#endregion
}
