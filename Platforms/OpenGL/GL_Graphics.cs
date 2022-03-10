using System;
using System.Collections.Generic;

namespace MGE.OpenGL;

public class GL_Graphics : Graphics, IGraphicsOpenGL
{
	internal ISystemOpenGL system => App.system as ISystemOpenGL ?? throw new Exception("System does not implement IGLSystem");

	// Background Context can be null up until Startup, at which point they never are again
	internal ISystemOpenGL.Context BackgroundContext = null!;

	// Stores info about the Context
	internal class ContextMeta
	{
		public List<uint> VertexArraysToDelete = new List<uint>();
		public List<uint> FrameBuffersToDelete = new List<uint>();
		public RenderTarget? LastRenderTarget;
		public RenderPass? LastRenderState;
		public RectInt Viewport;
		public bool ForceScissorUpdate;
	}

	// various resources waiting to be deleted
	internal List<uint> BuffersToDelete = new List<uint>();
	internal List<uint> ProgramsToDelete = new List<uint>();
	internal List<uint> TexturesToDelete = new List<uint>();

	// list of Contexts and their associated Metadata
	readonly Dictionary<ISystemOpenGL.Context, ContextMeta> contextMetadata = new Dictionary<ISystemOpenGL.Context, ContextMeta>();
	readonly List<ISystemOpenGL.Context> disposedContexts = new List<ISystemOpenGL.Context>();

	// stored delegates for deleting graphics resources
	delegate void DeleteResource(uint id);
	readonly DeleteResource deleteArray = GL.DeleteVertexArray;
	readonly DeleteResource deleteFramebuffer = GL.DeleteFramebuffer;
	readonly DeleteResource deleteBuffer = GL.DeleteBuffer;
	readonly DeleteResource deleteTexture = GL.DeleteTexture;
	readonly DeleteResource deleteProgram = GL.DeleteProgram;

	public override Renderer renderer => Renderer.OpenGL;

	protected override void ApplicationStarted()
	{
		apiName = "OpenGL";
	}

	protected override void FirstWindowCreated()
	{
		GL.Init(this, system);
		GL.DepthMask(true);

		maxTextureSize = GL.MaxTextureSize;
		originBottomLeft = true;
		apiVersion = new Version(GL.MajorVersion, GL.MinorVersion);
		deviceName = GL.GetString(GLEnum.RENDERER);

		BackgroundContext = system.CreateGLContext();
	}

	protected override void Shutdown()
	{
		BackgroundContext.Dispose();
	}

	protected override void FrameStart()
	{
		// delete any GL graphics resources that are shared between contexts
		DeleteResources(deleteBuffer, BuffersToDelete);
		DeleteResources(deleteProgram, ProgramsToDelete);
		DeleteResources(deleteTexture, TexturesToDelete);

		// check for any resources we're still tracking that are tied to contexts
		{
			var lastContext = system.GetCurrentGLContext();
			var changedContext = false;

			lock (contextMetadata)
			{
				foreach (var kv in contextMetadata)
				{
					var context = kv.Key;
					var meta = kv.Value;

					if (context.IsDisposed)
					{
						disposedContexts.Add(context);
					}
					else if (meta.FrameBuffersToDelete.Count > 0 || meta.VertexArraysToDelete.Count > 0)
					{
						lock (context)
						{
							system.SetCurrentGLContext(context);

							DeleteResources(deleteFramebuffer, meta.FrameBuffersToDelete);
							DeleteResources(deleteArray, meta.VertexArraysToDelete);

							changedContext = true;
						}
					}
				}

				foreach (var context in disposedContexts)
					contextMetadata.Remove(context);
			}

			if (changedContext)
				system.SetCurrentGLContext(lastContext);
		}
	}

	protected override void AfterRenderWindow(Window window)
	{
		GL.Flush();
	}

	void DeleteResources(DeleteResource deleter, List<uint> list)
	{
		lock (list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
				deleter(list[i]);
			list.Clear();
		}
	}

	internal ContextMeta GetContextMeta(ISystemOpenGL.Context context)
	{
		if (!contextMetadata.TryGetValue(context, out var meta))
			contextMetadata[context] = meta = new ContextMeta();
		return meta;
	}

	protected override Texture.Platform CreateTexture(int width, int height, TextureFormat format)
	{
		return new GL_Texture(this);
	}

	protected override FrameBuffer.Platform CreateFrameBuffer(int width, int height, TextureFormat[] attachments)
	{
		return new GL_FrameBuffer(this, width, height, attachments);
	}

	protected override Shader.Platform CreateShader(ShaderSource source)
	{
		return new GL_Shader(this, source);
	}

