using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MGE
{
	public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
	{
		private float[] _vertices =
		{
			// Position	 Texture	 Color
				 0,  0,    0,  0,    1.0f,  1.0f,  1.0f,  1.0f,
				 0,  1,    0,  1,    1.0f,  1.0f,  1.0f,  1.0f,
				 1,  1,    1,  1,    1.0f,  1.0f,  1.0f,  1.0f,

				 0,  0,    0,  0,    1.0f,  1.0f,  1.0f,  1.0f,
				 1,  0,    1,  0,    1.0f,  1.0f,  1.0f,  1.0f,
				 1,  1,    1,  1,    1.0f,  1.0f,  1.0f,  1.0f,
		};

		int _vertexBufferObject;
		int _vertexArrayObject;
		Shader _shader;
		Texture _sprite;

		public GameWindow() : base(new GameWindowSettings(), new NativeWindowSettings())
		{
			_shader = new("Assets/Sprite.vert", "Assets/Sprite.frag");
			_sprite = Texture.LoadFromFile("Sprite.png");
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.ClearColor(new Color("#161616"));

			_vertexBufferObject = GL.GenBuffer();

			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);

			_vertexArrayObject = GL.GenVertexArray();
			GL.BindVertexArray(_vertexArrayObject);

			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);

			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 2 * sizeof(float));
			GL.EnableVertexAttribArray(2);

			GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 4 * sizeof(float));
			GL.EnableVertexAttribArray(1);

			_shader.Use();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			GL.BindVertexArray(_vertexArrayObject);
			_shader.Use();
			_sprite.Use();

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			SwapBuffers();
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			var input = KeyboardState;

			if (input.IsKeyDown(Keys.Up))
			{
				_vertices[1] += (float)e.Time;
			}
			if (input.IsKeyDown(Keys.Down))
			{
				_vertices[1] -= (float)e.Time;
			}

			if (input.IsKeyDown(Keys.Right))
			{
				_vertices[0] += (float)e.Time;
			}
			if (input.IsKeyDown(Keys.Left))
			{
				_vertices[0] -= (float)e.Time;
			}

			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StreamDraw);

			if (input.IsKeyDown(Keys.Escape))
			{
				Close();
			}
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);

			GL.Viewport(0, 0, Size.X, Size.Y);
		}

		protected override void OnUnload()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			GL.UseProgram(0);

			GL.DeleteBuffer(_vertexBufferObject);
			GL.DeleteVertexArray(_vertexArrayObject);

			GL.DeleteProgram(_shader.handle);

			_shader.Dispose();
			_sprite.Dispose();

			base.OnUnload();
		}
	}
}
