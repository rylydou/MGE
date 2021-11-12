using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MGE.Graphics;

public class Shader : GraphicsResource, IUseable
{
	readonly Dictionary<string, int> _uniformLocations;

	public Shader(int handle) : base(handle)
	{
		GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

		_uniformLocations = new Dictionary<string, int>();

		for (var i = 0; i < numberOfUniforms; i++)
		{
			var key = GL.GetActiveUniform(handle, i, out _, out _);
			var location = GL.GetUniformLocation(handle, key);

			_uniformLocations.Add(key, location);
		}
	}

	public Shader(string vertPath, string fragPath) : base(GL.CreateProgram())
	{
		var shaderSource = File.ReadAllText($"{Environment.CurrentDirectory}/Assets/{vertPath}");
		var vertexShader = GL.CreateShader(ShaderType.VertexShader);

		GL.ShaderSource(vertexShader, shaderSource);

		CompileShader(vertexShader);

		shaderSource = File.ReadAllText($"{Environment.CurrentDirectory}/Assets/{fragPath}");
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

	public void Use() => GL.UseProgram(handle);

	public void StopUse() => GL.UseProgram(0);

	static void CompileShader(int shader)
	{
		GL.CompileShader(shader);
		GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);

		if (code != 1)
		{
			var infoLog = GL.GetShaderInfoLog(shader);

			throw new Exception($"Error occurred when compiling Shader\n\n{infoLog}");
		}
	}

	static void LinkProgram(int program)
	{
		GL.LinkProgram(program);
		GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);

		if (code != 1) throw new Exception($"Error occurred when linking Program");
	}

	public int GetAttribLocation(string attrName) => GL.GetAttribLocation(handle, attrName);

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

	public void SetVector2(string name, Vector2 data)
	{
		GL.UseProgram(handle);
		GL.Uniform2(_uniformLocations[name], data.x, data.y);
	}

	public void SetVector3(string name, Vector3 data)
	{
		GL.UseProgram(handle);
		GL.Uniform3(_uniformLocations[name], data.x, data.y, data.z);
	}

	public void SetVector4(string name, Vector4 data)
	{
		GL.UseProgram(handle);
		GL.Uniform4(_uniformLocations[name], data.x, data.y, data.z, data.w);
	}

	public void SetColor(string name, Color color)
	{
		GL.UseProgram(handle);
		GL.Uniform4(_uniformLocations[name], color.r, color.g, color.b, color.a);
	}

	public void SetMatrix(string name, Matrix data)
	{
		GL.UseProgram(handle);
		var mat = (Matrix4)data;
		GL.UniformMatrix4(_uniformLocations[name], true, ref mat);
	}

	protected override void Delete()
	{
		GL.DeleteProgram(handle);
	}
}