	protected override Mesh.Platform CreateMesh()
	{
		return new GL_Mesh(this);
	}

	protected override ShaderSource CreateShaderSourceBatch2D()
	{
		var vertexSource = Calc.EmbeddedResourceText("Resources/batch2d.vert");
		var fragmentSource = Calc.EmbeddedResourceText("Resources/batch2d.frag");

		return new ShaderSource(vertexSource, fragmentSource);
	}

	protected override void ClearInternal(RenderTarget target, Clear flags, Color color, float depth, int stencil, RectInt viewport)
	{
		if (target is Window window)
		{
			var context = system.GetWindowGLContext(window);
			lock (context)
			{
				if (context != system.GetCurrentGLContext())
					system.SetCurrentGLContext(context);
				GL.BindFramebuffer(GLEnum.FRAMEBUFFER, 0);
				Clear(this, context, target, flags, color, depth, stencil, viewport);
			}
		}
		else if (target is FrameBuffer rt && rt.implementation is GL_FrameBuffer renderTexture)
		{
			// if we're off the main thread, clear using the Background Context
			if (mainThreadId != Environment.CurrentManagedThreadId)
			{
				lock (BackgroundContext)
				{
					system.SetCurrentGLContext(BackgroundContext);

					renderTexture.Bind(BackgroundContext);
					Clear(this, BackgroundContext, target, flags, color, depth, stencil, viewport);
					GL.Flush();

					system.SetCurrentGLContext(null);
				}
			}
			// otherwise just clear, regardless of Context
			else
			{
				var context = system.GetCurrentGLContext();
				if (context is null)
				{
					context = system.GetWindowGLContext(App.window);
					system.SetCurrentGLContext(context);
				}

				lock (context)
				{
					renderTexture.Bind(context);
					Clear(this, context, target, flags, color, depth, stencil, viewport);
				}
			}
		}

		static void Clear(GL_Graphics graphics, ISystemOpenGL.Context context, RenderTarget target, Clear flags, Color color, float depth, int stencil, RectInt viewport)
		{
			// update the viewport
			var meta = graphics.GetContextMeta(context);
			{
				viewport.y = target.renderHeight - viewport.y - viewport.height;

				if (meta.Viewport != viewport)
				{
					GL.Viewport(viewport.x, viewport.y, viewport.width, viewport.height);
					meta.Viewport = viewport;
				}
			}

			// we disable the scissor for clearing
			meta.ForceScissorUpdate = true;
			GL.Disable(GLEnum.SCISSOR_TEST);

			// clear
			var mask = GLEnum.ZERO;

			if (flags.HasFlag(MGE.Clear.Color))
			{
				GL.ClearColor(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f);
				mask |= GLEnum.COLOR_BUFFER_BIT;
			}

			if (flags.HasFlag(MGE.Clear.Depth))
			{
				GL.ClearDepth(depth);
				mask |= GLEnum.DEPTH_BUFFER_BIT;
			}

			if (flags.HasFlag(MGE.Clear.Stencil))
			{
				GL.ClearStencil(stencil);
				mask |= GLEnum.STENCIL_BUFFER_BIT;
			}

			GL.Clear(mask);
			GL.BindFramebuffer(GLEnum.FRAMEBUFFER, 0);
		}
	}

