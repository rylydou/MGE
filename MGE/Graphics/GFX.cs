using System;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

#if MGE_MORE_VERTICES
using VertIndex = System.UInt32;
#else
using VertIndex = System.UInt16;
#endif

namespace MGE.Graphics;

public static class GFX
{
#if MGE_MORE_VERTICES
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedInt;
#else
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedShort;
#endif

	class BatchKey : IEquatable<BatchKey>, IComparable<BatchKey>
	{
		public Texture texture;
		public Shader shader;
		public sbyte priority;
		public PrimitiveType primitiveType = PrimitiveType.Triangles;

		public BatchKey(Texture texture, Shader shader, sbyte priority)
		{
			this.texture = texture;
			this.shader = shader;
			this.priority = priority;
		}

		public int CompareTo(BatchKey? other) => this.priority.CompareTo(other);

		public override bool Equals(object? obj) => obj is BatchKey key && Equals(key);
		public bool Equals(BatchKey? other) => other is not null && other.texture == texture && other.shader == shader && other.priority == priority;

		public override int GetHashCode() => HashCode.Combine(texture, shader, priority);
	}

	class BatchItem
	{
		public readonly BatchKey key;

		public ChunkedList<float> vertexData = new();
		public ChunkedList<VertIndex> elements = new();

		public VertIndex vertexCount;

		public BatchItem(BatchKey key)
		{
			this.key = key;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			vertexData.Clear();
			elements.Clear();

			vertexCount = 0;
		}
	}

	public static Matrix transform;

	static AutoDictionary<BatchKey, BatchItem> _batches = new(v => v.key);
#nullable disable
	static BatchItem _batch;
#nullable enable
	static VertIndex _batchIndexOffset;

	// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
	static VertexArray _vertexDataArray;

	static Buffer<float> _vertexBuffer;
	static Buffer<VertIndex> _elementBuffer;

	static Texture _pixelTexture;
	static Shader _spriteShader;

	// public Shader shader;
	// public Texture texture;
	// public sbyte priority;

	static GFX()
	{
		const VertIndex vertexCapacity = 1024 * 4 /* * Vertex.SIZE_IN_ELEMENTS */;
		const VertIndex elementCapacity = 1024 * 6;

		_vertexBuffer = new();
		_vertexBuffer.Init(BufferTarget.ArrayBuffer, (int)Math.NextPowerOf2((uint)(vertexCapacity * Vertex.SIZE_IN_ELEMENTS)), BufferUsageHint.StreamDraw);

		_vertexDataArray = new();
		_vertexDataArray.Bind();
		_vertexDataArray.BindAttribute(0, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 0 * sizeof(float), false); // Position
		_vertexDataArray.BindAttribute(1, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 2 * sizeof(float), false); // Texture Coord
		_vertexDataArray.BindAttribute(2, _vertexBuffer, 4, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 4 * sizeof(float), false); // Color

		_elementBuffer = new();
		_elementBuffer.Init(BufferTarget.ElementArrayBuffer, (int)Math.NextPowerOf2(elementCapacity), BufferUsageHint.StreamDraw);

		_spriteShader = new("Sprite.vert", "Sprite.frag");

		_pixelTexture = new Texture(new(1, 1), new[] { Color.white });
	}

	public static void Flush()
	{
		// Debug.LogVariable(_batches.Count);

		foreach (var batch in _batches)
		{
			if (batch.vertexCount == 0) continue;

			// Debug.Log($"\nVertices ({batch.vertexCount} / {batch.vertexData.Count}):");
			// Debug.Log(string.Join(' ', batch.vertexData.array.Select(x => x.ToString()).ToArray(), 0, batch.vertexData.Count));
			// Debug.Log($"Elements ({batch.elements.Count}):");
			// Debug.Log(string.Join(' ', batch.elements.array.Select(x => x.ToString()).ToArray(), 0, batch.elements.Count));

			batch.key.texture.Use();
			batch.key.shader.SetMatrix("transform", transform);

			_vertexBuffer.SubData(BufferTarget.ArrayBuffer, batch.vertexData.array, 0, batch.vertexData.Count);
			_elementBuffer.SubData(BufferTarget.ElementArrayBuffer, batch.elements.array, 0, batch.elements.Count);

			_vertexDataArray.DrawElements(batch.key.primitiveType, batch.elements.Count, ELEMENTS_TYPE);

			// _pixelTexture.Use();
			// _vertexDataArray.DrawElements(PrimitiveType.Lines, batch.elements.Count, ELEMENTS_TYPE);

			batch.Clear();
		}
	}

