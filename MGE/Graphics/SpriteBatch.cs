using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;
#if MGE_UINT32_INDICES
using VertIndex = System.UInt32;
#else
using VertIndex = System.UInt16;
#endif

namespace MGE.Graphics;

public class SpriteBatch : IDisposable
{
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

#if MGE_UINT32_INDICES
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedInt;
#else
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedShort;
#endif

	public readonly int capacity;

	public Matrix transform;

	// TODO
	// Dictionary<BatchKey, float[]> _batches = new();

	Dictionary<BatchKey, List<SpriteBatchItem>> _batches = new();

	VertexArray _verts;

	Buffer<float> _vertBuffer;
	VertIndex _vertexPosition;
	VertIndex _arrayPosition;
	// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
	float[] _items;

	Buffer<VertIndex> _indexBuffer;
	VertIndex _elementPosition;
	VertIndex[] _elements;

	Shader _spriteShader;

	public SpriteBatch(int capacity = 256)
	{
		this.capacity = capacity;

		_vertBuffer = new();
		_vertBuffer.Init(BufferTarget.ArrayBuffer, capacity * 4 * Vertex.SIZE, BufferUsageHint.StreamDraw);

		_verts = new();
		_verts.Bind();
		_verts.BindAttribute(0, _vertBuffer, 2, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 0, false);                   // Position
		_verts.BindAttribute(1, _vertBuffer, 2, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 2 * sizeof(float), false);   // Texture Coord
		_verts.BindAttribute(2, _vertBuffer, 4, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 4 * sizeof(float), false);   // Color

		_indexBuffer = new();
		_indexBuffer.Init(BufferTarget.ElementArrayBuffer, capacity * 6, BufferUsageHint.StreamDraw);

		_items = new float[capacity * 4 * Vertex.SIZE];
		_elements = new VertIndex[capacity * 6];

		_spriteShader = new("Sprite.vert", "Sprite.frag");
	}

	public void Flush()
	{
		// Debug.LogVariable(_batches.Count);

		foreach (var batch in _batches)
		{
			if (batch.Value.Count == 0) continue;

			// Debug.LogVariable(batch.Value.Count);

			ResetPosition();

			// Loop over all the items and add their vertices and indexes
			foreach (var item in batch.Value)
			{
				// Add all the vertices
				foreach (var vertex in item.vertices)
				{
					SetVertex(vertex);
				}

				// Add all the indices
				var startingIndex = _vertexPosition;
				foreach (var index in item.indices)
				{
					SetIndex((VertIndex)(startingIndex + index));
				}
			}

			batch.Value.Clear();

			batch.Key.texture.Use();
			batch.Key.shader.SetMatrix("transform", transform);

			_vertBuffer.SubData(BufferTarget.ArrayBuffer, _items, 0, (int)_arrayPosition);
			_indexBuffer.SubData(BufferTarget.ElementArrayBuffer, _elements, 0, (int)_elementPosition);

			_verts.DrawElements(PrimitiveType.Triangles, (int)_elementPosition, ELEMENTS_TYPE);
			// _verts.DrawElements(PrimitiveType.LineLoop, (int)_elementPosition, ELEMENTS_TYPE);
		}
	}

	public void DrawTexture(Texture texture, Vector2 position) => DrawTexture(texture, position, Color.white);
	public void DrawTexture(Texture texture, Vector2 position, Color color) => SetItem(new(texture, _spriteShader, 0, new(position - ((Vector2)texture.size / 2), texture.size), color));

	public void DrawTexture(Texture texture, Rect destination, Color? color = null) => SetItem(new(texture, _spriteShader, 0, destination, color ?? Color.white));

	public void DrawTextureRegion(Texture texture, Rect destination, RectInt source, Color? color = null) => SetItem(new(texture, _spriteShader, 0, destination, source, color ?? Color.white));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetVertex(Vertex vertex)
	{
		SetValue(vertex.position.x);
		SetValue(vertex.position.y);

		SetValue(vertex.textureCoordinate.x);
		SetValue(vertex.textureCoordinate.y);

		SetValue(vertex.color.r);
		SetValue(vertex.color.g);
		SetValue(vertex.color.b);
		SetValue(vertex.color.a);

		_vertexPosition++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)] void SetValue(float value) => _items[_arrayPosition++] = value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)] void SetIndex(VertIndex index) => _elements[_elementPosition++] = index;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void ResetPosition()
	{
		_vertexPosition = 0;
		_arrayPosition = 0;
		_elementPosition = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetItem(SpriteBatchItem item)
	{
		var key = new BatchKey(item.texture, item.shader, item.priority);
		if (!_batches.TryGetValue(key, out var list))
		{
			list = new();
			_batches.Add(key, list);
		}
		list.Add(item);
	}

	public void Dispose()
	{
		_verts.Dispose();

		_vertBuffer.Dispose();
		_indexBuffer.Dispose();

		_spriteShader.Dispose();
	}
}
