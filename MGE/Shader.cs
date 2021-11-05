using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MGE
{
	public class Shader : GraphicsResource
	{
		readonly Dictionary<string, int> _uniformLocations;

		public Shader(string vertPath, string fragPath) : base(GL.CreateProgram())
		{
			var shaderSource = File.ReadAllText(vertPath);
			var vertexShader = GL.CreateShader(ShaderType.VertexShader);

			GL.ShaderSource(vertexShader, shaderSource);

			CompileShader(vertexShader);

			shaderSource = File.ReadAllText(fragPath);
			var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(fragmentShader, shaderSource);
			CompileShader(fragmentShader);

			GL.AttachShader(handle, vertexShader);
			GL.AttachShader(handle, fragmentShader);

			LinkProgram(handle);

			GL.DetachShader(handle, vertexShader);
			GL.DetachShader(handle, fragmentShader);
			GL.DeleteShader(fragmentShader);
			GL.DeleteShader(vertexShader);

			GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

			_uniformLocations = new Dictionary<string, int>();

			for (var i = 0; i < numberOfUniforms; i++)
			{
				var key = GL.GetActiveUniform(handle, i, out _, out _);
				var location = GL.GetUniformLocation(handle, key);

				_uniformLocations.Add(key, location);
			}
		}

		static void CompileShader(int shader)
		{
			GL.CompileShader(shader);
			GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);

			if (code != (int)All.True)
			{
				var infoLog = GL.GetShaderInfoLog(shader);

				throw new Exception($"Error occurred when compiling Shader({shader}).\n\n{infoLog}");
			}
		}

		static void LinkProgram(int program)
		{
			GL.LinkProgram(program);
			GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);

			if (code != (int)All.True) throw new Exception($"Error occurred when linking Program({program})");
		}

		public void Use() => GL.UseProgram(handle);

		public int GetAttribLocation(string attribName) => GL.GetAttribLocation(handle, attribName);

		public void SetInt(string name, int data)
		{
			GL.UseProgram(handle);
			GL.Uniform1(_uniformLocations[name], data);
		}

		public void SetFloat(string name, float data)
		{
			GL.UseProgram(handle);
			GL.Uniform1(_uniformLocations[name], data);
		}

		public void SetMatrix4(string name, Matrix4 data)
		{
			GL.UseProgram(handle);
			GL.UniformMatrix4(_uniformLocations[name], true, ref data);
		}

		public void SetVector3(string name, Vector3 data)
		{
			GL.UseProgram(handle);
			GL.Uniform3(_uniformLocations[name], data.x, data.y, data.z);
		}

		protected override void Dispose(bool manual)
		{
			if (!manual) return;

			GL.DeleteProgram(handle);
		}
	}
}
