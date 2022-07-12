using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace MGE
{
	/// <summary>
	/// A Subtexture, representing a rectangular segment of a Texture
	/// </summary>
	public class Subtexture
	{
		/// <summary>
		/// The Texture coordinates. These are set automatically based on the Source rectangle
		/// </summary>
		public readonly Vector2[] texCoords = new Vector2[4];

		/// <summary>
		/// The draw coordinates. These are set automatically based on the Source and Frame rectangle
		/// </summary>
		public readonly Vector2[] drawCoords = new Vector2[4];

		/// <summary>
		/// The Texture this Subtexture is... a subtexture of
		/// </summary>
		public Texture? texture
		{
			get => _texture;
			set
			{
				if (_texture != value)
				{
					_texture = value;
					UpdateCoords();
				}
			}
		}

		/// <summary>
		/// The source rectangle to sample from the Texture
		/// </summary>
		public Rect source
		{
			get => _source;
			set
			{
				_source = value;
				UpdateCoords();
			}
		}

		/// <summary>
		/// The frame of the Subtexture. This is useful if you trim transparency and want to store the original size of the image
		/// For example, if the original image was (64, 64), but the trimmed version is (32, 48), the Frame may be (-16, -8, 64, 64)
		/// </summary>
		public Rect frame
		{
			get => _frame;
			set
			{
				_frame = value;
				UpdateCoords();
			}
		}

		/// <summary>
		/// The Draw width of the Subtexture
		/// </summary>
		public float width => _frame.width;

		/// <summary>
		/// The Draw height of the Subtexture
		/// </summary>
		public float height => _frame.height;

		Texture? _texture;
		Rect _frame;
		Rect _source;

		public Subtexture()
		{

		}

		public Subtexture(Texture texture) : this(texture, new Rect(0, 0, texture.width, texture.height))
		{

		}

		public Subtexture(Texture texture, Rect source) : this(texture, source, new Rect(0, 0, source.width, source.height))
		{

		}

		public Subtexture(Texture texture, Rect source, Rect frame)
		{
			this._texture = texture;
			this._source = source;
			this._frame = frame;

			UpdateCoords();
		}

		public void Reset(Texture texture, Rect source, Rect frame)
		{
			this._texture = texture;
			this._source = source;
			this._frame = frame;

			UpdateCoords();
		}

		public (Rect Source, Rect Frame) GetClip(in Rect clip)
		{
			(Rect Source, Rect Frame) result = new();

			result.Source = RectInt.Intersect(clip + source.position + frame.position, source);

			result.Frame.x = Mathf.Min(0, frame.x + clip.x);
			result.Frame.y = Mathf.Min(0, frame.y + clip.y);
			result.Frame.width = clip.width;
			result.Frame.height = clip.height;

			return result;
		}

		public (Rect Source, Rect Frame) GetClip(float x, float y, float w, float h)
		{
			return GetClip(new Rect(x, y, w, h));
		}

		public Subtexture GetClipSubtexture(Rect clip)
		{
			var (source, frame) = GetClip(clip);
			return new Subtexture(texture!, source, frame);
		}

		void UpdateCoords()
		{
			drawCoords[0].x = -_frame.x;
			drawCoords[0].y = -_frame.y;
			drawCoords[1].x = -_frame.x + _source.width;
			drawCoords[1].y = -_frame.y;
			drawCoords[2].x = -_frame.x + _source.width;
			drawCoords[2].y = -_frame.y + _source.height;
			drawCoords[3].x = -_frame.x;
			drawCoords[3].y = -_frame.y + _source.height;

			if (_texture is not null)
			{
				var tx0 = _source.x / _texture.width;
				var ty0 = _source.y / _texture.height;
				var tx1 = _source.right / _texture.width;
				var ty1 = _source.bottom / _texture.height;

				texCoords[0].x = tx0;
				texCoords[0].y = ty0;
				texCoords[1].x = tx1;
				texCoords[1].y = ty0;
				texCoords[2].x = tx1;
				texCoords[2].y = ty1;
				texCoords[3].x = tx0;
				texCoords[3].y = ty1;
			}
		}

	}
}
