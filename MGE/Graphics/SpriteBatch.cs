using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics;

public class SpriteBatch : IDisposable
{
	readonly int _capacity;

	Dictionary<(Texture, Shader, sbyte), List<SpriteBatchItem>> _itemGroups = new();

	ushort _vertexPosition;
	ushort _vertexItemPosition;
	float[] _vertices;
	// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A

	ushort _indexPosition;
	ushort[] _indices;

	int _vertexBufferObject;
	int _vertexArrayObject;
	int _elementBufferObject;
	Shader _spriteShader;

	public SpriteBatch(int capacity = 256)
	{
		_capacity = capacity;

		_vertices = new float[capacity * 4 * Vertex.SIZE];
		_indices = new ushort[capacity * 6];

		#region Vertex Buffer Object

		_vertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
		// GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);

		#endregion Vertex Buffer Object

		#region Vertex Array Object

		_vertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(_vertexArrayObject);

		// Position
		GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vertex.FULL_SIZE, 0);
		GL.EnableVertexAttribArray(0);

		// Texture Coordinate
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.FULL_SIZE, 2 * sizeof(float));
		GL.EnableVertexAttribArray(1);

		// Color
		GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vertex.FULL_SIZE, 4 * sizeof(float));
		GL.EnableVertexAttribArray(2);

		#endregion Vertex Array Object

		#region Element Buffer Object

		_elementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

		#endregion Element Buffer Object

		_spriteShader = new("Sprite.vert", "Sprite.frag");
	}

	public void Flush()
	{
		GL.BindVertexArray(_vertexBufferObject);

		foreach (var group in _itemGroups)
		{
			group.Key.Item2!.SetMatrix("projection", Matrix.CreateOrthographic(GameWindow.current.Size.X, GameWindow.current.Size.Y, -1, 1));

			// Loop over all the items and add their vertices and indexes
			foreach (var item in group.Value)
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
					SetIndex((ushort)(startingIndex + index));
				}
			}

			group.Value.Clear();

			GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(ushort), _indices, BufferUsageHint.StreamDraw);

			group.Key.Item1?.Use();
			group.Key.Item2?.Use();

			GL.DrawElements(PrimitiveType.Triangles, (int)_indexPosition, DrawElementsType.UnsignedShort, 0);

			_vertexPosition = 0;
			_vertexItemPosition = 0;
			_indexPosition = 0;
		}
	}

	public void DrawTexture(Texture texture, Vector2 position, Color? color = null)
	{
		if (!color.HasValue) color = Color.white;

		SetItem(new(texture, _spriteShader, 0, new(position / ((Vector2)texture.size / 2), texture.size), color.Value));
	}

	public void DrawTexture(Texture texture, Rect destination, Color? color = null)
	{
		if (!color.HasValue) color = Color.white;

		SetItem(new(texture, _spriteShader, 0, destination, color.Value));
	}

	public void DrawTextureRegion(Texture texture, Rect destination, RectInt source, Color? color = null)
	{
		if (!color.HasValue) color = Color.white;

		SetItem(new(texture, _spriteShader, 0, destination, source, color.Value));
	}

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
	void SetValue(float value)
	{
		_vertices[_vertexItemPosition] = value;

		_vertexItemPosition++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetIndex(ushort index)
	{
		_indices[_indexPosition] = index;
		_indexPosition++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void SetItem(SpriteBatchItem item)
	{
		var key = (item.texture, item.shader, item.priority);
		if (_itemGroups.TryGetValue(key, out var list))
		{
			list.Add(item);
			return;
		}
		_itemGroups.Add(key, new() { item });
	}

	public void Dispose()
	{
		GL.DeleteBuffer(_vertexBufferObject);
		_spriteShader.Dispose();
	}
}
