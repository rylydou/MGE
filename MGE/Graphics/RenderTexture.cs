using System;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics;

public class RenderTexture : GraphicsResource, IUseable
{
	public readonly Texture texture;

	public RenderTexture(Vector2Int size) : base(GL.GenFramebuffer())
	{
		texture = new(size);

		Use();

		GL.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultWidth, size.x);
		GL.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultHeight, size.y);

		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.Color, TextureTarget.Texture2D, texture.handle, 0);

		var errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		if (errorCode != FramebufferErrorCode.FramebufferComplete) throw new Exception($"Error when initialising RenderTexture {((int)errorCode)} - {errorCode.ToString()}");

		StopUse();
	}

	public void Use() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

	public void StopUse() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

	protected override void Dispose(bool manual)
	{
		if (!manual) return;

		GL.DeleteFramebuffer(handle);
	}
}
