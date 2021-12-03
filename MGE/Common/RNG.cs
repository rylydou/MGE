using System;

namespace MGE
{
	public class RNG
	{
		public readonly static RNG shared = new();

		int _state;

		public RNG() : this(Math.Abs(Guid.NewGuid().GetHashCode())) { }

		public RNG(int seed)
		{
			if (seed < 1) throw new ArgumentOutOfRangeException(nameof(seed), "Seed must be greater than zero");

			_state = seed;
		}

		public int RandomInt() => ((214013 * _state + 2531011) >> 16) & 0x7FFF;
		public int RandomInt(int max) => (int)(max * RandomFloat() + 0.5f);
		public int RandomInt(int min, int max) => (int)((max - min) * RandomFloat() + 0.5f) + min;

		public float RandomFloat() => RandomInt() / (float)short.MaxValue;
		public float RandomFloat(float max) => max * RandomFloat();
		public float RandomFloat(float min, float max) => (max - min) * RandomFloat() + min;

		public bool RandomBool() => RandomInt(1) == 0;

		public int RandomSign() => RandomBool() ? 1 : -1;

		public float RandomAngle() => RandomFloat(Math.pi2);

		public Vector2 RandomUnitVector()
		{
			var angle = RandomAngle();
			return new((float)Math.Cos(angle), (float)Math.Sin(angle));
		}

		public Color RandomColor() => new(RandomFloat(), RandomFloat(), RandomFloat());
	}
}
