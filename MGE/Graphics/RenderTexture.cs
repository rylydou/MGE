using System;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics;

public class RenderTexture : GraphicsResource, IUseable
{
	public readonly Texture texture;

	int _rbo;

	public RenderTexture(Vector2Int size) : base(GL.GenFramebuffer())
	{
		Use();

		texture = new(size);

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture.handle, 0);

		_rbo = GL.GenRenderbuffer();
		GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
		GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, size.x, size.y);
		GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

		var errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		if (errorCode != FramebufferErrorCode.FramebufferComplete) throw new Exception($"Error when initializing RenderTexture {((int)errorCode)} - {errorCode.ToString()}");

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
