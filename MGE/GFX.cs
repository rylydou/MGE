using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;
using MGE.Graphics;
using System.Collections.Generic;

#if MGE_MORE_VERTICES
using VertIndex = System.UInt32;
#else
using VertexIndex = System.UInt16;
#endif

namespace MGE;

public static class GFX
{
#if MGE_MORE_VERTICES
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedInt;
#else
	const DrawElementsType ELEMENTS_TYPE = DrawElementsType.UnsignedShort;
#endif

	class BatchItem
	{
		public readonly BatchKey key;

		public ChunkedList<float> vertexData = new();
		public ChunkedList<VertexIndex> indices = new();

		public VertexIndex vertexCount;

		public BatchItem(BatchKey key)
		{
			this.key = key;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			vertexData.Clear();
			indices.Clear();

			vertexCount = 0;
		}
	}

	static AutoDictionary<BatchKey, BatchItem> _batches = new(v => v.key);
#nullable disable
	static BatchItem _batch;
#nullable restore
	static VertexIndex _batchIndexOffset;

	public static Stack<Matrix> transformStack = new();
	public static Matrix transform;

#nullable disable
	public static Texture texture;
	public static Shader shader;
#nullable restore
	public static float priority;

	internal static Matrix windowViewportTransform;
	static Matrix currentProjectionTransform;
	static Shader _basicSpriteShader;

	// Vertex layout: Position X, Position Y, Texture Coordinate X, Texture Coordinate Y, Color R, Color G, Color B, Color A
	static VertexArray _vertexDataArray;

	static Buffer<float> _vertexBuffer;
	static Buffer<VertexIndex> _indexBuffer;

	static GFX()
	{
		const VertexIndex vertexCapacity = 1024 * 4 /* * Vertex.SIZE_IN_ELEMENTS */;
		const VertexIndex indexCapacity = 1024 * 6;

		_vertexBuffer = new();
		_vertexBuffer.Init(BufferTarget.ArrayBuffer, (int)Math.CeilToPowerOf2((uint)(vertexCapacity * Vertex.SIZE_IN_ELEMENTS)), BufferUsageHint.StreamDraw);

		_vertexDataArray = new();
		_vertexDataArray.Bind();
		_vertexDataArray.BindAttribute(0, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 0 * sizeof(float), false); // Position
		_vertexDataArray.BindAttribute(1, _vertexBuffer, 2, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 2 * sizeof(float), false); // Texture Coord
		_vertexDataArray.BindAttribute(2, _vertexBuffer, 4, VertexAttribPointerType.Float, Vertex.SIZE_IN_BYTES, 4 * sizeof(float), false); // Color

		_indexBuffer = new();
		_indexBuffer.Init(BufferTarget.ElementArrayBuffer, (int)Math.CeilToPowerOf2(indexCapacity), BufferUsageHint.StreamDraw);

		_basicSpriteShader = new(Folder.assets.GetFile("Sprite.vert"), Folder.assets.GetFile("Sprite.frag"));

		PushTransform(Matrix.identity);
	}

	public static void Clear(Color color)
	{
		GL.ClearColor(color);
		GL.Clear(ClearBufferMask.ColorBufferBit);
	}

	public static void PushTransform(Matrix transform)
	{
		transformStack.Push(GFX.transform);
		GFX.transform = transform;
	}

	public static void PopTransform()
	{
		transform = transformStack.Pop();
	}

	public static void SetRenderTarget(RenderTexture? renderTexture)
	{
		var size = Vector2Int.zero;

		if (renderTexture is null)
		{
			size = GameWindow.current.Size;
			currentProjectionTransform = windowViewportTransform;
			RenderTexture.SetScreenAsTarget();
		}
		else
		{
			size = renderTexture.size;
			currentProjectionTransform = renderTexture.viewportTransform;
			renderTexture.SetAsTarget();
		}

		GL.Viewport(0, 0, size.x, size.y);
	}

