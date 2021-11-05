using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MGE
{
	public class SpriteBatch
	{
		int _position;
		float[] _vertices;

		int vbo;

		public SpriteBatch()
		{
			const int CAPACITY = 256;

			_vertices = new float[CAPACITY * (3 + 4)];

			vbo = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);
		}

		public void Reset()
		{
			_position = 0;
		}

		public void DrawBox(Rect rect, Color color)
		{

			SetVertex(new(rect.x, rect.y), color);
			SetVertex(new(rect.x + rect.width, rect.y), color);
			SetVertex(new(rect.x + rect.width, rect.y + rect.height), color);


			SetVertex(new(rect.x, rect.y), color);
			SetVertex(new(rect.x, rect.y + rect.height), color);
			SetVertex(new(rect.x + rect.width, rect.y + rect.height), color);
		}

		void SetVertex(Vector2 position, Color color)
		{
			SetValue(position.x);
			SetValue(position.y);
			SetValue(0f);

			SetValue(color.r);
			SetValue(color.g);
			SetValue(color.b);
			SetValue(color.a);
		}

		void SetValue(float value)
		{
			_vertices[_position] = value;
			_position++;
		}

		public void Draw()
		{

		}
	}
}
