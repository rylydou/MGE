using System;
using System.Collections.Generic;
using System.Linq;
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

	class BatchItem
	{
		public LowLevelList<float> items = new();
		public LowLevelList<VertIndex> elements = new();

		public ushort shapeCount;

		public BatchItem()
		{
			items = new();
			elements = new();
		}

		public void AddVerts(float[] items, VertIndex[] elements)
		{
			var indexOffset = this.elements.Count;

			this.elements.Add(elements.Select(index => (VertIndex)(indexOffset + index)), elements.Length);
			this.items.Add(items);

			shapeCount++;
		}

		public void Clear()
		{
			items.Clear();
			elements.Clear();

			shapeCount = 0;
		}
	}

#if MGE_UINT32_INDICES
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedInt;
#else
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedShort;
#endif

	public readonly int capacity;

	public Matrix transform;

	// TODO
	Dictionary<BatchKey, BatchItem> _batches = new();

	// Dictionary<BatchKey, List<SpriteBatchItem>> _batches = new();

	VertexArray _verts;

	Buffer<float> _itemBuffer;
	// VertIndex _vertexCount;
	// VertIndex _itemPosition;
	// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
	// float[] _items;

	Buffer<VertIndex> _elementBuffer;
	// VertIndex _elementPosition;
	// VertIndex[] _elements;

	Shader _spriteShader;

	public SpriteBatch(int capacity = 256)
	{
		this.capacity = capacity;

		_itemBuffer = new();
		_itemBuffer.Init(BufferTarget.ArrayBuffer, capacity * 4 * Vertex.SIZE, BufferUsageHint.StreamDraw);

		_verts = new();
		_verts.Bind();
		_verts.BindAttribute(0, _itemBuffer, 2, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 0, false);                   // Position
		_verts.BindAttribute(1, _itemBuffer, 2, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 2 * sizeof(float), false);   // Texture Coord
		_verts.BindAttribute(2, _itemBuffer, 4, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 4 * sizeof(float), false);   // Color

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
			_itemBuffer.SubData(BufferTarget.ArrayBuffer, batch.Value.items.items, 0, (int)batch.Value.items.Count);
			_elementBuffer.SubData(BufferTarget.ElementArrayBuffer, batch.Value.elements.items, 0, (int)batch.Value.elements.Count);

			batch.Value.Clear();

			batch.Key.texture.Use();
			batch.Key.shader.SetMatrix("transform", transform);

			_verts.DrawElements(PrimitiveType.Triangles, (int)batch.Value.elements.Count, ELEMENTS_TYPE);
			// _verts.DrawElements(PrimitiveType.LineLoop, (int)_elementPosition, ELEMENTS_TYPE);
		}
	}

	public void DrawTexture(Texture texture, Vector2 position) => DrawTexture(texture, position, Color.white);
	public void DrawTexture(Texture texture, Vector2 position, Color color) => SetItem(new(texture, _spriteShader, 0, new(position - ((Vector2)texture.size / 2), texture.size), color));

	public void DrawTexture(Texture texture, Rect destination) => SetItem(new(texture, _spriteShader, 0, destination, Color.white));
	public void DrawTexture(Texture texture, Rect destination, Color color) => SetItem(new(texture, _spriteShader, 0, destination, color));

	public void DrawTextureRegion(Texture texture, Rect destination, RectInt source) => SetItem(new(texture, _spriteShader, 0, destination, source, Color.white));
	public void DrawTextureRegion(Texture texture, Rect destination, RectInt source, Color color) => SetItem(new(texture, _spriteShader, 0, destination, source, color));

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
	void SetItem(SpriteBatchItem item)
	{
		var key = new BatchKey(item.texture, item.shader, item.priority);
		if (!_batches.TryGetValue(key, out var list))
		{
			list = new();
			_batches.Add(key, list);
		}
		var items = new float[item.vertices.Length * Vertex.SIZE];
		var i = 0;
		item.vertices.ForEach(vert =>
		{
			items[i++] = vert.position.x;
			items[i++] = vert.position.y;

			items[i++] = vert.textureCoordinate.x;
			items[i++] = vert.textureCoordinate.y;

			items[i++] = vert.color.r;
			items[i++] = vert.color.g;
			items[i++] = vert.color.b;
			items[i++] = vert.color.a;
		});
		list.AddVerts(items, item.indices);
		// list.Add(item);
	}

	public void Dispose()
	{
		_verts.Dispose();

		_itemBuffer.Dispose();
		_elementBuffer.Dispose();

		_spriteShader.Dispose();
	}
}