	#region Primitive Drawing

	public static void DrawPoint(Vector2 position, Color color)
	{
		StartVertexBatch(new(_pixelTexture, _spriteShader, 0) { primitiveType = PrimitiveType.Points });

		SetVertex(position, Vector2.zero, color);

		SetIndex(0);
	}

	// FIXME Don't be lazy and draw triangles instead of lines
	//       because the renderer dies when drawing lines
	public static void DrawLine(Vector2 start, Vector2 end, float width, Color color)
	{
		var angle = Vector2.Angle(start, end);
		var center = Vector2.Middle(start, end);
		var length = Vector2.Distance(start, end);

		SetTextureScaledAndRotated(_pixelTexture, _spriteShader, 0, center, new Vector2(width, length), angle, color);
	}

	public static void DrawSquare(Rect rect, Color color) => DrawTexture(_pixelTexture, rect, color);

	/// <summary>
	///
	/// </summary>
	/// <param name="position"></param>
	/// <param name="radius"></param>
	/// <param name="color"></param>
	/// <param name="resolutionMultiplier">Higher values = lower resolution</param>
	public static void DrawCircleFilled(Vector2 position, float radius, Color color, float resolutionMultiplier = 2f)
	{
		var vertexCount = (VertIndex)(Math.CeilToEven(radius / resolutionMultiplier));

		StartVertexBatch(new(_pixelTexture, _spriteShader, 0));

		SetVertex(position, Vector2.zero, color);

		// Draw the vertices
		for (VertIndex i = 0; i < vertexCount; i++)
		{
			var vertexPosition = new Vector2(
				position.x + (radius * Math.Sin(i * Math.pi2 / vertexCount)),
				position.y + (radius * Math.Cos(i * Math.pi2 / vertexCount))
			);

			SetVertex(vertexPosition, Vector2.zero, color);
		}

		// Connect the vertices
		for (VertIndex i = 1; i < vertexCount; i++)
		{
			SetTriangleIndices(0, i, (VertIndex)(i + 1));
		}

		SetTriangleIndices(0, vertexCount, 1);
	}

	// public static void DrawCircleOutline(Vector2 position, float radius, Color color)
	// {
	// 	const float CIRCLE_DETAIL_MUL = 3; // Use 2 for more detail
	// 	var vertexCount = (VertIndex)(radius / CIRCLE_DETAIL_MUL);

	// 	for (VertIndex i = 0; i < vertexCount; i++)
	// 	{
	// 		var vertexPosition = new Vector2(
	// 			position.x + (radius * Math.Cos(i * Math.pi2 / vertexCount)),
	// 			position.y + (radius * Math.Sin(i * Math.pi2 / vertexCount))
	// 		);

	// 		SetVertex(vertexPosition, Vector2.zero, color);
	// 		SetIndex(i);
	// 	}
	// }

	#endregion

	#region Texture Drawing

	public static void DrawTexture(Texture texture, Vector2 position) => DrawTexture(texture, position, Color.white);
	public static void DrawTexture(Texture texture, Vector2 position, Color color) => DrawTexture(texture, position, Vector2.one, color);
	public static void DrawTexture(Texture texture, Vector2 position, Vector2 scale, Color color) => SetTextureScaled(texture, _spriteShader, 0, position, scale, color);
	public static void DrawTexture(Texture texture, Vector2 position, Vector2 scale, float rotationInRadians, Color color) => SetTextureScaledAndRotated(texture, _spriteShader, 0, position, scale, rotationInRadians, color);

	public static void DrawTexture(Texture texture, Rect destination) => SetTextureUsingDest(texture, _spriteShader, 0, destination, Color.white);
	public static void DrawTexture(Texture texture, Rect destination, Color color) => SetTextureUsingDest(texture, _spriteShader, 0, destination, color);

