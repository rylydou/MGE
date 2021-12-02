using System;

namespace MGE.Graphics;

public class BatchKey : IEquatable<BatchKey>, IComparable<BatchKey>
{
	public Texture texture;
	public Shader shader;

	public float priority;

	public byte timesSkiped;

	public BatchKey(Texture texture, Shader shader)
	{
		this.texture = texture;
		this.shader = shader;
	}

	public int CompareTo(BatchKey? other) => this.priority.CompareTo(other);

	public override bool Equals(object? obj) => obj is BatchKey key && Equals(key);
	public bool Equals(BatchKey? other) => other is not null && other.texture == texture && other.shader == shader && other.priority == priority;

	public override int GetHashCode() => HashCode.Combine(texture, shader, priority);
}
