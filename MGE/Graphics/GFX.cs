using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

#if MGE_MORE_VERTICES
using VertIndex = System.UInt32;
#else
using VertIndex = System.UInt16;
#endif

namespace MGE.Graphics;

public static class GFX
{
#if MGE_MORE_VERTICES
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedInt;
#else
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedShort;
#endif

	class BatchKey : IEquatable<BatchKey>, IComparable<BatchKey>
	{
		public Texture texture;
		public Shader shader;
		public sbyte priority;

		public BatchKey(Texture texture, Shader shader, sbyte priority)
		{
			this.texture = texture;
			this.shader = shader;
			this.priority = priority;
		}

		public int CompareTo(BatchKey? other) => this.priority.CompareTo(other);

		public override bool Equals(object? obj) => obj is BatchKey key && Equals(key);
		public bool Equals(BatchKey? other) => other is not null && other.texture == texture && other.shader == shader && other.priority == priority;

		public override int GetHashCode() => HashCode.Combine(texture, shader, priority);
	}

	class BatchItem
	{
		public LowLevelList<float> vertexData = new();
		public LowLevelList<VertIndex> elements = new();

		public VertIndex vertexCount;

		public VertIndex shapeCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVerts(Vertex[] vertices, VertIndex[] elements)
		{
			this.elements.Add(elements.Select(index => (VertIndex)(vertexCount + index)), elements.Length);

			vertexData.EnsureCapacity(vertexData.Count + vertices.Length);
			foreach (var vertex in vertices)
			{
				vertexData.AddUnsafe(vertex.position.x);
				vertexData.AddUnsafe(vertex.position.y);

				vertexData.AddUnsafe(vertex.textureCoordinate.x);
				vertexData.AddUnsafe(vertex.textureCoordinate.y);

				vertexData.AddUnsafe(vertex.color.r);
				vertexData.AddUnsafe(vertex.color.g);
				vertexData.AddUnsafe(vertex.color.b);
				vertexData.AddUnsafe(vertex.color.a);
			}

			vertexCount += (VertIndex)vertices.Length;
			shapeCount++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			vertexData.Clear();
			elements.Clear();

			shapeCount = 0;
			vertexCount = 0;
		}
	}

	public static Matrix transform;

	static Dictionary<BatchKey, BatchItem> _batches = new();

	// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
	static VertexArray _vertexDataArray;

	static Buffer<float> _vertexBuffer;

	static Buffer<VertIndex> _elementBuffer;

	static Shader _spriteShader;

	// public Shader shader;
	// public Texture texture;
	// public sbyte priority;

	static GFX()
	{
		const uint vertexCapacity = 1024;
		const uint elementCapacity = 1024;

		_vertexBuffer = new();
		_vertexBuffer.Init(BufferTarget.ArrayBuffer, (int)Math.NextPowerOf2((uint)(vertexCapacity * Vertex.SIZE_IN_ELEMENTS)), BufferUsageHint.StreamDraw);

		_vertexDataArray = new();
		_vertexDataArray.Bind();
		_vertexDataArray.BindAttribute(0, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 0 * sizeof(float), false); // Position
		_vertexDataArray.BindAttribute(1, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 2 * sizeof(float), false); // Texture Coord
		_vertexDataArray.BindAttribute(2, _vertexBuffer, 4, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 4 * sizeof(float), false); // Color

		_elementBuffer = new();
		_elementBuffer.Init(BufferTarget.ElementArrayBuffer, (int)Math.NextPowerOf2(elementCapacity), BufferUsageHint.StreamDraw);

		_spriteShader = new("Sprite.vert", "Sprite.frag");
	}

