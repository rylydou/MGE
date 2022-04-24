using System.Collections.Generic;
using System.Linq;

namespace MGE;

public abstract class Body2D : Node2D
{
	[Prop] public bool collidable = true;
	[Prop] public Layer layer = new Layer(1);

	[Prop] public Collider2D? collider { get; private set; }

	public void SetCollider(Collider2D? collider)
	{
		if (this.collider == collider) return;

		if (collider is not null)
		{
			if (collider.node is not null)
				throw new Exception("Set collider", "Collider already has an owner");

			collider.node = this;
		}

		if (this.collider is not null)
		{
			this.collider.node = null;
		}

		this.collider = collider;
	}

	#region Check

	public bool CollideCheck(Body2D other) => Physics.Check(this, other);
	public bool CollideCheck(Body2D other, Vector2 at) => Physics.Check(this, other, at);

	public bool CollideCheck(Layer layer) => Physics.Check(this, scene!.bodies.WhereInLayer(layer));
	public bool CollideCheck(Layer layer, Vector2 at) => Physics.Check(this, scene!.bodies.WhereInLayer(layer), at);

	public bool CollideCheck<T>() where T : Body2D => Physics.Check(this, scene!.bodies.Where(b => b is T));
	public bool CollideCheck<T>(Vector2 at) where T : Body2D => Physics.Check(this, scene!.bodies.Where(b => b is T), at);

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
	public bool CollideCheckOutside<T>(Vector2 at) where T : Body2D
	{
		foreach (var body in scene!.bodies.Where(body => body is T))
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

	public T? CollideFirst<T>() where T : Body2D => Physics.First(this, scene!.bodies.Where(b => b is T)) as T;
	public T? CollideFirst<T>(Vector2 at) where T : Body2D => Physics.First(this, scene!.bodies.Where(b => b is T), at) as T;

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

	public IEnumerable<Body2D> CollideAll(Layer layer) => scene!.bodies.WhereInLayer(layer).Where(b => CollideCheck(b));
	public IEnumerable<Body2D> CollideAll(Layer layer, Vector2 at) => scene!.bodies.WhereInLayer(layer).Where(b => CollideCheck(b, at));

	public IEnumerable<T> CollideAll<T>() where T : Body2D => scene!.bodies.Where(b => b is T && CollideCheck(b)).Select(b => (T)b);
	public IEnumerable<T> CollideAll<T>(Vector2 at) where T : Body2D => scene!.bodies.Where(b => b is T && CollideCheck(b, at)).Select(b => (T)b);

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
