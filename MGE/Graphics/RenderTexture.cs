using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics;

public class RenderTexture : GraphicsResource
{
	public readonly Vector2Int size;
	public readonly Matrix viewportTransform;

	public readonly Texture colorTexture;

	int _rbo;

	public RenderTexture(Vector2Int size) : base(GL.GenFramebuffer())
	{
		this.size = size;
		viewportTransform = Matrix.CreateOrthographic(size.x, size.y, 0, 1);

		colorTexture = new(this.size);

		SetAsTarget();

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture.handle, 0);

		_rbo = GL.GenRenderbuffer();
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
		GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, this.size.x, this.size.y);
		GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

		var errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		if (errorCode != FramebufferErrorCode.FramebufferComplete) throw new MGEException($"Error when initializing RenderTexture {((int)errorCode)} - {errorCode.ToString()}");

		SetScreenAsTarget();
	}

	internal void SetAsTarget() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

	internal static void SetScreenAsTarget() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

	protected override void Delete()
	{
		GL.DeleteFramebuffer(handle);
		GL.DeleteRenderbuffer(_rbo);
	}
}
