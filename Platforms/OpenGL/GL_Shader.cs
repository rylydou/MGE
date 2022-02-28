using System;
using System.Collections.Specialized;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MGE;

namespace MGE.OpenGL
{
	internal class GL_Shader : Shader.Platform
	{

		readonly GL_Graphics graphics;

		public uint ID { get; set; }

		internal GL_Shader(GL_Graphics graphics, ShaderSource source)
		{
			this.graphics = graphics;

			if (graphics.mainThreadId != Environment.CurrentManagedThreadId)
			{
				lock (graphics.BackgroundContext)
				{
					graphics.system.SetCurrentGLContext(graphics.BackgroundContext);

					Create();
					GL.Flush();

					graphics.system.SetCurrentGLContext(null);
				}
			}
			else
			{
				Create();
			}

			void Create()
			{
				ID = GL.CreateProgram();

				Span<uint> shaders = stackalloc uint[2];

				// vertex shader
				if (source.vertex is not null)
				{
					uint shaderId = GL.CreateShader(GLEnum.VERTEX_SHADER);
					shaders[0] = shaderId;

					string glsl = Encoding.UTF8.GetString(source.vertex);

					GL.ShaderSource(shaderId, 1, new[] { glsl }, new int[] { glsl.Length });
					GL.CompileShader(shaderId);

					string? errorMessage = GL.GetShaderInfoLog(shaderId);
					if (!string.IsNullOrEmpty(errorMessage))
						throw new Exception(errorMessage);

					GL.AttachShader(ID, shaderId);
				}

				// fragment shader
				if (source.fragment is not null)
				{
					uint shaderId = GL.CreateShader(GLEnum.FRAGMENT_SHADER);
					shaders[1] = shaderId;

					string glsl = Encoding.UTF8.GetString(source.fragment);

					GL.ShaderSource(shaderId, 1, new[] { glsl }, new int[] { glsl.Length });
					GL.CompileShader(shaderId);

					string? errorMessage = GL.GetShaderInfoLog(shaderId);
					if (!string.IsNullOrEmpty(errorMessage))
						throw new Exception(errorMessage);

					GL.AttachShader(ID, shaderId);
				}

				GL.LinkProgram(ID);

				string? programError = GL.GetProgramInfoLog(ID);
				if (!string.IsNullOrEmpty(programError))
					throw new Exception(programError);

				// get attributes
				GL.GetProgramiv(ID, GLEnum.ACTIVE_ATTRIBUTES, out int attributeCount);
				for (int i = 0; i < attributeCount; i++)
				{
					GL.GetActiveAttrib(ID, (uint)i, out _, out _, out string name);
					int location = GL.GetAttribLocation(ID, name);
					if (location >= 0)
						Attributes.Add(name, new ShaderAttribute(name, (uint)location));
				}

				// get uniforms
				GL.GetProgramiv(ID, GLEnum.ACTIVE_UNIFORMS, out int uniformCount);
				for (int i = 0; i < uniformCount; i++)
				{
					GL.GetActiveUniform(ID, (uint)i, out int size, out GLEnum type, out string name);
					int location = GL.GetUniformLocation(ID, name);
					if (location >= 0)
					{
						if (size > 1 && name.EndsWith("[0]"))
							name = name.Substring(0, name.Length - 3);
						Uniforms.Add(name, new GL_Uniform(this, name, size, location, type));
					}
				}

				// dispose shaders
				for (int i = 0; i < shaders.Length; i++)
				{
					if (shaders[i] != 0)
					{
						GL.DetachShader(ID, shaders[i]);
						GL.DeleteShader(shaders[i]);
					}
				}
			}
		}

		~GL_Shader()
		{
			Dispose();
		}

		public unsafe void Use(Material material)
		{
			GL.UseProgram(ID);

			// upload uniform values
			int textureSlot = 0;

			for (int p = 0; p < material.parameters.Count; p++)
			{
				var parameter = material.parameters[p];

				if (!(parameter.uniform is GL_Uniform uniform))
					continue;

				// Sampler 2D
				if (uniform.Type == UniformType.Sampler)
				{
#pragma warning disable CA2014 // Do not use stackalloc in loops
					int* n = stackalloc int[uniform.Length];
#pragma warning restore CA2014 // Do not use stackalloc in loops

					for (int i = 0; i < uniform.Length; i++)
					{
						var textures = (parameter.value as Texture?[]);
						var texture = textures?[i]?.implementation as GL_Texture;
						var id = texture?.ID ?? 0;

						GL.ActiveTexture((uint)(GLEnum.TEXTURE0 + textureSlot));
						GL.BindTexture(GLEnum.TEXTURE_2D, id);

						n[i] = textureSlot;
						textureSlot++;
					}

					GL.Uniform1iv(uniform.Location, uniform.Length, new IntPtr(n));
				}
				// Int
				else if (uniform.Type == UniformType.Int && parameter.value is int[] intArray)
				{
					fixed (int* ptr = intArray)
						GL.Uniform1iv(uniform.Location, uniform.Length, new IntPtr(ptr));
				}
				// Float
				else if (uniform.Type == UniformType.Float && parameter.value is float[] floatArray)
				{
					fixed (float* ptr = floatArray)
						GL.Uniform1fv(uniform.Location, uniform.Length, new IntPtr(ptr));
				}
				// Float2
				else if (uniform.Type == UniformType.Float2 && parameter.value is float[] float2Array)
				{
					fixed (float* ptr = float2Array)
						GL.Uniform2fv(uniform.Location, uniform.Length, new IntPtr(ptr));
				}
				// Float3
				else if (uniform.Type == UniformType.Float3 && parameter.value is float[] float3Array)
				{
					fixed (float* ptr = float3Array)
						GL.Uniform3fv(uniform.Location, uniform.Length, new IntPtr(ptr));
				}
				// Float4
				else if (uniform.Type == UniformType.Float4 && parameter.value is float[] float4Array)
				{
					fixed (float* ptr = float4Array)
						GL.Uniform4fv(uniform.Location, uniform.Length, new IntPtr(ptr));
				}
				// Matrix3x2
				else if (uniform.Type == UniformType.Matrix3x2 && parameter.value is float[] matrix3x2Array)
				{
					fixed (float* ptr = matrix3x2Array)
						GL.UniformMatrix3x2fv(uniform.Location, uniform.Length, false, new IntPtr(ptr));
				}
				// Matrix4x4
				else if (uniform.Type == UniformType.Matrix4x4 && parameter.value is float[] matrix4x4Array)
				{
					fixed (float* ptr = matrix4x4Array)
						GL.UniformMatrix4fv(uniform.Location, uniform.Length, false, new IntPtr(ptr));
				}
			}
		}

		protected override void Dispose()
		{
			if (ID != 0)
			{
				graphics.ProgramsToDelete.Add(ID);
				ID = 0;
			}
		}
	}
}
