using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	[DataContract]
	public class PixelCamera : Camera
	{
		public Vector2Int _targetResolution = new Vector2Int(320, 180);
		[DataMember]
		public Vector2Int targetResolution
		{
			get => _targetResolution;
			set
			{
				if (_targetResolution == value) return;
				_targetResolution = value;
				UpdateWindowSize();
			}
		}

		Vector2Int _trueResolution;
		public Vector2Int trueResolution
		{
			get => _trueResolution;
			protected set
			{
				if (_trueResolution == value) return;
				_trueResolution = value;
				renderTexture?.Dispose();
				renderTexture = new RenderTarget2D(GFX.gfxManager.GraphicsDevice, _trueResolution.x, _trueResolution.y);
				dirty = true;
			}
		}

		int _pixelsPerUnit = 16;
		[DataMember] public int pixelsPerUnit { get => _pixelsPerUnit; set { if (_pixelsPerUnit != value) { _pixelsPerUnit = value; dirty = true; } } }

		public float renderScale { get; private set; }

		public PixelCamera() { }

		public PixelCamera(Vector2Int targetResolution, int pixelsPerUnit)
		{
			this.targetResolution = targetResolution;
			this.pixelsPerUnit = pixelsPerUnit;
		}

		protected override void Init()
		{
			Window.onResize.Sub(() => UpdateWindowSize());
			UpdateWindowSize();

			base.Init();
		}

		protected override void OnDestroy()
		{
			Window.onResize.Unsub(() => UpdateWindowSize());

			base.OnDestroy();
		}

		protected override Matrix GetTransform()
		{
			dirty = false;
			_transform =
				Matrix.CreateTranslation(new Vector3(-_position.x * _pixelsPerUnit, -_position.y * _pixelsPerUnit, 0)) *
				Matrix.CreateRotationZ(rotation) *
				Matrix.CreateScale(zoom);/*  *
				Matrix.CreateTranslation((Vector2)trueResolution / 2); */
			return _transform;
		}

		protected void UpdateWindowSize()
		{
			renderScale = (float)targetResolution.y / Window.size.y;
			var windowSize = Window.size * renderScale;
			trueResolution = new Vector2Int((int)windowSize.x, targetResolution.y);
			LogVar(nameof(trueResolution), trueResolution);
		}

		public override Vector2 WindowToCamera(Vector2 position)
		{
			var invertedMatrix = Matrix.Invert(transform);
			return Microsoft.Xna.Framework.Vector2.Transform(position, invertedMatrix) / pixelsPerUnit * renderScale;
		}

		public override Vector2 CameraToWindow(Vector2 position)
		{
			var invertedMatrix = Matrix.Invert(transform);
			return Microsoft.Xna.Framework.Vector2.Transform(position, invertedMatrix) / renderScale * pixelsPerUnit;
		}
	}
}