	protected override void RenderInternal(ref RenderPass pass)
	{
		if (pass.target is Window window)
		{
			var context = system.GetWindowGLContext(window);
			lock (context)
			{
				if (context != system.GetCurrentGLContext())
					system.SetCurrentGLContext(context);
				Draw(this, ref pass, context);
			}
		}
		else if (mainThreadId != Environment.CurrentManagedThreadId)
		{
			lock (BackgroundContext)
			{
				system.SetCurrentGLContext(BackgroundContext);

				Draw(this, ref pass, BackgroundContext);
				GL.Flush();

				system.SetCurrentGLContext(null);
			}
		}
		else
		{
			var context = system.GetCurrentGLContext();
			if (context is null)
			{
				context = system.GetWindowGLContext(App.window);
				system.SetCurrentGLContext(context);
			}

			lock (context)
			{
				Draw(this, ref pass, context);
			}
		}

		static void Draw(GL_Graphics graphics, ref RenderPass pass, ISystemOpenGL.Context context)
		{
			RenderPass lastPass;

			// get the previous state
			var updateAll = false;
			var contextMeta = graphics.GetContextMeta(context);
			if (contextMeta.LastRenderState is null)
			{
				updateAll = true;
				lastPass = pass;
			}
			else
				lastPass = contextMeta.LastRenderState.Value;
			contextMeta.LastRenderState = pass;

			// Bind the Target
			if (updateAll || contextMeta.LastRenderTarget != pass.target)
			{
				if (pass.target is Window)
				{
					GL.BindFramebuffer(GLEnum.FRAMEBUFFER, 0);
				}
				else if (pass.target is FrameBuffer rt && rt.implementation is GL_FrameBuffer renderTexture)
				{
					renderTexture.Bind(context);
				}

				contextMeta.LastRenderTarget = pass.target;
			}

			// Use the Shader
			if (pass.material.shader.implementation is GL_Shader glShader)
				glShader.Use(pass.material);

			// Bind the Mesh
			if (pass.mesh.implementation is GL_Mesh glMesh)
				glMesh.Bind(context, pass.material);

			// Blend Mode
			{
				GL.Enable(GLEnum.BLEND);

				if (updateAll ||
						lastPass.blendMode.colorOperation != pass.blendMode.colorOperation ||
						lastPass.blendMode.alphaOperation != pass.blendMode.alphaOperation)
				{
					GLEnum colorOp = GetBlendFunc(pass.blendMode.colorOperation);
					GLEnum alphaOp = GetBlendFunc(pass.blendMode.alphaOperation);

					GL.BlendEquationSeparate(colorOp, alphaOp);
				}

				if (updateAll ||
						lastPass.blendMode.colorSource != pass.blendMode.colorSource ||
						lastPass.blendMode.colorDestination != pass.blendMode.colorDestination ||
						lastPass.blendMode.alphaSource != pass.blendMode.alphaSource ||
						lastPass.blendMode.alphaDestination != pass.blendMode.alphaDestination)
				{
					GLEnum colorSrc = GetBlendFactor(pass.blendMode.colorSource);
					GLEnum colorDst = GetBlendFactor(pass.blendMode.colorDestination);
					GLEnum alphaSrc = GetBlendFactor(pass.blendMode.alphaSource);
					GLEnum alphaDst = GetBlendFactor(pass.blendMode.alphaDestination);

					GL.BlendFuncSeparate(colorSrc, colorDst, alphaSrc, alphaDst);
				}

				if (updateAll || lastPass.blendMode.mask != pass.blendMode.mask)
				{
					GL.ColorMask(
							(pass.blendMode.mask & BlendMask.Red) != 0,
							(pass.blendMode.mask & BlendMask.Green) != 0,
							(pass.blendMode.mask & BlendMask.Blue) != 0,
							(pass.blendMode.mask & BlendMask.Alpha) != 0);
				}

				if (updateAll || lastPass.blendMode.color != pass.blendMode.color)
				{
					GL.BlendColor(
							pass.blendMode.color.r / 255f,
							pass.blendMode.color.g / 255f,
							pass.blendMode.color.b / 255f,
							pass.blendMode.color.a / 255f);
				}
			}

			// Depth Function
			if (updateAll || lastPass.depthFunction != pass.depthFunction)
			{
				if (pass.depthFunction == Compare.None)
				{
					GL.Disable(GLEnum.DEPTH_TEST);
				}
				else
				{
					GL.Enable(GLEnum.DEPTH_TEST);

					switch (pass.depthFunction)
					{
						case Compare.Always:
							GL.DepthFunc(GLEnum.ALWAYS);
							break;
						case Compare.Equal:
							GL.DepthFunc(GLEnum.EQUAL);
							break;
						case Compare.Greater:
							GL.DepthFunc(GLEnum.GREATER);
							break;
						case Compare.GreaterOrEqual:
							GL.DepthFunc(GLEnum.GEQUAL);
							break;
						case Compare.Less:
							GL.DepthFunc(GLEnum.LESS);
							break;
						case Compare.LessOrEqual:
							GL.DepthFunc(GLEnum.LEQUAL);
							break;
						case Compare.Never:
							GL.DepthFunc(GLEnum.NEVER);
							break;
						case Compare.NotEqual:
							GL.DepthFunc(GLEnum.NOTEQUAL);
							break;

						default:
							throw new NotImplementedException();
					}
				}
			}

			// Cull Mode
			if (updateAll || lastPass.cullMode != pass.cullMode)
			{
				if (pass.cullMode == CullMode.None)
				{
					GL.Disable(GLEnum.CULL_FACE);
				}
				else
				{
					GL.Enable(GLEnum.CULL_FACE);

					if (pass.cullMode == CullMode.Back)
						GL.CullFace(GLEnum.BACK);
					else if (pass.cullMode == CullMode.Front)
						GL.CullFace(GLEnum.FRONT);
					else
						GL.CullFace(GLEnum.FRONT_AND_BACK);
				}
			}

			// Viewport
			var viewport = pass.viewport ?? new RectInt(0, 0, pass.target.renderWidth, pass.target.renderHeight);
			{
				viewport.y = pass.target.renderHeight - viewport.y - viewport.height;

				if (updateAll || contextMeta.Viewport != viewport)
				{
					GL.Viewport(viewport.x, viewport.y, viewport.width, viewport.height);
					contextMeta.Viewport = viewport;
				}
			}

			// Scissor
			{
				var scissor = pass.scissor ?? new RectInt(0, 0, pass.target.renderWidth, pass.target.renderHeight);
				scissor.y = pass.target.renderHeight - scissor.y - scissor.height;
				scissor.width = Math.Max(0, scissor.width);
				scissor.height = Math.Max(0, scissor.height);

				if (updateAll || lastPass.scissor != scissor || contextMeta.ForceScissorUpdate)
				{
					if (pass.scissor is null)
					{
						GL.Disable(GLEnum.SCISSOR_TEST);
					}
					else
					{
						GL.Enable(GLEnum.SCISSOR_TEST);
						GL.Scissor(scissor.x, scissor.y, scissor.width, scissor.height);
					}

					contextMeta.ForceScissorUpdate = false;
					lastPass.scissor = scissor;
				}
			}
			GLEnum indexType = pass.indexElementSize == IndexElementSize.ThirtyTwoBits ? GLEnum.UNSIGNED_INT : GLEnum.UNSIGNED_SHORT;
			int indexSize = pass.indexElementSize == IndexElementSize.ThirtyTwoBits ? sizeof(int) : sizeof(ushort);

			// Draw the Mesh
			{
				if (pass.meshInstanceCount > 0)
				{
					GL.DrawElementsInstanced(GLEnum.TRIANGLES, (int)(pass.meshIndexCount), indexType, new IntPtr(indexSize * pass.meshIndexStart), (int)pass.meshInstanceCount);
				}
				else
				{
					GL.DrawElements(GLEnum.TRIANGLES, (int)(pass.meshIndexCount), indexType, new IntPtr(indexSize * pass.meshIndexStart));
				}

				GL.BindVertexArray(0);
			}
		}
	}

