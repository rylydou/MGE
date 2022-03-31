using System;

namespace MGE;

public enum GraphicsRenderer
{
	None = 0,
	Unknown = 1,

	// Graphics

	OpenGLES,
	OpenGL,

	Vulkan,

	Direct3D9,
	Direct3D11,
	Direct3D12,

	Metal,

	// Audio

	OpenAL,
}

public abstract class Graphics : AppModule
{
	/// <summary>
	/// The underlying Graphics API Name
	/// </summary>
	public string apiName { get; protected set; } = "Unknown";

	/// <summary>
	/// The underlying Graphics API version
	/// </summary>
	public Version apiVersion { get; protected set; } = new Version(0, 0, 0);

	/// <summary>
	/// The Underlying Graphics Device Name
	/// </summary>
	public string deviceName { get; protected set; } = "Unknown";

	/// <summary>
	/// The Maximum Texture Width and Height supported, in pixels
	/// </summary>
	public int maxTextureSize { get; protected set; } = 0;

	/// <summary>
	/// Whether the Frame Buffer origin is in the bottom left
	/// </summary>
	public bool originBottomLeft { get; protected set; } = false;

	/// <summary>
	/// The Renderer this Graphics Module implements
	/// </summary>
	public abstract GraphicsRenderer renderer { get; }

	protected Graphics() : base(200)
	{

	}

	protected internal override void Startup()
	{
		Log.System($"{apiName} {apiVersion} ({deviceName})");
	}

	/// <summary>
	/// Clears the Color of the Target
	/// </summary>
	public void Clear(RenderTarget target, Color color) => Clear(target, MGE.Clear.Color, color, 0, 0, new RectInt(0, 0, target.renderWidth, target.renderHeight));

	/// <summary>
	/// Clears the Target
	/// </summary>
	public void Clear(RenderTarget target, Color color, float depth, int stencil) => Clear(target, MGE.Clear.All, color, depth, stencil, new RectInt(0, 0, target.renderWidth, target.renderHeight));

	/// <summary>
	/// Clears the Target
	/// </summary>
	public void Clear(RenderTarget target, Clear flags, Color color, float depth, int stencil, RectInt viewport)
	{
		if (!target.renderable)
			throw new Exception("Render Target cannot currently be drawn to");

		var bounds = new RectInt(0, 0, target.renderWidth, target.renderHeight);
		var clamped = RectInt.Intersect(viewport, bounds);

		ClearInternal(target, flags, color.Premultiply(), depth, stencil, clamped);
	}

	/// <summary>
	/// Clears the Target
	/// </summary>
	protected abstract void ClearInternal(RenderTarget target, Clear flags, Color color, float depth, int stencil, RectInt viewport);

	/// <summary>
	/// Draws the data from the Render pass to the Render Target.
	/// This will fail if the Target is not Drawable.
	/// </summary>
	public void Render(ref RenderPass pass)
	{
		if (!pass.target.renderable)
			throw new Exception("Render Target cannot currently be drawn to");

		if (!(pass.target is FrameBuffer) && !(pass.target is Window))
			throw new Exception("RenderTarget must be a Render Texture or a Window");

		if (pass.mesh is null)
			throw new Exception("Mesh cannot be null when drawing");

		if (pass.material is null)
			throw new Exception("Material cannot be null when drawing");

		if (pass.mesh.instanceCount > 0 && (pass.mesh.instanceFormat is null || (pass.mesh.instanceCount < pass.mesh.instanceCount)))
			throw new Exception("Trying to draw more Instances than exist in the Mesh");

		if (pass.mesh.indexCount < pass.meshIndexStart + pass.meshIndexCount)
			throw new Exception("Trying to draw more Indices than exist in the Mesh");

		if (pass.viewport is not null)
		{
			var bounds = new RectInt(0, 0, pass.target.renderWidth, pass.target.renderHeight);
			pass.viewport = RectInt.Intersect(pass.viewport.Value, bounds);
		}

		RenderInternal(ref pass);
	}

	protected abstract void RenderInternal(ref RenderPass pass);

	/// <summary>
	/// Creates a new Color Texture of the given size
	/// </summary>
	protected internal abstract Texture.Platform CreateTexture(int width, int height, TextureFormat format);

	/// <summary>
	/// Creates a new render texture of the given size, with the given amount of color and depth buffers
	/// </summary>
	protected internal abstract FrameBuffer.Platform CreateFrameBuffer(int width, int height, TextureFormat[] attachments);

	/// <summary>
	/// Creates a new Shader from the Shader Source
	/// </summary>
	protected internal abstract Shader.Platform CreateShader(ShaderSource source);

	/// <summary>
	/// Creates a new Mesh
	/// </summary>
	protected internal abstract Mesh.Platform CreateMesh();

	/// <summary>
	/// Gets the Shader Source for the Batch2D
	/// </summary>
	protected internal abstract ShaderSource CreateShaderSourceBatch2D();
}
