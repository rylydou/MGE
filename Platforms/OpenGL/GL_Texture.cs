﻿using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace MGE.OpenGL;

internal class GL_Texture : Texture.Platform
{
	public uint ID { get; set; }

	readonly GL_Graphics graphics;
	internal bool isRenderTexture;

	Texture texture;
	GLEnum glInternalFormat;
	GLEnum glFormat;
	GLEnum glType;

	internal GL_Texture(GL_Graphics graphics)
	{
		this.graphics = graphics;
		texture = null!;
	}

	~GL_Texture()
	{
		Dispose();
	}

	protected override bool IsFrameBuffer()
	{
		return isRenderTexture;
	}

	protected override void Init(Texture texture)
	{
		this.texture = texture;

		glInternalFormat = texture.format switch
		{
			TextureFormat.Red => GLEnum.RED,
			TextureFormat.RG => GLEnum.RG,
			TextureFormat.RGB => GLEnum.RGB,
			TextureFormat.Color => GLEnum.RGBA,
			TextureFormat.DepthStencil => GLEnum.DEPTH24_STENCIL8,
			_ => throw new MGException("Invalid Texture Format"),
		};

		glFormat = texture.format switch
		{
			TextureFormat.Red => GLEnum.RED,
			TextureFormat.RG => GLEnum.RG,
			TextureFormat.RGB => GLEnum.RGB,
			TextureFormat.Color => GLEnum.RGBA,
			TextureFormat.DepthStencil => GLEnum.DEPTH_STENCIL,
			_ => throw new MGException("Invalid Texture Format"),
		};

		glType = texture.format switch
		{
			TextureFormat.Red => GLEnum.UNSIGNED_BYTE,
			TextureFormat.RG => GLEnum.UNSIGNED_BYTE,
			TextureFormat.RGB => GLEnum.UNSIGNED_BYTE,
			TextureFormat.Color => GLEnum.UNSIGNED_BYTE,
			TextureFormat.DepthStencil => GLEnum.UNSIGNED_INT_24_8,
			_ => throw new MGException("Invalid Texture Format"),
		};

		Initialize();
	}

