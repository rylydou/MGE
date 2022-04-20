// using System;
// using System.Collections.Generic;

// namespace MGE;

// [Flags]
// public enum PointSectors { Center = 0, Top = 1, Bottom = 2, TopLeft = 9, TopRight = 5, Left = 8, Right = 4, BottomLeft = 10, BottomRight = 6 };

// public static class Collide
// {
// 	#region Entity vs Entity

// 	public static bool Check(CollisionObject2D a, CollisionObject2D b)
// 	{
// 		if (a.collider is null || b.collider is null)
// 			return false;
// 		else
// 			return a != b && b.Collidable && a.collider.Collide(b);
// 	}

// 	public static bool Check(CollisionObject2D a, CollisionObject2D b, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		bool ret = Check(a, b);
// 		a.position = old;
// 		return ret;
// 	}

// 	public static bool Check(CollisionObject2D a, CollidableComponent b)
// 	{
// 		if (a.collider is null || b.collider is null)
// 			return false;
// 		else
// 			return b.Collidable && b.CollisionObject2D.Collidable && a.collider.Collide(b);
// 	}

// 	public static bool Check(CollisionObject2D a, CollidableComponent b, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		bool ret = Check(a, b);
// 		a.position = old;
// 		return ret;
// 	}

// 	#endregion

// 	#region Entity vs Entity Enumerable

// 	#region Check

// 	public static bool Check(CollisionObject2D a, IEnumerable<CollisionObject2D> b)
// 	{
// 		foreach (var e in b)
// 			if (Check(a, e))
// 				return true;

// 		return false;
// 	}

// 	public static bool Check(CollisionObject2D a, IEnumerable<CollisionObject2D> b, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		bool ret = Check(a, b);
// 		a.position = old;
// 		return ret;
// 	}

// 	#endregion

// 	#region First

// 	public static CollisionObject2D First(CollisionObject2D a, IEnumerable<CollisionObject2D> b)
// 	{
// 		foreach (var e in b)
// 			if (Check(a, e))
// 				return e;

// 		return null;
// 	}

// 	public static CollisionObject2D First(CollisionObject2D a, IEnumerable<CollisionObject2D> b, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		CollisionObject2D ret = First(a, b);
// 		a.position = old;
// 		return ret;
// 	}

// 	#endregion

// 	#region All

// 	public static List<CollisionObject2D> All(CollisionObject2D a, IEnumerable<CollisionObject2D> b, List<CollisionObject2D> into)
// 	{
// 		foreach (var e in b)
// 			if (Check(a, e))
// 				into.Add(e);

// 		return into;
// 	}

// 	public static List<CollisionObject2D> All(CollisionObject2D a, IEnumerable<CollisionObject2D> b, List<CollisionObject2D> into, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		List<CollisionObject2D> ret = All(a, b, into);
// 		a.position = old;
// 		return ret;
// 	}

// 	public static List<CollisionObject2D> All(CollisionObject2D a, IEnumerable<CollisionObject2D> b)
// 	{
// 		return All(a, b, new List<CollisionObject2D>());
// 	}

// 	public static List<CollisionObject2D> All(CollisionObject2D a, IEnumerable<CollisionObject2D> b, Vector2 at)
// 	{
// 		return All(a, b, new List<CollisionObject2D>(), at);
// 	}

// 	#endregion

// 	#endregion

// 	#region Entity vs Point

// 	public static bool CheckPoint(CollisionObject2D a, Vector2 point)
// 	{
// 		if (a.collider is null)
// 			return false;
// 		else
// 			return a.collider.Collide(point);
// 	}

// 	public static bool CheckPoint(CollisionObject2D a, Vector2 point, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		bool ret = CheckPoint(a, point);
// 		a.position = old;
// 		return ret;
// 	}

// 	#endregion

// 	#region Entity vs Line

// 	public static bool CheckLine(CollisionObject2D a, Vector2 from, Vector2 to)
// 	{
// 		if (a.collider is null)
// 			return false;
// 		else
// 			return a.collider.LineCheck(from, to);
// 	}

// 	public static bool CheckLine(CollisionObject2D a, Vector2 from, Vector2 to, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		bool ret = CheckLine(a, from, to);
// 		a.position = old;
// 		return ret;
// 	}

// 	#endregion

// 	#region Entity vs Rectangle

// 	public static bool CheckRect(CollisionObject2D a, Rect rect)
// 	{
// 		if (a.collider is null)
// 			return false;
// 		else
// 			return a.collider.RectCheck(rect);
// 	}

