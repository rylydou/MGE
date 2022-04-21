using System.Collections.Generic;
using System.Linq;

namespace MGE;

public abstract class Body2D : Node2D
{
	[Prop] public bool collidable = true;
	[Prop] public Layer layer = new Layer(1);

	[Prop] public Collider2D? collider;

	// public bool OnGround(int downCheck = 1)
	// {
	// 	if (this.CollideCheck<Solid>(this.globalPosition + Vector2.down * (float)downCheck))
	// 		return true;
	// 	return !this.IgnoreJumpThrus && this.CollideCheckOutside<JumpThru>(this.position + Vector2.down * (float)downCheck);
	// }

	// public bool OnGround(Vector2 at, int downCheck = 1)
	// {
	// 	Vector2 position = this.position;
	// 	this.position = at;
	// 	int num = this.OnGround(downCheck) ? 1 : 0;
	// 	this.position = position;
	// 	return num != 0;
	// }

	#region Check

	public bool CollideCheck(Body2D other) => Physics.Check(this, other);
	public bool CollideCheck(Body2D other, Vector2 at) => Physics.Check(this, other, at);

	public bool CollideCheck(Layer layer) => Physics.Check(this, scene!.bodies.WhereInLayer(layer));
	public bool CollideCheck(Layer layer, Vector2 at) => Physics.Check(this, scene!.bodies.WhereInLayer(layer), at);

	public bool CollideCheck<T>() where T : Body2D => Physics.Check(this, scene!.bodies.Where(body => body is T));
	public bool CollideCheck<T>(Vector2 at) where T : Body2D => Physics.Check(this, scene!.bodies.Where(body => body is T), at);

	public bool CollideCheckOutside(Body2D other, Vector2 at) => !Physics.Check(this, other) && Physics.Check(this, other, at);
	public bool CollideCheckOutside(Layer layer, Vector2 at)
	{
		foreach (var body in scene!.bodies.WhereInLayer(layer))
		{
			if (CollideCheckOutside(body, at))
				return true;
		}
		return false;
	}

	#endregion Check

	#region First

	public Body2D? CollideFirst() => Physics.First(this, scene!.bodies);
	public Body2D? CollideFirst(Vector2 at) => Physics.First(this, scene!.bodies, at);

	public Body2D? CollideFirst(Layer layer) => Physics.First(this, scene!.bodies.WhereInLayer(layer));
	public Body2D? CollideFirst(Layer layer, Vector2 at) => Physics.First(this, scene!.bodies.WhereInLayer(layer), at);

	public T? CollideFirst<T>() where T : Body2D => Physics.First(this, scene!.bodies.Where(body => body is T)) as T;
	public T? CollideFirst<T>(Vector2 at) where T : Body2D => Physics.First(this, scene!.bodies.Where(body => body is T), at) as T;

	public Body2D? CollideFirstOutside(Vector2 at)
	{
		foreach (var body in scene!.bodies)
		{
			if (CollideCheckOutside(body, at))
				return body;
		}
		return null;
	}
	public Body2D? CollideFirstOutside(Layer layer, Vector2 at)
	{
		foreach (var body in scene!.bodies.WhereInLayer(layer))
		{
			if (CollideCheckOutside(body, at))
				return body;
		}
		return null;
	}
	public T? CollideFirstOutside<T>(Vector2 at) where T : Body2D
	{
		foreach (var body in scene!.bodies)
		{
			if (CollideCheckOutside(body, at))
				return body as T;
		}
		return null;
	}

	#endregion First

	#region All

	public IEnumerable<Body2D> CollideAll(Layer layer) => scene!.bodies.WhereInLayer(layer).Where(body => CollideCheck(body));
	public IEnumerable<Body2D> CollideAll(Layer layer, Vector2 at) => scene!.bodies.WhereInLayer(layer).Where(body => CollideCheck(body, at));

	public IEnumerable<T> CollideAll<T>() where T : Body2D => (IEnumerable<T>)scene!.bodies.Where(body => body is T && CollideCheck(body));
	public IEnumerable<T> CollideAll<T>(Vector2 at) where T : Body2D => (IEnumerable<T>)scene!.bodies.Where(body => body is T && CollideCheck(body, at));

	#endregion All

	#region Shapes

	public bool CollidePoint(Vector2 point) => Physics.CheckPoint(this, point);
	public bool CollidePoint(Vector2 point, Vector2 at) => Physics.CheckPoint(this, point, at);

	public bool CollideLine(Vector2 from, Vector2 to) => Physics.CheckLine(this, from, to);
	public bool CollideLine(Vector2 from, Vector2 to, Vector2 at) => Physics.CheckLine(this, from, to, at);

	public bool CollideRect(Rect rect) => Physics.CheckRect(this, rect);
	public bool CollideRect(Rect rect, Vector2 at) => Physics.CheckRect(this, rect, at);

	#endregion Shapes

	#region Utilities

	// protected bool TrySquishWiggle(CollisionData info, int wiggleX = 3, int wiggleY = 3)
	// {
	// 	info.Pusher.Collidable = true;
	// 	for (int index1 = 0; index1 <= wiggleX; ++index1)
	// 	{
	// 		for (int index2 = 0; index2 <= wiggleY; ++index2)
	// 		{
	// 			if (index1 != 0 || index2 != 0)
	// 			{
	// 				for (int index3 = 1; index3 >= -1; index3 -= 2)
	// 				{
	// 					for (int index4 = 1; index4 >= -1; index4 -= 2)
	// 					{
	// 						Vector2 vector2 = new Vector2((float)(index1 * index3), (float)(index2 * index4));
	// 						if (!this.CollideCheck<Solid>(this.position + vector2))
	// 						{
	// 							this.position = this.position + vector2;
	// 							info.Pusher.Collidable = false;
	// 							return true;
	// 						}
	// 					}
	// 				}
	// 			}
	// 		}
	// 	}
	// 	for (int index1 = 0; index1 <= wiggleX; ++index1)
	// 	{
	// 		for (int index2 = 0; index2 <= wiggleY; ++index2)
	// 		{
	// 			if (index1 != 0 || index2 != 0)
	// 			{
	// 				for (int index3 = 1; index3 >= -1; index3 -= 2)
	// 				{
	// 					for (int index4 = 1; index4 >= -1; index4 -= 2)
	// 					{
	// 						Vector2 vector2 = new Vector2((float)(index1 * index3), (float)(index2 * index4));
	// 						if (!this.CollideCheck<Solid>(info.TargetPosition + vector2))
	// 						{
	// 							this.position = info.TargetPosition + vector2;
	// 							info.Pusher.Collidable = false;
	// 							return true;
	// 						}
	// 					}
	// 				}
	// 			}
	// 		}
	// 	}
	// 	info.Pusher.Collidable = false;
	// 	return false;
	// }

	public Body2D? Closest(Layer layer)
	{
		Body2D? closest = null;
		var closestDistanceSqr = float.PositiveInfinity;

		foreach (var body in scene!.bodies.WhereInLayer(layer))
		{
			var distanceSqr = Vector2.DistanceSqr(body.globalPosition, body.globalPosition);
			if (distanceSqr >= closestDistanceSqr) continue;
			closest = body;
			closestDistanceSqr = distanceSqr;
		}

		return closest;
	}

	#endregion Utilities
}
