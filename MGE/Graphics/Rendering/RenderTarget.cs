namespace MGE.Graphics.Rendering;

/// <summary>
/// An Object that can be rendered to (ex. a FrameBuffer or a Window)
/// </summary>
public abstract class RenderTarget
{
	/// <summary>
	/// The Width of the Target
	/// </summary>
	public abstract int renderWidth { get; }

	/// <summary>
	/// The Height of the Target
	/// </summary>
	public abstract int renderHeight { get; }

	/// <summary>
	/// Whether the Target can be rendered to.
	/// </summary>
	public bool renderable { get; internal protected set; }
}
