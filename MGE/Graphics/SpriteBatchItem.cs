using System;

namespace MGE.Graphics;

internal class SpriteBatchItem : IComparable<SpriteBatchItem>
{
	public Texture texture;
	public Shader shader;
	public sbyte priority;

	public Vertex[] vertices;
	public ushort[] indices;

	public SpriteBatchItem(Texture texture, Shader shader, sbyte priority, Vertex[] vertices, ushort[] indices)
	{
		this.texture = texture;
		this.shader = shader;
		this.priority = priority;

		this.vertices = vertices;
		this.indices = indices;
	}

	public SpriteBatchItem(Texture texture, Shader shader, sbyte priority, Rect destination, RectInt source, Color color)
	{
		this.texture = texture;
		this.shader = shader;
		this.priority = priority;

		vertices = new Vertex[]
		{
			new(destination.topRight, texture.GetTextureCoord(source.topRight), color),				// Top right
			new(destination.bottomRight,texture.GetTextureCoord(source.bottomRight), color),	// Bottom right
			new(destination.bottomLeft, texture.GetTextureCoord(source.bottomLeft), color),		// Bottom left
			new(destination.topLeft, texture.GetTextureCoord(source.topLeft), color),					// Top left
		};

		indices = new ushort[]
		{
			0, 1, 3,	// Bottom right
			1, 2, 3,	// Top left
		};
	}

	public SpriteBatchItem(Texture texture, Shader shader, sbyte priority, Rect destination, Color color)
	{
		this.texture = texture;
		this.shader = shader;
		this.priority = priority;

		vertices = new Vertex[]
		{
			new(destination.topRight, Vector2.upRight, color),	// Top right
			new(destination.bottomRight, Vector2.right, color),	// Bottom right
			new(destination.bottomLeft, Vector2.zero, color),		// Bottom left
			new(destination.topLeft, Vector2.up, color),				// Top left
		};

		indices = new ushort[]
		{
			0, 1, 3,	// Bottom right
			1, 2, 3,	// Top left
		};
	}

	public int CompareTo(SpriteBatchItem? other) => priority.CompareTo(other!.priority);

	public override bool Equals(object? obj) => obj is SpriteBatchItem item && texture == item.texture && shader == item.shader && priority == item.priority;

	public override int GetHashCode() => HashCode.Combine(texture, shader, priority);
}
