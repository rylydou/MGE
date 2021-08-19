using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	[DataContract]
	public class Camera : Node
	{
		protected bool dirty = true;

		protected Vector2 _position = Vector2.zero;
		[DataMember] public Vector2 position { get => _position; set { if (_position != value) { _position = value; dirty = true; } } }

		protected float _rotation = 0;
		[DataMember] public float rotation { get => _rotation; set { if (_rotation != value) { _rotation = value; dirty = true; } } }

		protected float _zoom = 1;
		[DataMember] public float zoom { get => _zoom; set { if (_zoom != value) { _zoom = value; dirty = true; } } }

		protected Matrix _transform;
		public Matrix transform
		{
			get
			{
				if (dirty)
					GetTransform();
				return _transform;
			}
		}

		[DataMember] public bool clearScreen = true;
		[DataMember] public Color screenClearColor = new Color("#122020");

		[DataMember] public bool renderToScreenWithTransparency = false;
		[DataMember] public Effect postProcessEffect = null;
		[DataMember] public Color renderTint = Color.white;

		public RenderTarget2D renderTexture;

		protected override void Draw()
		{
			GFX.gfxManager.GraphicsDevice.SetRenderTarget(renderTexture);
			{
				if (clearScreen)
					GFX.gfxManager.GraphicsDevice.Clear(screenClearColor);

				GFX.StartTransform(transform);
				{
					base.Draw();
				}
				GFX.EndTransform();
			}
			GFX.gfxManager.GraphicsDevice.SetRenderTarget(null);

			GFX.StartBatch(renderToScreenWithTransparency, false, 1, postProcessEffect);
			{
				GFX.sb.Draw(renderTexture, new Rect(0, 0, Window.size), renderTint);
			}
			GFX.EndBatch();
		}

		protected virtual Matrix GetTransform()
		{
			dirty = false;

			_transform =
				Matrix.CreateTranslation(new Vector3(-_position.x, -_position.y, 0)) *
				Matrix.CreateRotationZ(rotation) *
				Matrix.CreateScale(zoom);

			return _transform;
		}

		public virtual Vector2 WindowToCamera(Vector2 position)
		{
			var invertedMatrix = Matrix.Invert(transform);
			return Microsoft.Xna.Framework.Vector2.Transform(position, invertedMatrix);
		}

		public virtual Vector2 CameraToWindow(Vector2 position)
		{
			var invertedMatrix = Matrix.Invert(transform);
			return Microsoft.Xna.Framework.Vector2.Transform(position, invertedMatrix);
		}
	}
}