// 	public static bool CheckRect(CollisionObject2D a, Rect rect, Vector2 at)
// 	{
// 		Vector2 old = a.position;
// 		a.position = at;
// 		bool ret = CheckRect(a, rect);
// 		a.position = old;
// 		return ret;
// 	}

// 	#endregion

// 	#region Line

// 	public static bool LineCheck(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
// 	{
// 		Vector2 b = a2 - a1;
// 		Vector2 d = b2 - b1;
// 		float bDotDPerp = b.x * d.y - b.y * d.x;

// 		// if b dot d == 0, it means the lines are parallel so have infinite intersection points
// 		if (bDotDPerp == 0)
// 			return false;

// 		Vector2 c = b1 - a1;
// 		float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
// 		if (t < 0 || t > 1)
// 			return false;

// 		float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
// 		if (u < 0 || u > 1)
// 			return false;

// 		return true;
// 	}

// 	public static bool LineCheck(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
// 	{
// 		intersection = Vector2.zero;

// 		Vector2 b = a2 - a1;
// 		Vector2 d = b2 - b1;
// 		float bDotDPerp = b.x * d.y - b.y * d.x;

// 		// if b dot d == 0, it means the lines are parallel so have infinite intersection points
// 		if (bDotDPerp == 0)
// 			return false;

// 		Vector2 c = b1 - a1;
// 		float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
// 		if (t < 0 || t > 1)
// 			return false;

// 		float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
// 		if (u < 0 || u > 1)
// 			return false;

// 		intersection = a1 + t * b;

// 		return true;
// 	}

// 	#endregion

// 	#region Circle

// 	public static bool CircleToLine(Vector2 cPosiition, float cRadius, Vector2 lineFrom, Vector2 lineTo)
// 	{
// 		return Vector2.DistanceSqr(cPosiition, Calc.ClosestPointOnLine(lineFrom, lineTo, cPosiition)) < cRadius * cRadius;
// 	}

// 	public static bool CircleToPoint(Vector2 cPosition, float cRadius, Vector2 point)
// 	{
// 		return Vector2.DistanceSqr(cPosition, point) < cRadius * cRadius;
// 	}

// 	public static bool CircleToRect(Vector2 cPosition, float cRadius, float rX, float rY, float rW, float rH)
// 	{
// 		return RectToCircle(rX, rY, rW, rH, cPosition, cRadius);
// 	}

// 	public static bool CircleToRect(Vector2 cPosition, float cRadius, Rect rect)
// 	{
// 		return RectToCircle(rect, cPosition, cRadius);
// 	}

// 	#endregion

// 	#region Rect

// 	public static bool RectToCircle(float rX, float rY, float rW, float rH, Vector2 cPosition, float cRadius)
// 	{
// 		//Check if the rectangle contains the circle's center point
// 		if (Collide.RectToPoint(rX, rY, rW, rH, cPosition))
// 			return true;

// 		//Check the circle against the relevant edges
// 		Vector2 edgeFrom;
// 		Vector2 edgeTo;
// 		PointSectors sector = GetSector(rX, rY, rW, rH, cPosition);

// 		if ((sector & PointSectors.Top) != 0)
// 		{
// 			edgeFrom = new Vector2(rX, rY);
// 			edgeTo = new Vector2(rX + rW, rY);
// 			if (CircleToLine(cPosition, cRadius, edgeFrom, edgeTo))
// 				return true;
// 		}

// 		if ((sector & PointSectors.Bottom) != 0)
// 		{
// 			edgeFrom = new Vector2(rX, rY + rH);
// 			edgeTo = new Vector2(rX + rW, rY + rH);
// 			if (CircleToLine(cPosition, cRadius, edgeFrom, edgeTo))
// 				return true;
// 		}

// 		if ((sector & PointSectors.Left) != 0)
// 		{
// 			edgeFrom = new Vector2(rX, rY);
// 			edgeTo = new Vector2(rX, rY + rH);
// 			if (CircleToLine(cPosition, cRadius, edgeFrom, edgeTo))
// 				return true;
// 		}

// 		if ((sector & PointSectors.Right) != 0)
// 		{
// 			edgeFrom = new Vector2(rX + rW, rY);
// 			edgeTo = new Vector2(rX + rW, rY + rH);
// 			if (CircleToLine(cPosition, cRadius, edgeFrom, edgeTo))
// 				return true;
// 		}