	static GLEnum GetBlendFunc(BlendOperations operation)
	{
		return operation switch
		{
			BlendOperations.Add => GLEnum.FUNC_ADD,
			BlendOperations.Subtract => GLEnum.FUNC_SUBTRACT,
			BlendOperations.ReverseSubtract => GLEnum.FUNC_REVERSE_SUBTRACT,
			BlendOperations.Min => GLEnum.MIN,
			BlendOperations.Max => GLEnum.MAX,

			_ => throw new InvalidOperationException($"Unsupported Blend Opteration {operation}"),
		};
	}

	static GLEnum GetBlendFactor(BlendFactors factor)
	{
		return factor switch
		{
			BlendFactors.Zero => GLEnum.ZERO,
			BlendFactors.One => GLEnum.ONE,
			BlendFactors.SrcColor => GLEnum.SRC_COLOR,
			BlendFactors.OneMinusSrcColor => GLEnum.ONE_MINUS_SRC_COLOR,
			BlendFactors.DstColor => GLEnum.DST_COLOR,
			BlendFactors.OneMinusDstColor => GLEnum.ONE_MINUS_DST_COLOR,
			BlendFactors.SrcAlpha => GLEnum.SRC_ALPHA,
			BlendFactors.OneMinusSrcAlpha => GLEnum.ONE_MINUS_SRC_ALPHA,
			BlendFactors.DstAlpha => GLEnum.DST_ALPHA,
			BlendFactors.OneMinusDstAlpha => GLEnum.ONE_MINUS_DST_ALPHA,
			BlendFactors.ConstantColor => GLEnum.CONSTANT_COLOR,
			BlendFactors.OneMinusConstantColor => GLEnum.ONE_MINUS_CONSTANT_COLOR,
			BlendFactors.ConstantAlpha => GLEnum.CONSTANT_ALPHA,
			BlendFactors.OneMinusConstantAlpha => GLEnum.ONE_MINUS_CONSTANT_ALPHA,
			BlendFactors.SrcAlphaSaturate => GLEnum.SRC_ALPHA_SATURATE,
			BlendFactors.Src1Color => GLEnum.SRC1_COLOR,
			BlendFactors.OneMinusSrc1Color => GLEnum.ONE_MINUS_SRC1_COLOR,
			BlendFactors.Src1Alpha => GLEnum.SRC1_ALPHA,
			BlendFactors.OneMinusSrc1Alpha => GLEnum.ONE_MINUS_SRC1_ALPHA,

			_ => throw new InvalidOperationException($"Unsupported Blend Factor {factor}"),
		};
	}
}
