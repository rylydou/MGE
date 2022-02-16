using System;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics;

public class BatchKey : IEquatable<BatchKey>, IComparable<BatchKey>
{
	public Texture texture;
	public Shader shader;
	public RectInt? scissor;
	public bool filtering;

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Apply()
	{
		texture.Use();
		shader.Use();

		if (scissor.HasValue)
		{
			var scissorRect = scissor.Value;
			GL.Scissor(scissorRect.x, scissorRect.y, scissorRect.width, scissorRect.height);
			GL.Enable(EnableCap.ScissorTest);
		}
		else
		{
			GL.Disable(EnableCap.ScissorTest);
		}
	}
}
