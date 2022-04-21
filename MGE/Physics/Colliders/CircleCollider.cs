namespace MGE;

public class CircleCollider2D : Collider2D
{
	[Prop] public float radius;

	public CircleCollider2D(float radius, Vector2? position = null) : base(position)
	{
		this.radius = radius;
	}

	public override float width { get => radius * 2; set => radius = value / 2; }
	public override float height { get => radius * 2; set => radius = value / 2; }
	public override float left { get => position.x - radius; set => position.x = value + radius; }
	public override float top { get => position.y - radius; set => position.y = value + radius; }
	public override float right { get => position.x + radius; set => position.x = value - radius; }
	public override float bottom { get => position.y + radius; set => position.y = value - radius; }

	protected override void Render(Batch2D batch, Color color) => batch.HollowCircle(position, radius, 1, 24, color);

	public override bool PointCheck(Vector2 point) => Physics.CircleVsPoint(absPosition, radius, point);
	public override bool RectCheck(Rect rect) => Physics.RectVsCircle(rect, absPosition, radius);
	public override bool LineCheck(Vector2 from, Vector2 to) => Physics.CircleVsLine(absPosition, radius, from, to);

	protected override bool? CheckCollider(Collider2D collider)
	{
		if (collider is CircleCollider2D circle)
			return Vector2.DistanceSqr(absPosition, circle.absPosition) < (radius + circle.radius) * (radius + circle.radius);
		return null;
	}
}
