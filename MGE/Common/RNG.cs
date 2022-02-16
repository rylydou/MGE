using System;

namespace MGE;

public class RNG
{
	public readonly static RNG shared = new();

	Random rng;

	public RNG() : this(Math.Abs(Guid.NewGuid().GetHashCode())) { }
	public RNG(int seed) => rng = new(seed);

	public int RandomInt() => rng.Next();
	public int RandomInt(int max) => rng.Next(max + 1);
	public int RandomInt(int min, int max) => rng.Next(min, max + 1);

	public float RandomFloat() => rng.NextSingle();
	public float RandomFloat(float max) => max * RandomFloat();
	public float RandomFloat(float min, float max) => (max - min) * RandomFloat() + min;

	public bool RandomBool() => RandomInt(1) == 0;
	public bool RandomBool(float chance) => RandomFloat() <= chance;
	public bool RandomBool(int chance) => RandomInt(100) <= chance;

	public float RandomSign() => RandomBool() ? 1f : -1f;
	public int RandomSignInt() => RandomBool() ? 1 : -1;

	public float RandomAngle() => RandomFloat(Math.tau);

	public Vector2 RandomUnitVector()
	{
		var angle = RandomAngle();
		return new((float)Math.Cos(angle), (float)Math.Sin(angle));
	}

	public Color RandomColor() => new(RandomFloat(), RandomFloat(), RandomFloat(), 1.0f);
}
