namespace MGE;

public class HitboxCollider2D : Collider2D
{
	[Prop] Vector2 _size;
	public override Vector2 size => _size;

	public override bool ignoreBounds => true;

	public override float width { get => _size.x; set => _size.x = value; }
	public override float height { get => _size.y; set => _size.y = value; }

	public override float left { get => position.x; set => position.x = value; }
	public override float top { get => position.y; set => position.y = value; }
	public override float right { get => position.x + width; set => position.x = value - width; }
	public override float bottom { get => position.y + height; set => position.y = value - height; }

	public HitboxCollider2D(Vector2 size, Vector2? position = null) : base(position)
	{
		_size = size;
	}

	public bool Intersects(HitboxCollider2D hitbox) => absLeft < hitbox.absRight && absRight > hitbox.absLeft && absBottom > hitbox.absTop && absTop < hitbox.absBottom;
	public bool Intersects(float x, float y, float width, float height) => absRight > x && absBottom > y && absLeft < x + width && absTop < y + height;

	protected override void Render(Batch2D batch, Color color) => batch.HollowRect(bounds, 1, color);

	public void SetFromRectangle(Rect rect)
	{
		position = new Vector2(rect.x, rect.y);
		width = rect.width;
		height = rect.height;
	}

	public void GetTopEdge(out Vector2 from, out Vector2 to)
	{
		from.x = absLeft;
		to.x = absRight;
		from.y = to.y = absTop;
	}

	public void GetBottomEdge(out Vector2 from, out Vector2 to)
	{
		from.x = absLeft;
		to.x = absRight;
		from.y = to.y = absBottom;
	}

	public void GetLeftEdge(out Vector2 from, out Vector2 to)
	{
		from.y = absTop;
		to.y = absBottom;
		from.x = to.x = absLeft;
	}

	public void GetRightEdge(out Vector2 from, out Vector2 to)
	{
		from.y = absTop;
		to.y = absBottom;
		from.x = to.x = absRight;
	}

	public override bool PointCheck(Vector2 point) => Physics.RectVsPoint(absLeft, absTop, width, height, point);
	public override bool RectCheck(Rect rect) => absRight > rect.left && absBottom > rect.top && absLeft < rect.right && absTop < rect.bottom;
	public override bool LineCheck(Vector2 start, Vector2 end) => Physics.RectVsLine(absLeft, absTop, width, height, start, end);
	public override RaycastHit Raycast(Vector2 position, Vector2 direction) => throw new System.NotImplementedException();

	protected override bool? CheckCollider(Collider2D collider)
	{
		if (collider is HitboxCollider2D hitbox)
			return Intersects(hitbox);
		else if (collider is CircleCollider2D circle)
			return Physics.RectVsCircle(absLeft, absTop, width, height, circle.absPosition, circle.radius);
		return null;
	}
}
