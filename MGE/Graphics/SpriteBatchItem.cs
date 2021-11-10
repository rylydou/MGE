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
			new(destination.topRight, new(1, 0), color),		// Top right
			new(destination.bottomRight, new(1, 1), color),	// Bottom right
			new(destination.bottomLeft, new(0, 1), color),	// Bottom left
			new(destination.topLeft, new(0, 0), color),			// Top left
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
