using System;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics;

public class RenderTexture : GraphicsResource, IUseable
{
	public readonly Texture texture;

	public readonly Vector2Int size;

	int _rbo;

	public RenderTexture(Vector2Int size) : base(GL.GenFramebuffer())
	{
		this.size = size;

		texture = new(this.size);

		Use();

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture.handle, 0);

		_rbo = GL.GenRenderbuffer();
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
		GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, this.size.x, this.size.y);
		GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

		var errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		if (errorCode != FramebufferErrorCode.FramebufferComplete) throw new MGEException($"Error when initializing RenderTexture {((int)errorCode)} - {errorCode.ToString()}");

		StopUse();
	}

	public void Use() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

	public void StopUse() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

	protected override void Delete()
	{
		GL.DeleteFramebuffer(handle);
		GL.DeleteRenderbuffer(_rbo);
	}
}