// 		return false;
// 	}

// 	public static bool RectToCircle(Rect rect, Vector2 cPosition, float cRadius)
// 	{
// 		return RectToCircle(rect.x, rect.y, rect.width, rect.height, cPosition, cRadius);
// 	}

// 	public static bool RectToLine(float rX, float rY, float rW, float rH, Vector2 lineFrom, Vector2 lineTo)
// 	{
// 		PointSectors fromSector = Collide.GetSector(rX, rY, rW, rH, lineFrom);
// 		PointSectors toSector = Collide.GetSector(rX, rY, rW, rH, lineTo);

// 		if (fromSector == PointSectors.Center || toSector == PointSectors.Center)
// 			return true;
// 		else if ((fromSector & toSector) != 0)
// 			return false;
// 		else
// 		{
// 			PointSectors both = fromSector | toSector;

// 			//Do line checks against the edges
// 			Vector2 edgeFrom;
// 			Vector2 edgeTo;

// 			if ((both & PointSectors.Top) != 0)
// 			{
// 				edgeFrom = new Vector2(rX, rY);
// 				edgeTo = new Vector2(rX + rW, rY);
// 				if (Collide.LineCheck(edgeFrom, edgeTo, lineFrom, lineTo))
// 					return true;
// 			}

// 			if ((both & PointSectors.Bottom) != 0)
// 			{
// 				edgeFrom = new Vector2(rX, rY + rH);
// 				edgeTo = new Vector2(rX + rW, rY + rH);
// 				if (Collide.LineCheck(edgeFrom, edgeTo, lineFrom, lineTo))
// 					return true;
// 			}

// 			if ((both & PointSectors.Left) != 0)
// 			{
// 				edgeFrom = new Vector2(rX, rY);
// 				edgeTo = new Vector2(rX, rY + rH);
// 				if (Collide.LineCheck(edgeFrom, edgeTo, lineFrom, lineTo))
// 					return true;
// 			}

// 			if ((both & PointSectors.Right) != 0)
// 			{
// 				edgeFrom = new Vector2(rX + rW, rY);
// 				edgeTo = new Vector2(rX + rW, rY + rH);
// 				if (Collide.LineCheck(edgeFrom, edgeTo, lineFrom, lineTo))
// 					return true;
// 			}
// 		}

// 		return false;
// 	}

// 	public static bool RectToLine(Rect rect, Vector2 lineFrom, Vector2 lineTo)
// 	{
// 		return RectToLine(rect.x, rect.y, rect.width, rect.height, lineFrom, lineTo);
// 	}

// 	public static bool RectToPoint(float rX, float rY, float rW, float rH, Vector2 point)
// 	{
// 		return point.x >= rX && point.y >= rY && point.x < rX + rW && point.y < rY + rH;
// 	}

// 	public static bool RectToPoint(Rect rect, Vector2 point)
// 	{
// 		return RectToPoint(rect.x, rect.y, rect.width, rect.height, point);
// 	}

// 	#endregion

// 	#region Sectors

// 	/*
// 	 *  Bitflags and helpers for using the Cohen–Sutherland algorithm
// 	 *  http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
// 	 *
// 	 *  Sector bitflags:
// 	 *      1001  1000  1010
// 	 *      0001  0000  0010
// 	 *      0101  0100  0110
// 	 */

// 	public static PointSectors GetSector(Rect rect, Vector2 point)
// 	{
// 		PointSectors sector = PointSectors.Center;

// 		if (point.x < rect.left)
// 			sector |= PointSectors.Left;
// 		else if (point.x >= rect.right)
// 			sector |= PointSectors.Right;

// 		if (point.y < rect.top)
// 			sector |= PointSectors.Top;
// 		else if (point.y >= rect.bottom)
// 			sector |= PointSectors.Bottom;

// 		return sector;
// 	}

// 	public static PointSectors GetSector(float rX, float rY, float rW, float rH, Vector2 point)
// 	{
// 		PointSectors sector = PointSectors.Center;

// 		if (point.x < rX)
// 			sector |= PointSectors.Left;
// 		else if (point.x >= rX + rW)
// 			sector |= PointSectors.Right;

// 		if (point.y < rY)
// 			sector |= PointSectors.Top;
// 		else if (point.y >= rY + rH)
// 			sector |= PointSectors.Bottom;

// 		return sector;
// 	}

// 	#endregion
// }