	public static void DrawTextureRegion(Texture texture, Rect destination, RectInt source) => SetTextureRegionUsingDest(texture, _spriteShader, 0, destination, source, Color.white);
	public static void DrawTextureRegion(Texture texture, Rect destination, RectInt source, Color color) => SetTextureRegionUsingDest(texture, _spriteShader, 0, destination, source, color);

	#endregion

	#region Low Level Drawing

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetTextureScaled(Texture texture, Shader shader, sbyte priority, Vector2 position, Vector2 scale, Color color)
	{
		var halfRealSize = (Vector2)texture.size * scale / 2;

		StartVertexBatch(new(texture, shader, priority));

		SetVertex(position + new Vector2(halfRealSize.x, -halfRealSize.y), new(1, 1), color); // Top right
		SetVertex(position + halfRealSize, new(1, 0), color);                                 // Bottom right
		SetVertex(position + new Vector2(-halfRealSize.x, halfRealSize.y), new(0, 0), color); // Bottom left
		SetVertex(position - halfRealSize, new(0, 1), color);                                 // Top left

		SetTriangleIndices(0, 1, 3); // Bottom right
		SetTriangleIndices(1, 2, 3); // Top left
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetTextureScaledAndRotated(Texture texture, Shader shader, sbyte priority, Vector2 position, Vector2 scale, float rotationInRadians, Color color)
	{
		var halfRealSize = (Vector2)texture.size * scale / 2;

		StartVertexBatch(new(texture, shader, priority));

		SetVertex(position + Vector2.RotateAroundPoint(new Vector2(halfRealSize.x, -halfRealSize.y), rotationInRadians), new(1, 1), color); // Top right
		SetVertex(position + Vector2.RotateAroundPoint(+halfRealSize, rotationInRadians), new(1, 0), color);                                 // Bottom right
		SetVertex(position + Vector2.RotateAroundPoint(new Vector2(-halfRealSize.x, halfRealSize.y), rotationInRadians), new(0, 0), color); // Bottom left
		SetVertex(position + Vector2.RotateAroundPoint(-halfRealSize, rotationInRadians), new(0, 1), color);                                 // Top left

		SetTriangleIndices(0, 1, 3); // Bottom right
		SetTriangleIndices(1, 2, 3); // Top left
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetTextureUsingDest(Texture texture, Shader shader, sbyte priority, Rect destination, Color color)
	{
		StartVertexBatch(new(texture, shader, priority));

		SetVertex(destination.topRight, new(1, 1), color);    // Top right
		SetVertex(destination.bottomRight, new(1, 0), color); // Bottom right
		SetVertex(destination.bottomLeft, new(0, 0), color);  // Bottom left
		SetVertex(destination.topLeft, new(0, 1), color);     // Top left

		SetTriangleIndices(0, 1, 3); // Bottom right
		SetTriangleIndices(1, 2, 3); // Top left
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetTextureRegionUsingDest(Texture texture, Shader shader, sbyte priority, Rect destination, RectInt source, Color color)
	{
		StartVertexBatch(new(texture, shader, priority));

		SetVertex(destination.topRight, texture.GetTextureCoord(source.bottomRight), color); // Top right
		SetVertex(destination.bottomRight, texture.GetTextureCoord(source.topRight), color); // Bottom right
		SetVertex(destination.bottomLeft, texture.GetTextureCoord(source.topLeft), color);   // Bottom left
		SetVertex(destination.topLeft, texture.GetTextureCoord(source.bottomLeft), color);   // Top left

		SetTriangleIndices(0, 1, 3); // Bottom right
		SetTriangleIndices(1, 2, 3); // Top left
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void SetTextureRegionUsingVerts(Texture texture, Shader shader, sbyte priority, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, RectInt source, Color color)
	{
		StartVertexBatch(new(texture, shader, priority));

		SetVertex(topRight, texture.GetTextureCoord(source.bottomRight), color); // Top right
		SetVertex(bottomRight, texture.GetTextureCoord(source.topRight), color); // Bottom right
		SetVertex(bottomLeft, texture.GetTextureCoord(source.topLeft), color);   // Bottom left
		SetVertex(topLeft, texture.GetTextureCoord(source.bottomLeft), color);   // Top left

		SetTriangleIndices(0, 1, 3); // Bottom right
		SetTriangleIndices(1, 2, 3); // Top left
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void StartVertexBatch(BatchKey key)
	{
		if (!_batches.TryGetValue(key, out var batch))
		{
			batch = new(key);
			_batches.Add(batch);
		}
		_batch = batch;
		_batchIndexOffset = batch.vertexCount;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetVertex(Vector2 position, Vector2 textureCoordinate, Color color)
	{
		_batch.vertexCount++;

		_batch.vertexData.EnsureSpaceFor(8);

		_batch.vertexData.AddUnsafe(position.x);
		_batch.vertexData.AddUnsafe(position.y);

		_batch.vertexData.AddUnsafe(textureCoordinate.x);
		_batch.vertexData.AddUnsafe(textureCoordinate.y);

		_batch.vertexData.AddUnsafe(color.r);
		_batch.vertexData.AddUnsafe(color.g);
		_batch.vertexData.AddUnsafe(color.b);
		_batch.vertexData.AddUnsafe(color.a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetVertex(Vertex vertex)
	{
		_batch.vertexCount++;

		_batch.vertexData.EnsureSpaceFor(Vertex.SIZE_IN_ELEMENTS);

		_batch.vertexData.AddUnsafe(vertex.position.x);
		_batch.vertexData.AddUnsafe(vertex.position.y);

		_batch.vertexData.AddUnsafe(vertex.textureCoordinate.x);
		_batch.vertexData.AddUnsafe(vertex.textureCoordinate.y);

		_batch.vertexData.AddUnsafe(vertex.color.r);
		_batch.vertexData.AddUnsafe(vertex.color.g);
		_batch.vertexData.AddUnsafe(vertex.color.b);
		_batch.vertexData.AddUnsafe(vertex.color.a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetVertices(params Vertex[] vertices)
	{
		_batch.vertexCount += (VertIndex)vertices.Length;

		_batch.vertexData.EnsureSpaceFor(vertices.Length * Vertex.SIZE_IN_ELEMENTS);

		foreach (var vertex in vertices)
		{
			_batch.vertexData.AddUnsafe(vertex.position.x);
			_batch.vertexData.AddUnsafe(vertex.position.y);

			_batch.vertexData.AddUnsafe(vertex.textureCoordinate.x);
			_batch.vertexData.AddUnsafe(vertex.textureCoordinate.y);

			_batch.vertexData.AddUnsafe(vertex.color.r);
			_batch.vertexData.AddUnsafe(vertex.color.g);
			_batch.vertexData.AddUnsafe(vertex.color.b);
			_batch.vertexData.AddUnsafe(vertex.color.a);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetVertexData(float[] vertexData)
	{
		_batch.vertexCount += (VertIndex)(vertexData.Length / Vertex.SIZE_IN_ELEMENTS);

		_batch.vertexData.Add(vertexData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetIndex(VertIndex index)
	{
		_batch.elements.Add((VertIndex)(_batchIndexOffset + index));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetTriangleIndices(VertIndex index1, VertIndex index2, VertIndex index3)
	{
		_batch.elements.EnsureSpaceFor(3);

		_batch.elements.AddUnsafe((VertIndex)(_batchIndexOffset + index1));
		_batch.elements.AddUnsafe((VertIndex)(_batchIndexOffset + index2));
		_batch.elements.AddUnsafe((VertIndex)(_batchIndexOffset + index3));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetIndices(params VertIndex[] indices)
	{
		_batch.elements.EnsureSpaceFor(indices.Length);

		foreach (var index in indices)
		{
			_batch.elements.AddUnsafe((VertIndex)(_batchIndexOffset + index));
		}
	}

	#endregion

	#region Utilities

	internal static void CheckGLError()
	{
		var error = GL.GetError();

		if (error != ErrorCode.NoError)
		{
			throw new Exception($"Open GL Error: {((int)error)} - {error.ToString()}");
		}
	}

	#endregion
}
