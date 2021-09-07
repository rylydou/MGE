namespace MGE
{
	public class RaycastHit
	{
		public Vector2 origin;

		public float distance;
		public Vector2 direction;

		Vector2? _normal;
		public Vector2 normal
		{
			get
			{
				if (!_normal.HasValue)
					_normal = ((Vector2)_normal).normalized;
				return _normal.Value;
			}
			set => _normal = value;
		}

		Vector2? _position;
		public Vector2 position
		{
			get
			{
				if (!_position.HasValue)
					_position = origin + direction * distance;
				return _position.Value;
			}
			set => _position = value;
		}

		// public static bool WithinDistance(RaycastHit hit, float distance)
		// {
		// 	if (hit is null) return false;
		// 	return hit.distance < distance;
		// }

		// public static implicit operator bool(RaycastHit raycastHit)
		// {
		// 	if (raycastHit == null) return false;
		// 	if (raycastHit.normal == Vector2.zero) return false;
		// 	return true;
		// }
	}
}