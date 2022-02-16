using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace MGE;

/// <summary>
/// A Shader used for Rendering
/// </summary>
public class Shader : IDisposable
{
	public abstract class Platform
	{
		protected internal readonly Dictionary<string, ShaderAttribute> Attributes = new Dictionary<string, ShaderAttribute>();
		protected internal readonly Dictionary<string, ShaderUniform> Uniforms = new Dictionary<string, ShaderUniform>();
		protected internal abstract void Dispose();
	}

	/// <summary>
	/// A reference to the internal platform implementation of the Shader
	/// </summary>
	public readonly Platform implementation;

	/// <summary>
	/// List of all Vertex Attributes, by Name
	/// </summary>
	public readonly ReadOnlyDictionary<string, ShaderAttribute> Attributes;

	/// <summary>
	/// List of all Uniforms, by Name
	/// </summary>
	public readonly ReadOnlyDictionary<string, ShaderUniform> Uniforms;

	public Shader(Graphics graphics, ShaderSource source)
	{
		implementation = graphics.CreateShader(source);
		Uniforms = new ReadOnlyDictionary<string, ShaderUniform>(implementation.Uniforms);
		Attributes = new ReadOnlyDictionary<string, ShaderAttribute>(implementation.Attributes);
	}

	public Shader(ShaderSource source) : this(App.graphics, source)
	{
	}

	public void Dispose()
	{
		implementation.Dispose();
	}
}

/// <summary>
/// A Shader Attribute
/// </summary>
public class ShaderAttribute
{
	/// <summary>
	/// The name of the Attribute
	/// </summary>
	public readonly string Name;

	/// <summary>
	/// The Location of the Attribute in the Shader
	/// </summary>
	public readonly uint Location;

	public ShaderAttribute(string name, uint location)
	{
		Name = name;
		Location = location;
	}
}

/// <summary>
/// A Shader Uniform Type
/// </summary>
public enum UniformType
{
	Unknown,
	Int,
	Float,
	Float2,
	Float3,
	Float4,
	Matrix3x2,
	Matrix4x4,
	Sampler
}

/// <summary>
/// A Shader Uniform Value
/// </summary>
public class ShaderUniform
{
	/// <summary>
	/// The Name of the Uniform
	/// </summary>
	public readonly string Name;

	/// <summary>
	/// The Location of the Uniform in the Shader
	/// </summary>
	public readonly int Location;

	/// <summary>
	/// The Array length of the uniform
	/// </summary>
	public readonly int Length;

	/// <summary>
	/// The Type of Uniform
	/// </summary>
	public readonly UniformType Type;

	public ShaderUniform(string name, int location, int length, UniformType type)
	{
		Name = name;
		Location = location;
		Length = length;
		Type = type;
	}
}

/// <summary>
/// TODO: This isn't api-agnostic and doesn't represent the final implementation
///
/// Not sure what the best way to implement this is:
///
///     1)  Create our own Shader language that each platform can convert
///         into their required language. This has a lot of drawbacks
///         because it needs to be very easy to parse somehow ...
///
///     2)  Require either GLSL or HLSL and use a 3rd party tool to convert them.
///         The problem here is that I don't want to add more dependencies (especially
///         since most of these tools are offline / not in C#). I would like everything to
///         be cross-platform and runtime available.
///
///     3)  Don't worry about it and just make the end-user write shaders for each platform
///         they intend to use, and load them manually. (** what currently happens **)
///
/// </summary>
public class ShaderSource
{
	public byte[]? vertex;
	public byte[]? fragment;
	public byte[]? geometry;

	public ShaderSource(string vertexSource, string fragmentSource, string? geomSource = null)
	{
		if (!string.IsNullOrEmpty(vertexSource))
			vertex = Encoding.UTF8.GetBytes(vertexSource);

		if (!string.IsNullOrEmpty(fragmentSource))
			fragment = Encoding.UTF8.GetBytes(fragmentSource);

		if (!string.IsNullOrEmpty(geomSource))
			geometry = Encoding.UTF8.GetBytes(geomSource);
	}

	public ShaderSource(Stream vertexSource, Stream fragmentSource, Stream? geomSource = null)
	{
		if (vertexSource != null)
		{
			vertex = new byte[vertexSource.Length];
			vertexSource.Read(vertex, 0, vertex.Length);
		}

		if (fragmentSource != null)
		{
			fragment = new byte[fragmentSource.Length];
			fragmentSource.Read(fragment, 0, fragment.Length);
		}

		if (geomSource != null)
		{
			geometry = new byte[geomSource.Length];
			geomSource.Read(geometry, 0, geometry.Length);
		}
	}
}