	public static void DrawBatches()
	{
		// Debug.LogVariable(_batches.Count);

		foreach (var batch in _batches)
		{
			if (batch.vertexCount == 0) continue;

			// Debug.Log($"\nVertices ({batch.vertexCount} / {batch.vertexData.Count}):");
			// Debug.Log(string.Join(' ', batch.vertexData.array.Select(x => x.ToString()).ToArray(), 0, batch.vertexData.Count));
			// Debug.Log($"Elements ({batch.elements.Count}):");
			// Debug.Log(string.Join(' ', batch.elements.array.Select(x => x.ToString()).ToArray(), 0, batch.elements.Count));

			batch.key.texture.Bind();
			batch.key.shader.SetMatrix("transform", transform * currentProjectionTransform);

			_vertexBuffer.SubData(BufferTarget.ArrayBuffer, batch.vertexData.array, 0, batch.vertexData.Count);
			_indexBuffer.SubData(BufferTarget.ElementArrayBuffer, batch.indices.array, 0, batch.indices.Count);

			_vertexDataArray.DrawElements(PrimitiveType.Triangles, batch.indices.Count, ELEMENTS_TYPE);

			batch.Clear();
		}
	}

	#region Primitive Drawing

	public static void DrawPoint(Vector2 position, Color color, float radius = 1) => DrawBox(new(position - radius, radius * 2), color);