	void Initialize()
	{
		if (graphics.mainThreadId != Environment.CurrentManagedThreadId)
		{
			lock (graphics._backgroundContext)
			{
				graphics.system.SetCurrentGLContext(graphics._backgroundContext);

				Init();
				GL.Flush();

				graphics.system.SetCurrentGLContext(graphics._backgroundContext);
			}
		}
		else
		{
			Init();
		}

		void Init()
		{
			ID = GL.GenTexture();
			GL.ActiveTexture((uint)GLEnum.TEXTURE0);
			GL.BindTexture(GLEnum.TEXTURE_2D, ID);

			GL.TexImage2D(GLEnum.TEXTURE_2D, 0, glInternalFormat, texture.width, texture.height, 0, glFormat, glType, new IntPtr(0));
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_MIN_FILTER, (int)(texture.filter == TextureFilter.Nearest ? GLEnum.NEAREST : GLEnum.LINEAR));
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_MAG_FILTER, (int)(texture.filter == TextureFilter.Nearest ? GLEnum.NEAREST : GLEnum.LINEAR));
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_WRAP_S, (int)(texture.wrapX == TextureWrap.Clamp ? GLEnum.CLAMP_TO_EDGE : GLEnum.REPEAT));
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_WRAP_T, (int)(texture.wrapY == TextureWrap.Clamp ? GLEnum.CLAMP_TO_EDGE : GLEnum.REPEAT));
		}
	}

	protected override void Resize(int width, int height)
	{
		Dispose();
		Initialize();
	}

	protected override void SetFilter(TextureFilter filter)
	{
		GLEnum f = (filter == TextureFilter.Nearest ? GLEnum.NEAREST : GLEnum.LINEAR);

		if (graphics.mainThreadId != Environment.CurrentManagedThreadId)
		{
			lock (graphics._backgroundContext)
			{
				graphics.system.SetCurrentGLContext(graphics._backgroundContext);

				SetFilter(ID, f);
				GL.Flush();

				graphics.system.SetCurrentGLContext(graphics._backgroundContext);
			}
		}
		else
		{
			SetFilter(ID, f);
		}

		static void SetFilter(uint id, GLEnum f)
		{
			GL.ActiveTexture((uint)GLEnum.TEXTURE0);
			GL.BindTexture(GLEnum.TEXTURE_2D, id);
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_MIN_FILTER, (int)f);
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_MAG_FILTER, (int)f);
		}
	}

	protected override void SetWrap(TextureWrap x, TextureWrap y)
	{
		GLEnum s = (x == TextureWrap.Clamp ? GLEnum.CLAMP_TO_EDGE : GLEnum.REPEAT);
		GLEnum t = (y == TextureWrap.Clamp ? GLEnum.CLAMP_TO_EDGE : GLEnum.REPEAT);

		if (graphics.mainThreadId != Environment.CurrentManagedThreadId)
		{
			lock (graphics._backgroundContext)
			{
				graphics.system.SetCurrentGLContext(graphics._backgroundContext);

				SetFilter(ID, s, t);
				GL.Flush();

				graphics.system.SetCurrentGLContext(graphics._backgroundContext);
			}
		}
		else
		{
			SetFilter(ID, s, t);
		}

		static void SetFilter(uint id, GLEnum s, GLEnum t)
		{
			GL.ActiveTexture((uint)GLEnum.TEXTURE0);
			GL.BindTexture(GLEnum.TEXTURE_2D, id);
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_WRAP_S, (int)s);
			GL.TexParameteri(GLEnum.TEXTURE_2D, GLEnum.TEXTURE_WRAP_T, (int)t);
		}
	}

	protected override void SetData<T>(RectInt rect, T[] pixels) where T : struct
	{
		if (graphics.mainThreadId != Environment.CurrentManagedThreadId)
		{
			lock (graphics._backgroundContext)
			{
				graphics.system.SetCurrentGLContext(graphics._backgroundContext);

				Upload();
				GL.Flush();

				graphics.system.SetCurrentGLContext(graphics._backgroundContext);
			}
		}
		else
		{
			Upload();
		}

		void Upload()
		{
			var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			var pointer = handle.AddrOfPinnedObject();

			GL.ActiveTexture((uint)GLEnum.TEXTURE0);
			GL.BindTexture(GLEnum.TEXTURE_2D, ID);
			GL.TexSubImage2D(GLEnum.TEXTURE_2D, 0, rect.x, rect.y, rect.width, rect.height, glFormat, glType, pointer);

			handle.Free();
		}
	}

	protected override unsafe void SetData<T>(ReadOnlyMemory<T> buffer)
	{
		using MemoryHandle handle = buffer.Pin();

		if (graphics.mainThreadId != Environment.CurrentManagedThreadId)
		{
			lock (graphics._backgroundContext)
			{
				graphics.system.SetCurrentGLContext(graphics._backgroundContext);

				Upload();
				GL.Flush();

				graphics.system.SetCurrentGLContext(graphics._backgroundContext);
			}
		}
		else
		{
			Upload();
		}

		void Upload()
		{
			GL.ActiveTexture((uint)GLEnum.TEXTURE0);
			GL.BindTexture(GLEnum.TEXTURE_2D, ID);
			GL.TexImage2D(GLEnum.TEXTURE_2D, 0, glInternalFormat, texture.width, texture.height, 0, glFormat, glType, new IntPtr(handle.Pointer));
		}
	}

	protected override unsafe void GetData<T>(Memory<T> buffer)
	{
		using var handle = buffer.Pin();

		if (graphics.mainThreadId != Environment.CurrentManagedThreadId)
		{
			lock (graphics._backgroundContext)
			{
				graphics.system.SetCurrentGLContext(graphics._backgroundContext);

				Download();
				GL.Flush();

				graphics.system.SetCurrentGLContext(graphics._backgroundContext);
			}
		}
		else
		{
			Download();
		}

		void Download()
		{
			GL.ActiveTexture((uint)GLEnum.TEXTURE0);
			GL.BindTexture(GLEnum.TEXTURE_2D, ID);
			GL.GetTexImage(GLEnum.TEXTURE_2D, 0, glInternalFormat, glType, new IntPtr(handle.Pointer));
		}
	}

	protected override void Dispose()
	{
		if (ID != 0)
		{
			graphics.texturesToDelete.Add(ID);
			ID = 0;
		}
	}
}
