

namespace MGE
{
	public abstract class Collider
	{
		public CollidableNode node { get; internal set; }

		[Prop] public Vector2 position;

		public virtual bool ignoreBounds { get => false; }

		protected Collider(Vector2? position = null)
		{
			if (!position.HasValue) position = Vector2.zero;
			this.position = position.Value;
		}

		public bool Collide(Collider collider)
		{
			// if (!(ignoreBounds || collider.ignoreBounds) && !bounds.Overlaps(collider.bounds)) return false;
			if (!bounds.Overlaps(collider.bounds)) return false;

			var result = CheckCollider(collider);
			if (!result.HasValue) result = collider.CheckCollider(this);

			if (!result.HasValue) throw new System.NotImplementedException($"Collision {GetType().Name} vs {collider.GetType().Name} is not implemented");

			return result.Value;
		}
		protected abstract bool? CheckCollider(Collider collider);
		public abstract bool PointCheck(Vector2 point);
		public abstract bool RectCheck(Rect rect);
		public abstract bool LineCheck(Vector2 from, Vector2 to);
		public abstract RaycastHit Raycast(Vector2 position, Vector2 direction);

		public void Draw()
		{
			if (!ignoreBounds) GFX.DrawRectOutwards(bounds, Color.yellow);
			Draw(Color.green);
		}
		protected abstract void Draw(Color color);

		public abstract float width { get; set; }
		public abstract float height { get; set; }
		public abstract float top { get; set; }
		public abstract float bottom { get; set; }
		public abstract float left { get; set; }
		public abstract float right { get; set; }

		public void CenterOrigin()
		{
			position.x = -width / 2;
			position.y = -height / 2;
		}

		public virtual Vector2 size { get => new Vector2(width, height); }
		public Vector2 halfSize { get => size / 2; }

		public float centerX { get => left + width / 2; set => left = value - width / 2; }
		public float centerY { get => top + height / 2; set => top = value - height / 2; }

		public Vector2 topLeft { get => new Vector2(left, top); set { left = value.x; top = value.y; } }
		public Vector2 topCenter { get => new Vector2(centerX, top); set { centerX = value.x; top = value.y; } }
		public Vector2 topRight { get => new Vector2(right, top); set { right = value.x; top = value.y; } }
		public Vector2 centerLeft { get => new Vector2(left, centerY); set { left = value.x; centerY = value.y; } }
		public Vector2 center { get => new Vector2(centerX, centerY); set { centerX = value.x; centerY = value.y; } }
		public Vector2 centerRight { get => new Vector2(right, centerY); set { right = value.x; centerY = value.y; } }
		public Vector2 bottomLeft { get => new Vector2(left, bottom); set { left = value.x; bottom = value.y; } }
		public Vector2 bottomCenter { get => new Vector2(centerX, bottom); set { centerX = value.x; bottom = value.y; } }
		public Vector2 bottomRight { get => new Vector2(right, bottom); set { right = value.x; bottom = value.y; } }

		public Vector2 absPosition => node ? node.position + position : position;
		public float absX => node ? node.position.x + position.x : position.x;
		public float absY => node ? node.position.y + position.y : position.y;
		public float absTop => node ? node.position.y + top : top;
		public float absBottom => node ? node.position.y + bottom : bottom;
		public float absLeft => node ? node.position.x + left : left;
		public float absRight => node ? node.position.x + right : right;

		public Rect bounds => new Rect(absLeft, absTop, width, height);
	}
}
