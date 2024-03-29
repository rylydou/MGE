using System;
using System.Buffers;

namespace MGE;

public class Mesh : IDisposable
{
	public abstract class Platform
	{
		protected internal abstract void UploadVertices<T>(ReadOnlySequence<T> vertices, VertexFormat format);
		protected internal abstract void UploadInstances<T>(ReadOnlySequence<T> instances, VertexFormat format);
		protected internal abstract void UploadIndices<T>(ReadOnlySequence<T> indices);
		protected internal abstract void Dispose();
	}

	/// <summary>
	/// A reference to the internal platform implementation of the Mesh
	/// </summary>
	public readonly Platform implementation;

	/// <summary>
	/// Number of Vertices in the Mesh
	/// </summary>
	public uint vertexCount { get; set; }

	/// <summary>
	/// Number of Indices in the Mesh
	/// </summary>
	public uint indexCount { get; set; }

	/// <summary>
	/// Number of Instances in the Mesh
	/// </summary>
	public uint instanceCount { get; set; }

	/// <summary>
	/// The Number of Triangle Elements in the Mesh (IndicesCount / 3)
	/// </summary>
	public uint elementCount => indexCount / 3;

	/// <summary>
	/// Gets the Vertex Format, or null if never set
	/// </summary>
	public VertexFormat? vertexFormat { get; set; } = null;

	/// <summary>
	/// Gets the Instance Format, or null if never set
	/// </summary>
	public VertexFormat? instanceFormat { get; set; } = null;

	public Mesh()
	{
		implementation = App.graphics.CreateMesh();
	}

	public Mesh(Graphics graphics)
	{
		implementation = graphics.CreateMesh();
	}

	public void SetVertices<T>(T[] vertices) where T : struct, IVertex
	{
		SetVertices(new ReadOnlySequence<T>(vertices), default(T).format);
	}

	public void SetVertices<T>(ReadOnlyMemory<T> vertices) where T : struct, IVertex
	{
		SetVertices(new ReadOnlySequence<T>(vertices), default(T).format);
	}

	public void SetVertices<T>(ReadOnlySequence<T> vertices) where T : struct, IVertex
	{
		SetVertices(vertices, default(T).format);
	}

	public void SetVertices<T>(ReadOnlyMemory<T> vertices, VertexFormat format)
	{
		SetVertices(new ReadOnlySequence<T>(vertices), format);
	}

	public void SetVertices<T>(ReadOnlySequence<T> vertices, VertexFormat format)
	{
		vertexCount = (uint)vertices.Length;
		vertexFormat = format ?? throw new MGException("Vertex Format cannot be null");

		implementation.UploadVertices(vertices, vertexFormat);
	}

	public void SetIndices<T>(T[] indices)
	{
		SetIndices(new ReadOnlySequence<T>(indices));
	}

	public void SetIndices<T>(ReadOnlyMemory<T> indices)
	{
		SetIndices(new ReadOnlySequence<T>(indices));
	}

	public void SetIndices<T>(ReadOnlySequence<T> indices)
	{
		indexCount = (uint)indices.Length;
		implementation.UploadIndices<T>(indices);
	}

	public void SetInstances<T>(T[] vertices) where T : struct, IVertex
	{
		SetInstances(new ReadOnlySequence<T>(vertices), default(T).format);
	}

	public void SetInstances<T>(ReadOnlyMemory<T> vertices) where T : struct, IVertex
	{
		SetInstances(new ReadOnlySequence<T>(vertices), default(T).format);
	}

	public void SetInstances<T>(ReadOnlyMemory<T> vertices, VertexFormat format)
	{
		SetInstances(new ReadOnlySequence<T>(vertices), format);
	}

	public void SetInstances<T>(ReadOnlySequence<T> vertices) where T : struct, IVertex
	{
		SetInstances(vertices, default(T).format);
	}

	public void SetInstances<T>(ReadOnlySequence<T> vertices, VertexFormat format)
	{
		instanceCount = (uint)vertices.Length;
		instanceFormat = format ?? throw new MGException("Vertex Format cannot be null");

		implementation.UploadInstances(vertices, instanceFormat);
	}

	public void Dispose()
	{
		implementation.Dispose();
	}
}
