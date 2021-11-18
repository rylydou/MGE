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

public class SpriteBatch : IDisposable
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
		public LowLevelList<float> items = new();
		public LowLevelList<VertIndex> elements = new();

		public ushort shapeCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVerts(float[] items, VertIndex[] elements)
		{
			var indexOffset = this.items.Count;

			this.elements.Add(elements.Select(index => (VertIndex)(indexOffset + index)), elements.Length);
			this.items.Add(items);

			shapeCount++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			items.Clear();
			elements.Clear();

			shapeCount = 0;
		}
	}

	public Matrix transform;

	// TODO
	Dictionary<BatchKey, BatchItem> _batches = new();

	// Dictionary<BatchKey, List<SpriteBatchItem>> _batches = new();

	VertexArray _vertexArray;

	Buffer<float> _vertexBuffer;
	// VertIndex _vertexCount;
	// VertIndex _itemPosition;
	// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
	// float[] _items;

	Buffer<VertIndex> _elementBuffer;
	// VertIndex _elementPosition;
	// VertIndex[] _elements;

	Shader _spriteShader;

	// public Shader shader;
	// public Texture texture;
	// public sbyte priority;

	public SpriteBatch(int capacity = 256)
	{
		_vertexBuffer = new();
		_vertexBuffer.Init(BufferTarget.ArrayBuffer, capacity * 4 * Vertex.SIZE_IN_ELEMENTS, BufferUsageHint.StreamDraw);

		_vertexArray = new();
		_vertexArray.Bind();
		_vertexArray.BindAttribute(0, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 0, false);                   // Position
		_vertexArray.BindAttribute(1, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 2 * sizeof(float), false);   // Texture Coord
		_vertexArray.BindAttribute(2, _vertexBuffer, 4, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 4 * sizeof(float), false);   // Color

		_elementBuffer = new();
		_elementBuffer.Init(BufferTarget.ElementArrayBuffer, capacity * 6, BufferUsageHint.StreamDraw);

		// _items = new float[capacity * 4 * Vertex.SIZE];
		// _elements = new VertIndex[capacity * 6];

		_spriteShader = new("Sprite.vert", "Sprite.frag");
	}

	// public void SetRenderTexture(RenderTexture texture)
	// {
	// 	texture.Use();
	// }

	public void Flush()
	{
		foreach (var batch in _batches)
		{
			if (batch.Value.shapeCount == 0) continue;

			// ResetPosition();

			// // Loop over all the items and add their vertices and indexes
			// foreach (var item in batch.Value)
			// {
			// 	// Add all the indices
			// 	var indexOffset = _vertexCount;
			// 	foreach (var index in item.indices)
			// 	{
			// 		SetIndex((VertIndex)(indexOffset + index));
			// 	}

			// 	// Add all the vertices
			// 	foreach (var vertex in item.vertices)
			// 	{
			// 		SetVertex(vertex);
			// 	}
			// }

			batch.Key.texture.Use();
			batch.Key.shader.SetMatrix("transform", transform);

			_vertexBuffer.SubData(BufferTarget.ArrayBuffer, batch.Value.items.array, 0, batch.Value.items.Count);
			_elementBuffer.SubData(BufferTarget.ElementArrayBuffer, batch.Value.elements.array, 0, batch.Value.elements.Count);

			_vertexArray.DrawElements(PrimitiveType.Triangles, batch.Value.elements.Count, ELEMENTS_TYPE);
			// _verts.DrawElements(PrimitiveType.LineLoop, (int)_elementPosition, ELEMENTS_TYPE);

			batch.Value.Clear();
		}
	}

	public void DrawTexture(Texture texture, Vector2 position) => DrawTexture(texture, position, Color.white);
	public void DrawTexture(Texture texture, Vector2 position, Color color) => SetItem(texture, _spriteShader, 0, new(position - ((Vector2)texture.size / 2), texture.size), color);

	public void DrawTexture(Texture texture, Rect destination) => SetItem(texture, _spriteShader, 0, destination, Color.white);
	public void DrawTexture(Texture texture, Rect destination, Color color) => SetItem(texture, _spriteShader, 0, destination, color);

	public void DrawTextureRegion(Texture texture, Rect destination, RectInt source) => SetItem(texture, _spriteShader, 0, destination, source, Color.white);
	public void DrawTextureRegion(Texture texture, Rect destination, RectInt source, Color color) => SetItem(texture, _spriteShader, 0, destination, source, color);

	// [MethodImpl(MethodImplOptions.AggressiveInlining)]
	// void SetVertex(Vertex vertex)
	// {
	// 	SetValue(vertex.position.x);
	// 	SetValue(vertex.position.y);

	// 	SetValue(vertex.textureCoordinate.x);
	// 	SetValue(vertex.textureCoordinate.y);

	// 	SetValue(vertex.color.r);
	// 	SetValue(vertex.color.g);
	// 	SetValue(vertex.color.b);
	// 	SetValue(vertex.color.a);

	// 	_vertexCount++;
	// }

	// [MethodImpl(MethodImplOptions.AggressiveInlining)] void SetValue(float value) => _items[_itemPosition++] = value;

	// [MethodImpl(MethodImplOptions.AggressiveInlining)] void SetIndex(VertIndex index) => _elements[_elementPosition++] = index;

	// [MethodImpl(MethodImplOptions.AggressiveInlining)]
	// void ResetPosition()
	// {
	// 	_vertexCount = 0;
	// 	_itemPosition = 0;
	// 	_elementPosition = 0;
	// }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetItem(Texture texture, Shader shader, sbyte priority, Rect destination, RectInt source, Color color)
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
			new ushort[] {
				0, 1, 3,	// Bottom right
				1, 2, 3,	// Top left
			}
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetItem(Texture texture, Shader shader, sbyte priority, Rect destination, Color color)
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
			new ushort[] {
				0, 1, 3,	// Bottom right
				1, 2, 3,	// Top left
			}
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetItem(Texture texture, Shader shader, sbyte priority, Vertex[] vertices, VertIndex[] indices)
	{
		var key = new BatchKey(texture, shader, priority);

		if (!_batches.TryGetValue(key, out var list))
		{
			list = new();
			_batches.Add(key, list);
		}

		// There has to be a faster way of doing this, maybe Marshal or somthing
		var expandedVertices = new ArrayBuilder<float>(vertices.Length * Vertex.SIZE_IN_ELEMENTS);
		foreach (var vert in vertices)
		{
			expandedVertices.Add(vert.position.x);
			expandedVertices.Add(vert.position.y);

			expandedVertices.Add(vert.textureCoordinate.x);
			expandedVertices.Add(vert.textureCoordinate.y);

			expandedVertices.Add(vert.color.r);
			expandedVertices.Add(vert.color.g);
			expandedVertices.Add(vert.color.b);
			expandedVertices.Add(vert.color.a);
		}

		list.AddVerts(expandedVertices.array, indices);
		// list.Add(item);
	}

	public void Dispose()
	{
		_vertexArray.Dispose();

		_vertexBuffer.Dispose();
		_elementBuffer.Dispose();

		_spriteShader.Dispose();
	}
}
