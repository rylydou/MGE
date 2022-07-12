using System;

namespace MGE;

public enum VertexAttrib
{
	Position,
	Normal,
	Bitangent,
	Color0,
	Color1,
	Color2,
	Color3,
	Indices,
	Weight,
	TexCoord0,
	TexCoord1,
	TexCoord2,
	TexCoord3,
	TexCoord4,
	TexCoord5,
	TexCoord6,
	TexCoord7
}

public enum VertexComponents
{
	One = 1,
	Two = 2,
	Three = 3,
	Four = 4
}

/// <summary>
/// Represents the type of Data in a Vertex
/// </summary>
public enum VertexType
{
	Byte,
	Short,
	Int,
	Float
}

/// <summary>
/// A Vertex struct to be passed to a Shader
/// </summary>
public interface IVertex
{
	/// <summary>
	/// Gets the Format of the Vertex
	/// This should return a static value, not create a new format every time it's accessed
	/// </summary>
	VertexFormat format { get; }
}

/// <summary>
/// Describes a Vertex Attribute for a Shader
/// </summary>
public struct VertexAttribute
{
	/// <summary>
	/// The name of the Attribute
	/// Depending on the Graphics Implementation, this may or may not be respected
	/// </summary>
	public readonly string name;

	/// <summary>
	/// The Vertex Attribute Type
	/// Depending on the Graphics Implementation, this may or may not be respected
	/// </summary>
	public readonly VertexAttrib attrib;

	/// <summary>
	/// The Vertex Type of the Attribute
	/// </summary>
	public readonly VertexType type;

	/// <summary>
	/// The number of Components
	/// </summary>
	public readonly VertexComponents components;

	/// <summary>
	/// The size of a single Component, in bytes
	/// </summary>
	public readonly int componentSize;

	/// <summary>
	/// The size of the entire Attribute, in bytes
	/// </summary>
	public readonly int attributeSize;

	/// <summary>
	/// Whether the Attribute value should be normalized
	/// </summary>
	public readonly bool normalized;

	public VertexAttribute(string name, VertexAttrib attrib, VertexType type, VertexComponents components, bool normalized = false)
	{
		this.name = name;
		this.attrib = attrib;
		this.type = type;
		this.components = components;
		this.normalized = normalized;

		componentSize = type switch
		{
			VertexType.Byte => 1,
			VertexType.Short => 2,
			VertexType.Int => 4,
			VertexType.Float => 4,
			_ => throw new NotImplementedException(),
		};

		attributeSize = (int)this.components * componentSize;
	}
}

/// <summary>
/// Describes a Vertex Format for a Shader
/// This tells the Shader what Attributes, and in what order, are to be expected
/// </summary>
public class VertexFormat
{
	/// <summary>
	/// The list of Attributes
	/// </summary>
	public readonly VertexAttribute[] attributes;

	/// <summary>
	/// The stride of each Vertex (all the Attributes combined)
	/// </summary>
	public readonly int stride;

	public VertexFormat(params VertexAttribute[] attributes)
	{
		this.attributes = attributes;

		stride = 0;
		for (int i = 0; i < this.attributes.Length; i++)
			stride += this.attributes[i].attributeSize;
	}

	/// <summary>
	/// Attempts to find an attribute by name, and returns its relative pointer (offset)
	/// </summary>
	public bool TryGetAttribute(string name, out VertexAttribute element, out int pointer)
	{
		pointer = 0;
		for (int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].name == name)
			{
				element = attributes[i];
				return true;
			}
			pointer += attributes[i].attributeSize;
		}

		element = default;
		return false;
	}
}
