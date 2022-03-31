using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MGE;

/// <summary>
/// A 2D buffer that can be drawn to
/// </summary>
public class FrameBuffer : RenderTarget, IDisposable
{
	public abstract class Platform
	{
		protected internal readonly List<Texture> Attachments = new List<Texture>();
		protected internal abstract void Resize(int width, int height);
		protected internal abstract void Dispose();
	}

	/// <summary>
	/// A reference to the internal platform implementation of the FrameBuffer
	/// </summary>
	public Platform implementation { get; set; }

	/// <summary>
	/// Texture Attachments
	/// </summary>
	public ReadOnlyCollection<Texture> attachments { get; set; }

	/// <summary>
	/// Render Target Width
	/// </summary>
	public override int renderWidth => width;

	/// <summary>
	/// Render Target Height
	/// </summary>
	public override int renderHeight => height;

	int width;
	int height;

	public FrameBuffer(int width, int height) : this(App.graphics, width, height)
	{

	}

	public FrameBuffer(Graphics graphics, int width, int height) : this(graphics, width, height, TextureFormat.Color)
	{

	}

	public FrameBuffer(Graphics graphics, int width, int height, params TextureFormat[] attachments)
	{
		this.width = width;
		this.height = height;

		if (width <= 0 || height <= 0)
			throw new Exception("FrameBuffer must have a size larger than 0");

		implementation = graphics.CreateFrameBuffer(width, height, attachments);
		this.attachments = new ReadOnlyCollection<Texture>(implementation.Attachments);
		renderable = true;
	}

	public void Resize(int width, int height)
	{
		if (width <= 0 || height <= 0) throw new Exception("FrameBuffer must have a size larger than 0");

		if (this.width != width || this.height != height)
		{
			this.width = width;
			this.height = height;

			implementation.Resize(width, height);
		}
	}

	public void Dispose()
	{
		foreach (var texture in attachments)
			texture.Dispose();

		implementation.Dispose();
	}

	public static implicit operator Texture(FrameBuffer target) => target.attachments[0];
}
