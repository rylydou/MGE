using System;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics
{
	public class SpriteBatch : IDisposable
	{
		const int VERTEX_SIZE = 8;

		readonly int _capacity;

		uint _vertexPosition;
		uint _vertexOffset;
		// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
		float[] _vertices;

		uint _indexPosition;
		uint[] _indices;

		int _vertexBufferObject;
		int _vertexArrayObject;
		int _elementBufferObject;
		Shader _shader;
		Texture _texture;

		public SpriteBatch(int capacity = 64)
		{
			_capacity = capacity;

			_vertices = new float[capacity * VERTEX_SIZE];
			_indices = new uint[capacity];

			#region Vertex Buffer Object

			_vertexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
			// GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);

			#endregion Vertex Buffer Object

			#region Vertex Array Object

			_vertexArrayObject = GL.GenVertexArray();
			GL.BindVertexArray(_vertexArrayObject);

			// Position
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			// Texture Coordinate
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 2 * sizeof(float));
			GL.EnableVertexAttribArray(1);

			// Color
			GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 4 * sizeof(float));
			GL.EnableVertexAttribArray(2);

			#endregion Vertex Array Object

			#region Element Buffer Object

			_elementBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

			#endregion Element Buffer Object

			_shader = new("Sprite.vert", "Sprite.frag");
			_texture = Texture.LoadFromFile("Sprite.png");
		}

		public void Start()
		{
		}

		public void End()
		{
			GL.BindVertexArray(_vertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StreamDraw);

			_shader.Use();
			_texture.Use();

			GL.DrawElements(PrimitiveType.Triangles, (int)_indexPosition, DrawElementsType.UnsignedInt, 0);

			_vertexPosition = 0;
			_vertexOffset = 0;

			_indexPosition = 0;
		}

		public void DrawBox(Rect rect, Color color)
		{
			var startingVertex = _vertexPosition;

			SetVertex(new(rect.x + rect.width, rect.y + rect.height), new(1.0f, 1.0f), color);  // Top Right
			SetVertex(new(rect.x + rect.width, rect.y), new(1.0f, 0.0f), color);                // Bottom Right
			SetVertex(new(rect.x, rect.y), new(0.0f, 0.0f), color);                             // Bottom Left
			SetVertex(new(rect.x, rect.y + rect.height), new(0.0f, 1.0f), color);               // Top Left

			// Bottom Right
			SetIndex(startingVertex + 0);
			SetIndex(startingVertex + 1);
			SetIndex(startingVertex + 3);

			// Top right
			SetIndex(startingVertex + 1);
			SetIndex(startingVertex + 2);
			SetIndex(startingVertex + 3);

			// SetVertex(new(rect.x, rect.y), new(0.0f, 0.0f), color);
			// SetVertex(new(rect.x, rect.y + rect.height), new(0.0f, 1.0f), color);
			// SetVertex(new(rect.x + rect.width, rect.y + rect.height), new(1.0f, 1.0f), color);
		}

		void SetVertex(Vector2 position, Vector2 textureCoord, Color color)
		{
			SetValue(position.x);
			SetValue(position.y);

			SetValue(textureCoord.x);
			SetValue(textureCoord.y);

			SetValue(color.r);
			SetValue(color.g);
			SetValue(color.b);
			SetValue(color.a);

			_vertexPosition++;
		}

		void SetValue(float value)
		{
			_vertices[_vertexOffset] = value;

			_vertexOffset++;
		}

		void SetIndex(uint index)
		{
			_indices[_indexPosition] = index;
			_indexPosition++;
		}

		public void Dispose()
		{
			GL.DeleteBuffer(_vertexBufferObject);
			_texture.Dispose();
			_shader.Dispose();
		}
	}
}