	public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1)
	{
		var angle = Vector2.AngleTo(start, end);
		var center = Vector2.Midpoint(start, end);
		var length = Vector2.Distance(start, end);

		DrawTextureScaledAndRotated(Texture.pixelTexture, center, new Vector2(length, thickness), angle, color);
	}

	public static void DrawBox(Vector2 position, Vector2 scale, Color color) => DrawTextureScaled(Texture.pixelTexture, position, scale, color);
	public static void DrawBox(Vector2 position, Vector2 scale, float rotation, Color color) => DrawTextureScaledAndRotated(Texture.pixelTexture, position, scale, rotation, color);
	public static void DrawBox(Rect rect, Color color) => DrawTextureAtDest(Texture.pixelTexture, rect, color);

	public static void DrawRect(Rect rect, Color color, float thickness = 1)
	{
		var outerRect = rect;
		outerRect.Expand(thickness * 2);

		SetBatch(Texture.pixelTexture);

		// The rectangle
		// 4      5
		//   0  1
		//   2  3
		// 6      7

		SetSpaceForVertices(8);

		// Inner
		SetVertex(rect.topLeft, color);     // 0
		SetVertex(rect.topRight, color);    // 1
		SetVertex(rect.bottomLeft, color);  // 2
		SetVertex(rect.bottomRight, color); // 3

		// Outer
		SetVertex(outerRect.topLeft, color);     // 4
		SetVertex(outerRect.topRight, color);    // 5
		SetVertex(outerRect.bottomLeft, color);  // 6
		SetVertex(outerRect.bottomRight, color); // 7

		SetSpaceForIndices(24);

		SetIndices(0, 4, 1);
		SetIndices(1, 5, 4);

		SetIndices(1, 5, 3);
		SetIndices(3, 7, 5);

		SetIndices(3, 7, 2);
		SetIndices(2, 6, 7);

		SetIndices(2, 6, 0);
		SetIndices(0, 4, 6);
	}

	public static void DrawCircle(Vector2 center, float radius, Color color, float thickness = 1, float pixelsPerLine = 8f)
	{
		var vertexCount = (int)Math.CeilToEven(radius * Math.tau / pixelsPerLine);

		var points = new Vector2[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			points[i] = new(
				center.x + (radius * Math.Sin(i * Math.tau / vertexCount)),
				center.y + (radius * Math.Cos(i * Math.tau / vertexCount))
			);
		}

		DrawPolyline(points, color, thickness);
	}

	public static void DrawCircleFilled(Vector2 center, float radius, Color color, float pixelsPerLine = 8f)
	{
		var vertexCount = (VertexIndex)Math.CeilToEven(radius * Math.tau / pixelsPerLine);

		SetBatch(Texture.pixelTexture);

		SetSpaceForVertices(vertexCount + 1);

		SetVertex(center, Vector2.zero, color);

		// Draw the vertices
		for (VertexIndex i = 0; i < vertexCount; i++)
		{
			var vertexPosition = new Vector2(
				center.x + (radius * Math.Sin(i * Math.tau / vertexCount)),
				center.y + (radius * Math.Cos(i * Math.tau / vertexCount))
			);

			SetVertex(vertexPosition, Vector2.zero, color);
		}

		SetSpaceForIndices(vertexCount);

		// Connect the vertices
		for (VertexIndex i = 1; i < vertexCount; i++)
		{
			SetIndices(0, i, (VertexIndex)(i + 1));
		}

		SetIndices(0, vertexCount, 1);
	}

	// TODO Optimise
	public static void DrawPolyline(Vector2[] points, Color color, float thickness = 1)
	{
		if (points.Length < 2) return;
		for (int i = 1; i < points.Length; i++)
			DrawLine(points[i - 1], points[i], color, thickness);
		DrawLine(points[points.Length - 1], points[0], color, thickness);
	}

	#endregion

	#region Drawing

	#region Special

	public static void DrawRenderTexture(RenderTexture renderTexture)
	{
		var size = (Vector2Int)GameWindow.current.Size;
		SetBatch(renderTexture.colorTexture);
		var scaleFactor = (float)size.y / renderTexture.size.y;
		var realXSize = renderTexture.size.x * scaleFactor;
		SetBoxRegionAtDest(new(0, renderTexture.size.y, renderTexture.size.x, -renderTexture.size.y), new(-realXSize / 2, -size.y / 2, realXSize, size.y), Color.white);
	}

	#endregion

	public static void DrawTexture(Texture texture, Vector2 position, Color color)
	{
		SetBatch(texture);
		SetBox(position, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBox(Vector2 position, Color color)
	{
		var halfRealSize = (Vector2)texture.size / 2;

		SetQuadWithNormTexCoords(
			position - halfRealSize, new(0, 0), color,
			position + new Vector2(halfRealSize.x, -halfRealSize.y), new(1, 0), color,
			position + new Vector2(-halfRealSize.x, halfRealSize.y), new(0, 1), color,
			position + halfRealSize, new(1, 1), color
		);
	}

	public static void DrawTextureRegion(Texture texture, Vector2 position, RectInt source, Color color)
	{
		SetBatch(texture);
		SetBoxRegion(position, source, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxRegion(Vector2 position, RectInt source, Color color)
	{
		var halfRealSize = (Vector2)texture.size / 2;

		SetQuadWithNormTexCoords(
			position - halfRealSize, source.topLeft, color,
			position + new Vector2(halfRealSize.x, -halfRealSize.y), source.topRight, color,
			position + new Vector2(-halfRealSize.x, halfRealSize.y), source.bottomLeft, color,
			position + halfRealSize, source.bottomRight, color
		);
	}

	public static void DrawTextureScaled(Texture texture, Vector2 position, Vector2 scale, Color color)
	{
		SetBatch(texture);
		SetBoxScaled(position, scale, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxScaled(Vector2 position, Vector2 scale, Color color)
	{
		var halfRealSize = (Vector2)texture.size * scale / 2;

		SetQuadWithNormTexCoords(
			position - halfRealSize, new(0, 0), color,
			position + new Vector2(halfRealSize.x, -halfRealSize.y), new(1, 0), color,
			position + new Vector2(-halfRealSize.x, halfRealSize.y), new(0, 1), color,
			position + halfRealSize, new(1, 1), color
		);
	}

	public static void DrawTextureRegionScaled(Rect source, Texture texture, Vector2 position, Vector2 scale, Color color)
	{
		SetBatch(texture);
		SetBoxRegionScaled(source, position, scale, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxRegionScaled(Rect source, Vector2 position, Vector2 scale, Color color)
	{
		var halfRealSize = (Vector2)texture.size * scale / 2;

		SetQuadWithNormTexCoords(
			position - halfRealSize, source.topLeft, color,
			position + new Vector2(halfRealSize.x, -halfRealSize.y), source.topRight, color,
			position + new Vector2(-halfRealSize.x, halfRealSize.y), source.bottomLeft, color,
			position + halfRealSize, source.bottomRight, color
		);
	}

	public static void DrawTextureScaledAndRotated(Texture texture, Vector2 position, Vector2 scale, float rotationRad, Color color)
	{
		SetBatch(texture);
		SetBoxScaledAndRotated(position, scale, rotationRad, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxScaledAndRotated(Vector2 position, Vector2 scale, float rotationRad, Color color)
	{
		var halfRealSize = (Vector2)texture.size * scale / 2;

		SetQuadWithNormTexCoords(
			position + Vector2.RotateAroundPoint(-halfRealSize, rotationRad), new(0, 0), color,
			position + Vector2.RotateAroundPoint(new Vector2(halfRealSize.x, -halfRealSize.y), rotationRad), new(1, 0), color,
			position + Vector2.RotateAroundPoint(new Vector2(-halfRealSize.x, halfRealSize.y), rotationRad), new(0, 1), color,
			position + Vector2.RotateAroundPoint(halfRealSize, rotationRad), new(1, 1), color
		);
	}

	public static void DrawTextureScaledAndRotated(Texture texture, Vector2 position, Vector2 scale, float rotationRad, Rect source, Color color)
	{
		SetBatch(texture);
		SetBoxRegionScaledAndRotated(position, scale, rotationRad, source, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxRegionScaledAndRotated(Vector2 position, Vector2 scale, float rotationRad, RectInt source, Color color)
	{
		var halfRealSize = (Vector2)texture.size * scale / 2;

		SetQuad(
			position + Vector2.RotateAroundPoint(-halfRealSize, rotationRad), source.topLeft, color,
			position + Vector2.RotateAroundPoint(new Vector2(halfRealSize.x, -halfRealSize.y), rotationRad), source.topRight, color,
			position + Vector2.RotateAroundPoint(new Vector2(-halfRealSize.x, halfRealSize.y), rotationRad), source.bottomLeft, color,
			position + Vector2.RotateAroundPoint(halfRealSize, rotationRad), source.bottomRight, color
		);
	}

	public static void DrawTextureAtDest(Texture texture, Rect destination, Color color)
	{
		SetBatch(texture);
		SetBoxAtDest(destination, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxAtDest(Rect destination, Color color)
	{
		SetQuadWithNormTexCoords(
			destination.topLeft, new(0, 0), color,
			destination.topRight, new(1, 0), color,
			destination.bottomLeft, new(0, 1), color,
			destination.bottomRight, new(1, 1), color
		);
	}

	public static void DrawTextureRegionAtDest(Texture texture, RectInt source, Rect destination, Color color)
	{
		SetBatch(texture);
		SetBoxRegionAtDest(source, destination, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxRegionAtDest(RectInt source, Rect destination, Color color)
	{
		SetBoxRegionAtVerts(source, destination.topLeft, destination.topRight, destination.bottomLeft, destination.bottomRight, color);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetBoxRegionAtVerts(RectInt source, Vector2 destTL, Vector2 destTR, Vector2 destBL, Vector2 destBR, Color color)
	{
		SetQuad(
			destTL, source.topLeft, color,
			destTR, source.topRight, color,
			destBL, source.bottomLeft, color,
			destBR, source.bottomRight, color
		);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetQuad(
		Vector2 destTL, Vector2 srcTL, Color colorTL,
		Vector2 destTR, Vector2 srcTR, Color colorTR,
		Vector2 destBL, Vector2 srcBL, Color colorBL,
		Vector2 destBR, Vector2 srcBR, Color colorBR
	)
	{
		SetQuadWithNormTexCoords(
		 destTL, texture.GetNormalizedPoint(srcTL), colorTL,
		 destTR, texture.GetNormalizedPoint(srcTR), colorTR,
		 destBL, texture.GetNormalizedPoint(srcBL), colorBL,
		 destBR, texture.GetNormalizedPoint(srcBR), colorBR
	 );
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetQuadWithNormTexCoords(
		Vector2 destTL, Vector2 srcTL, Color colorTL,
		Vector2 destTR, Vector2 srcTR, Color colorTR,
		Vector2 destBL, Vector2 srcBL, Color colorBL,
		Vector2 destBR, Vector2 srcBR, Color colorBR
	)
	{
		SetSpaceForVertices(4);
		SetVertex(destTR, srcBR, colorTR); // Top right
		SetVertex(destBR, srcTR, colorBR); // Bottom right
		SetVertex(destBL, srcTL, colorBL); // Bottom left
		SetVertex(destTL, srcBL, colorTL); // Top left

		SetSpaceForIndices(6);
		SetIndices(0, 1, 3); // Bottom right
		SetIndices(1, 2, 3); // Top left
	}

	#endregion Drawing

	#region Item Setting

	#region Batching

	public static void SetBatch() => StartBatch();
	public static void SetBatch(Texture texture)
	{
		GFX.texture = texture;
		StartBatch();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void StartBatch()
	{
		var key = new BatchKey(texture ?? Texture.pixelTexture, shader ?? _basicSpriteShader) { priority = priority };
		if (!_batches.TryGetValue(key, out var batch))
		{
			batch = new(key);
			_batches.Add(batch);
		}
		_batch = batch;
		_batchIndexOffset = batch.vertexCount;
	}

	#endregion

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetSpaceForVertices(int amountOfVertices)
	{
		_batch.vertexCount += (VertexIndex)amountOfVertices;
		_batch.vertexData.EnsureSpaceFor(amountOfVertices * Vertex.SIZE_IN_ELEMENTS);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetSpaceForVertices(VertexIndex amountOfVertices)
	{
		_batch.vertexCount += amountOfVertices;
		_batch.vertexData.EnsureSpaceFor(amountOfVertices * Vertex.SIZE_IN_ELEMENTS);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetVertex(Vector2 position, Color color)
	{
		_batch.vertexData.AddUnsafe(position.x);
		_batch.vertexData.AddUnsafe(position.y);

		_batch.vertexData.AddUnsafe(0f);
		_batch.vertexData.AddUnsafe(0f);

		_batch.vertexData.AddUnsafe(color.r);
		_batch.vertexData.AddUnsafe(color.g);
		_batch.vertexData.AddUnsafe(color.b);
		_batch.vertexData.AddUnsafe(color.a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetVertex(Vector2 position, Vector2 textureCoordinate, Color color)
	{
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
	public static void SetVertex(Vertex vertex) => SetVertex(vertex.position, vertex.textureCoordinate, vertex.color);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetVertices(IEnumerable<Vertex> vertices)
	{
		foreach (var vertex in vertices)
			SetVertex(vertex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetSpaceForIndices(int amountOfIndices) => _batch.indices.EnsureSpaceFor(amountOfIndices);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetIndex(VertexIndex index) => _batch.indices.AddUnsafe((VertexIndex)(_batchIndexOffset + index));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetIndices(VertexIndex index1, VertexIndex index2, VertexIndex index3)
	{
		SetIndex(index1);
		SetIndex(index2);
		SetIndex(index3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetIndices(params VertexIndex[] indices)
	{
		foreach (var index in indices) SetIndex(index);
	}

	#endregion Item Setting

	#region Internal Utilities

	internal static void CheckError()
	{
		var error = GL.GetError();

		if (error != ErrorCode.NoError)
		{
			throw new MGEException($"Open GL Error: {((int)error)} - {error.ToString()}");
		}
	}

	#endregion Internal Utilities
}