	public static void Flush()
	{
		foreach (var batch in _batches)
		{
			if (batch.Value.shapeCount == 0) continue;

			// Debug.Log($"\nVertices ({batch.Value.vertexCount} / {batch.Value.vertices.Count}):");
			// Debug.Log(string.Join(' ', batch.Value.vertices.array.Select(x => x.ToString()).ToArray(), 0, batch.Value.vertices.Count));
			// Debug.Log($"Elements ({batch.Value.elements.Count}):");
			// Debug.Log(string.Join(' ', batch.Value.elements.array.Select(x => x.ToString()).ToArray(), 0, batch.Value.elements.Count));

			batch.Key.texture.Use();
			batch.Key.shader.SetMatrix("transform", transform);

			_vertexBuffer.SubData(BufferTarget.ArrayBuffer, batch.Value.vertexData.array, 0, batch.Value.vertexData.Count);
			_elementBuffer.SubData(BufferTarget.ElementArrayBuffer, batch.Value.elements.array, 0, batch.Value.elements.Count);

			_vertexDataArray.DrawElements(PrimitiveType.Triangles, batch.Value.elements.Count, ELEMENTS_TYPE);

			batch.Value.Clear();
		}
	}

	public static void DrawTexture(Texture texture, Vector2 position) => DrawTexture(texture, position, Color.white);
	public static void DrawTexture(Texture texture, Vector2 position, Color color) => SetItem(texture, _spriteShader, 0, new(position - ((Vector2)texture.size / 2), texture.size), color);

	public static void DrawTexture(Texture texture, Rect destination) => SetItem(texture, _spriteShader, 0, destination, Color.white);
	public static void DrawTexture(Texture texture, Rect destination, Color color) => SetItem(texture, _spriteShader, 0, destination, color);

	public static void DrawTextureRegion(Texture texture, Rect destination, RectInt source) => SetItem(texture, _spriteShader, 0, destination, source, Color.white);
	public static void DrawTextureRegion(Texture texture, Rect destination, RectInt source, Color color) => SetItem(texture, _spriteShader, 0, destination, source, color);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetItem(Texture texture, Shader shader, sbyte priority, Rect destination, RectInt source, Color color)
	{
		SetItem(
			texture,
			shader,
			priority,
			new Vertex[] {
				new(destination.topRight,    texture.GetTextureCoord(source.bottomRight), color),	// Top right
				new(destination.bottomRight, texture.GetTextureCoord(source.topRight),    color),	// Bottom right
				new(destination.bottomLeft,  texture.GetTextureCoord(source.topLeft),     color),	// Bottom left
				new(destination.topLeft,     texture.GetTextureCoord(source.bottomLeft),  color), // Top left
			},
			new VertIndex[] {
				0, 1, 3,	// Bottom right
				1, 2, 3,	// Top left
			}
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetItem(Texture texture, Shader shader, sbyte priority, Rect destination, Color color)
	{
		SetItem(
			texture,
			shader,
			priority,
			new Vertex[] {
				new(destination.topRight,    new(1, 1), color),	// Top right
				new(destination.bottomRight, new(1, 0), color),	// Bottom right
				new(destination.bottomLeft,  new(0, 0), color),	// Bottom left
				new(destination.topLeft,     new(0, 1), color), // Top left
			},
			new VertIndex[] {
				0, 1, 3,	// Bottom right
				1, 2, 3,	// Top left
			}
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetItem(Texture texture, Shader shader, sbyte priority, Vertex[] vertices, VertIndex[] indices)
	{
		var key = new BatchKey(texture, shader, priority);

		if (!_batches.TryGetValue(key, out var list))
		{
			list = new();
			_batches.Add(key, list);
		}

		list.AddVerts(vertices, indices);
	}

	internal static void CheckError()
	{
		var error = GL.GetError();

		if (error != ErrorCode.NoError)
		{
			throw new Exception($"Open GL Error: {((int)error)} - {error.ToString()}");
		}
	}

	// public static void Dispose()
	// {
	// 	_vertexDataArray.Dispose();

	// 	_vertexBuffer.Dispose();
	// 	_elementBuffer.Dispose();

	// 	_spriteShader.Dispose();
	// }
}
