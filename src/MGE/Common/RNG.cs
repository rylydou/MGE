using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MGE;

public class RNG
{
	const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

	public readonly static RNG shared = new();

	readonly Random _rng;

	[MethodImpl(INLINE)] public RNG() : this(Mathf.Abs(Guid.NewGuid().GetHashCode())) { }
	[MethodImpl(INLINE)] public RNG(int seed) => _rng = new(seed);

	[MethodImpl(INLINE)] public int RandomInt() => _rng.Next();
	[MethodImpl(INLINE)] public int RandomInt(int max) => _rng.Next(max + 1);
	[MethodImpl(INLINE)] public int RandomInt(int min, int max) => _rng.Next(min, max + 1);

	[MethodImpl(INLINE)] public float RandomFloat() => _rng.NextSingle();
	[MethodImpl(INLINE)] public float RandomFloat(float max) => RandomFloat() * max;
	[MethodImpl(INLINE)] public float RandomFloat(float min, float max) => (max - min) * RandomFloat() + min;

	[MethodImpl(INLINE)] public bool RandomBool() => RandomBool(0.5f);
	[MethodImpl(INLINE)] public bool RandomBool(float chance) => RandomFloat() <= chance;
	[MethodImpl(INLINE)] public bool RandomBool(int chance) => RandomInt(100) <= chance;

	[MethodImpl(INLINE)] public float RandomSign() => RandomBool() ? 1f : -1f;
	[MethodImpl(INLINE)] public int RandomSignInt() => RandomBool() ? 1 : -1;

	[MethodImpl(INLINE)] public float RandomAngle() => RandomFloat(Mathf.TAU);

	[MethodImpl(INLINE)]
	public Vector2 RandomUnitVector()
	{
		var angle = RandomAngle();
		return new((float)Mathf.Cos(angle), (float)Mathf.Sin(angle));
	}

	[MethodImpl(INLINE)] public Color RandomColor() => new(RandomFloat(), RandomFloat(), RandomFloat(), 1.0f);

	[MethodImpl(INLINE)]
	public T RandomItem<T>(T[] list)
	{
		return list[_rng.Next(list.Length)];
	}

	[MethodImpl(INLINE)]
	public T RandomItem<T>(IList<T> list)
	{
		return list[_rng.Next(list.Count)];
	}
}
