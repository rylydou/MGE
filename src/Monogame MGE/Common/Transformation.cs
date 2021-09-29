using Microsoft.Xna.Framework;

namespace MGE.Common
{
	public class Transformation
	{
		public Vector2 position = Vector2.zero;

		public float rotation = 0;

		public Vector2 scale = Vector2.one;

		private static readonly Transformation _identity = new Transformation();
		public static Transformation identity { get { return _identity; } }

		public static Transformation Compose(Transformation left, Transformation right)
		{
			var result = new Transformation();
			var transformedPosition = left.TransformVector(right.position);
			result.position = transformedPosition;
			result.rotation = left.rotation + right.rotation;
			result.scale = left.scale * right.scale;
			return result;
		}

		public static void Lerp(ref Transformation key1, ref Transformation key2, float amount, ref Transformation result)
		{
			result.position = Vector2.Lerp(key1.position, key2.position, amount);
			result.scale = Vector2.Lerp(key1.scale, key2.scale, amount);
			result.rotation = Math.Lerp(key1.rotation, key2.rotation, amount);
		}

		public Vector2 TransformVector(Vector2 point)
		{
			var result = (Vector2)Microsoft.Xna.Framework.Vector2.Transform(point, Matrix.CreateRotationZ(rotation));
			result *= scale;
			result += position;
			return result;
		}
	}
}