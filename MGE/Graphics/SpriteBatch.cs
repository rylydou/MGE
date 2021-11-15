using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;
#if MGE_UINT32_INDICES
using VertIndex = System.UInt32;
#else
using VertIndex = System.UInt16;
#endif
// using VertIndex = System.UInt32;

namespace MGE.Graphics;

public class SpriteBatch : IDisposable
{
#if MGE_UINT32_INDICES
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedInt;
#else
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedShort;
#endif

	public readonly int capacity;

	// SortedDictionary<sbyte, Dictionary<Shader, Dictionary<Texture, SpriteBatchItem>>> _batches = new();

	Dictionary<(Texture, Shader, sbyte), List<SpriteBatchItem>> _batches = new();

	VertexArray _verts;
	Buffer<float> _vertBuffer;
	Buffer<VertIndex> _indexBuffer;

	VertIndex _vertexPosition;
	VertIndex _vertexItemPosition;
	// // Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
	float[] _vertices;

	VertIndex _indexPosition;
	VertIndex[] _indices;

	// int _vertexBufferObject;
	// int _vertexArrayObject;
	// int _elementBufferObject;

	// IntPtr _vertMap;
	// IntPtr _indexMap;

	Shader _spriteShader;

	public SpriteBatch(int capacity = 256)
	{
		this.capacity = capacity;

		_vertBuffer = new();
		_vertBuffer.Init(BufferTarget.ArrayBuffer, capacity * 4 * Vertex.SIZE, BufferUsageHint.StreamDraw);
		_indexBuffer = new();
		_indexBuffer.Init(BufferTarget.ElementArrayBuffer, capacity * 6, BufferUsageHint.StreamDraw);

		_verts = new();
		_verts.Bind();
		_verts.BindAttribute(0, _vertBuffer, 2, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 0, false);
		_verts.BindAttribute(1, _vertBuffer, 2, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 2 * sizeof(float), false);
		_verts.BindAttribute(2, _vertBuffer, 4, VertexAttribPointerType.Float, Vertex.FULL_SIZE, 4 * sizeof(float), false);

		_vertices = new float[capacity * 4 * Vertex.SIZE];
		_indices = new VertIndex[capacity * 6];

		// #region Vertex Buffer Object

		// _vertexBufferObject = GL.GenBuffer();
		// GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
		// GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);
		// _vertMap = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadWrite);

		// #endregion Vertex Buffer Object

		// #region Element Buffer Object

		// _elementBufferObject = GL.GenBuffer();
		// GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
		// GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(VertIndex), _indices, BufferUsageHint.StreamDraw);
		// _indexMap = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadWrite);

		// #endregion Element Buffer Object

		// #region Vertex Array Object

		// _vertexArrayObject = GL.GenVertexArray();
		// GL.BindVertexArray(_vertexArrayObject);

		// // Position
		// GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vertex.FULL_SIZE, 0);
		// GL.EnableVertexAttribArray(0);

		// // Texture Coordinate
		// GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.FULL_SIZE, 2 * sizeof(float));
		// GL.EnableVertexAttribArray(1);

		// // Color
		// GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vertex.FULL_SIZE, 4 * sizeof(float));
		// GL.EnableVertexAttribArray(2);

		// #endregion Vertex Array Object

		_spriteShader = new("Sprite.vert", "Sprite.frag");
	}

	public void Flush()
	{
		// GL.BindVertexArray(_vertexBufferObject);


		foreach (var batch in _batches)
		{
			if (batch.Value.Count == 0) continue;

			ResetPosition();

			// Debug.LogVariable(batch.Value.Count);

			batch.Key.Item2!.SetMatrix("transform", Matrix.CreateOrthographic(GameWindow.current.Size.X, GameWindow.current.Size.Y, -1, 1));

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

			_vertBuffer.SubData(BufferTarget.ArrayBuffer, _vertices, 0, (int)_vertexPosition);
			_indexBuffer.SubData(BufferTarget.ElementArrayBuffer, _indices, 0, (int)_indexPosition);

			// GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertices.Length * sizeof(float), _vertices);
			// GFX.CheckError();
			// GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, _indices.Length * sizeof(VertIndex), _indices);
			// GFX.CheckError();
			// GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);
			// GFX.CheckError();
			// GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(VertIndex), _indices, BufferUsageHint.StreamDraw);

			// GL.BindTexture(TextureTarget.Texture2D, 0);
			// GL.UseProgram(0);

			batch.Key.Item1!.Use();
			batch.Key.Item2!.Use();

			_verts.DrawElements(PrimitiveType.Triangles, (int)_indexPosition, ELEMENTS_TYPE);

			// GL.DrawElements(PrimitiveType.Triangles, (int)_indexPosition, ELEMENTS_TYPE, 0);
			// GFX.CheckError();
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetValue(float value) => _vertices[_vertexItemPosition++] = value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetIndex(VertIndex index) => _indices[_indexPosition++] = index;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void ResetPosition()
	{
		_vertexPosition = 0;
		_vertexItemPosition = 0;
		_indexPosition = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetItem(SpriteBatchItem item)
	{
		var key = (item.texture, item.shader, item.priority);
		if (_batches.TryGetValue(key, out var list))
		{
			list.Add(item);
			return;
		}
		_batches.Add(key, new() { item });
	}


	public void Dispose()
	{
		// GL.DeleteBuffer(_vertexBufferObject);
		_verts.Dispose();
		_vertBuffer.Dispose();
		_spriteShader.Dispose();
	}
}
