using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MGE;

/// <summary>
/// A 2D Sprite Batcher used for drawing images, text, and shapes
/// </summary>
public class Batch2D
{
	public static Batch2D current = new Batch2D();

	public static readonly VertexFormat vertexFormat = new VertexFormat(
		new VertexAttribute("a_position", VertexAttrib.Position, VertexType.Float, VertexComponents.Two, false),
		new VertexAttribute("a_tex", VertexAttrib.TexCoord0, VertexType.Float, VertexComponents.Two, false),
		new VertexAttribute("a_color", VertexAttrib.Color0, VertexType.Byte, VertexComponents.Four, true),
		new VertexAttribute("a_type", VertexAttrib.TexCoord1, VertexType.Byte, VertexComponents.Three, true));

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vertex : IVertex
	{
		public Vector2 pos;
		public Vector2 tex;
		public Color col;
		public byte mult;
		public byte wash;
		public byte fill;

		public Vertex(Vector2 position, Vector2 texcoord, in Color color, int mult, int wash, int fill)
		{
			pos = position;
			tex = texcoord;
			col = color;
			this.mult = (byte)mult;
			this.wash = (byte)wash;
			this.fill = (byte)fill;
		}

		public VertexFormat format => vertexFormat;

		public override string ToString()
		{
			return $"{{pos:{pos}, tex:{tex}, col:{col}, mult:{mult}, wash:{wash}, fill:{fill}}}";
		}
	}

	static Shader? defaultBatchShader;

	public readonly Graphics graphics;
	public readonly Shader defaultShader;
	public readonly Material defaultMaterial;
	public readonly Mesh mesh;

	public Transform2D matrixStack = Transform2D.identity;
	public RectInt? scissor => _currentBatch.scissor;

	public string textureUniformName = "u_texture";
	public string matrixUniformName = "u_matrix";

	readonly Stack<Transform2D> _matrixStack = new Stack<Transform2D>();
	Vertex[] _vertices;
	int[] _indices;
	RenderPass _pass;
	readonly List<Batch> _batches;
	Batch _currentBatch;
	int _currentBatchInsert;
	bool _dirty;
	int _vertexCount;
	int _indexCount;

	public int triangleCount => _indexCount / 3;
	public int vertexCount => _vertexCount;
	public int indexCount => _indexCount;
	public int batchCount => _batches.Count + (_currentBatch.elements > 0 ? 1 : 0);

	struct Batch
	{
		public int layer;
		public Material? material;
		public BlendMode blendMode;
		public Transform2D matrix;
		public Texture? texture;
		public RectInt? scissor;
		public uint offset;
		public uint elements;

		public Batch(Material? material, BlendMode blend, Texture? texture, Transform2D matrix, uint offset, uint elements)
		{
			layer = 0;
			this.material = material;
			blendMode = blend;
			this.texture = texture;
			this.matrix = matrix;
			scissor = null;
			this.offset = offset;
			this.elements = elements;
		}
	}

	public Batch2D() : this(App.graphics)
	{

	}

	public Batch2D(Graphics graphics)
	{
		this.graphics = graphics;

		if (defaultBatchShader is null)
			defaultBatchShader = new Shader(graphics, graphics.CreateShaderSourceBatch2D());

		defaultShader = defaultBatchShader;
		defaultMaterial = new Material(defaultShader);

		mesh = new Mesh(graphics);

		_vertices = new Vertex[64];
		_indices = new int[64];
		_batches = new List<Batch>();

		Clear();
	}

	public void Clear()
	{
		_vertexCount = 0;
		_indexCount = 0;
		_currentBatchInsert = 0;
		_currentBatch = new Batch(null, BlendMode.Normal, null, Transform2D.identity, 0, 0);
		_batches.Clear();
		_matrixStack.Clear();
		matrixStack = Transform2D.identity;
	}

	#region Rendering

	public Batch2DRenderInfo Render(RenderTarget target)
	{
		var matrix = Matrix4x4.CreateOrthographicOffCenter(0, target.renderWidth, target.renderHeight, 0, 0, float.MaxValue);
		return Render(target, matrix);
	}

	public Batch2DRenderInfo Render(RenderTarget target, Color clearColor)
	{
		App.graphics.Clear(target, clearColor);
		return Render(target);
	}

	public Batch2DRenderInfo Render(RenderTarget target, Matrix4x4 matrix, RectInt? viewport = null, Color? clearColor = null)
	{
		var info = new Batch2DRenderInfo(triangleCount, vertexCount, indexCount, batchCount);

		var sw = new Stopwatch();
		sw.Start();

		if (clearColor is not null)
			App.graphics.Clear(target, clearColor.Value);

		_pass = new RenderPass(target, mesh, defaultMaterial);
		_pass.viewport = viewport;

		Debug.Assert(_matrixStack.Count <= 0, "Batch.MatrixStack Pushes more than it Pops");

		if (_batches.Count > 0 || _currentBatch.elements > 0)
		{
			if (_dirty)
			{
				mesh.SetVertices(new ReadOnlyMemory<Vertex>(_vertices, 0, _vertexCount));
				mesh.SetIndices(new ReadOnlyMemory<int>(_indices, 0, _indexCount));

				_dirty = false;
			}

			// render batches
			for (int i = 0; i < _batches.Count; i++)
			{
				// remaining elements in the current batch
				if (_currentBatchInsert == i && _currentBatch.elements > 0)
					RenderBatch(_currentBatch, matrix);

				// render the batch
				RenderBatch(_batches[i], matrix);
			}

			// remaining elements in the current batch
			if (_currentBatchInsert == _batches.Count && _currentBatch.elements > 0)
				RenderBatch(_currentBatch, matrix);
		}

		// Implied clear
		Clear();

		sw.Stop();
		info.time = sw.Elapsed;

		return info;
	}

	void RenderBatch(in Batch batch, in Matrix4x4 matrix)
	{
		_pass.scissor = batch.scissor;
		_pass.blendMode = batch.blendMode;

		// Render the Mesh
		// Note we apply the texture and matrix based on the current batch
		// If the user set these on the Material themselves, they will be overwritten here

		_pass.material = batch.material ?? defaultMaterial;
		_pass.material[textureUniformName]?.SetTexture(batch.texture);
		_pass.material[matrixUniformName]?.SetMatrix4x4((Matrix4x4)batch.matrix * matrix);

		_pass.meshIndexStart = batch.offset * 3;
		_pass.meshIndexCount = batch.elements * 3;
		_pass.meshInstanceCount = 0;

		graphics.Render(ref _pass);
	}

	#endregion

	#region Modify State

	public void SetMaterial(Material? material)
	{
		if (_currentBatch.elements == 0)
		{
			_currentBatch.material = material;
		}
		else if (_currentBatch.material != material)
		{
			_batches.Insert(_currentBatchInsert, _currentBatch);

			_currentBatch.material = material;
			_currentBatch.offset += _currentBatch.elements;
			_currentBatch.elements = 0;
			_currentBatchInsert++;
		}
	}

	public void SetBlendMode(in BlendMode blendmode)
	{
		if (_currentBatch.elements == 0)
		{
			_currentBatch.blendMode = blendmode;
		}
		else if (_currentBatch.blendMode != blendmode)
		{
			_batches.Insert(_currentBatchInsert, _currentBatch);

			_currentBatch.blendMode = blendmode;
			_currentBatch.offset += _currentBatch.elements;
			_currentBatch.elements = 0;
			_currentBatchInsert++;
		}
	}

	public BlendMode GetBlendMode()
	{
		return _currentBatch.blendMode;
	}

	public void SetMatrix(in Transform2D matrix)
	{
		if (_currentBatch.elements == 0)
		{
			_currentBatch.matrix = matrix;
		}
		else if (_currentBatch.matrix != matrix)
		{
			_batches.Insert(_currentBatchInsert, _currentBatch);

			_currentBatch.matrix = matrix;
			_currentBatch.offset += _currentBatch.elements;
			_currentBatch.elements = 0;
			_currentBatchInsert++;
		}
	}

	public void SetScissor(RectInt? scissor)
	{
		if (_currentBatch.elements == 0)
		{
			_currentBatch.scissor = scissor;
		}
		else if (_currentBatch.scissor != scissor)
		{
			_batches.Insert(_currentBatchInsert, _currentBatch);

			_currentBatch.scissor = scissor;
			_currentBatch.offset += _currentBatch.elements;
			_currentBatch.elements = 0;
			_currentBatchInsert++;
		}
	}

	public void SetTexture(Texture? texture)
	{
		if (_currentBatch.texture is null || _currentBatch.elements == 0)
		{
			_currentBatch.texture = texture;
		}
		else if (_currentBatch.texture != texture)
		{
			_batches.Insert(_currentBatchInsert, _currentBatch);

			_currentBatch.texture = texture;
			_currentBatch.offset += _currentBatch.elements;
			_currentBatch.elements = 0;
			_currentBatchInsert++;
		}
	}

	public void SetLayer(int layer)
	{
		if (_currentBatch.layer == layer)
			return;

		// insert last batch
		if (_currentBatch.elements > 0)
		{
			_batches.Insert(_currentBatchInsert, _currentBatch);
			_currentBatch.offset += _currentBatch.elements;
			_currentBatch.elements = 0;
		}

		// find the point to insert us
		var insert = 0;
		while (insert < _batches.Count && _batches[insert].layer >= layer)
			insert++;

		_currentBatch.layer = layer;
		_currentBatchInsert = insert;
	}

	public void SetState(Material? material, in BlendMode blendmode, in Transform2D matrix, RectInt? scissor)
	{
		SetMaterial(material);
		SetBlendMode(blendmode);
		SetMatrix(matrix);
		SetScissor(scissor);
	}

	public Transform2D PushMatrix(in Vector2 position, in Vector2 scale, in Vector2 origin, float rotation, bool relative = true)
	{
		return PushMatrix(Transform2D.CreateMatrix(position, origin, scale, rotation), relative);
	}

	public Transform2D PushMatrix(in Vector2 position, bool relative = true)
	{
		return PushMatrix(Transform2D.identity.Translated(position), relative);
	}

	public Transform2D PushMatrix(in Transform2D matrix, bool relative = true)
	{
		_matrixStack.Push(matrixStack);

		if (relative)
		{
			matrixStack = matrix * matrixStack;
		}
		else
		{
			matrixStack = matrix;
		}

		return matrixStack;
	}

	public Transform2D PopMatrix()
	{
		Debug.Assert(_matrixStack.Count > 0, "Batch.MatrixStack Pops more than it Pushes");

		if (_matrixStack.Count > 0)
		{
			matrixStack = _matrixStack.Pop();
		}
		else
		{
			matrixStack = Transform2D.identity;
		}

		return matrixStack;
	}

	#endregion

	#region Line

	public void SetLine(Vector2 from, Vector2 to, float thickness, in Color color)
	{
		var normal = (to - from).normalized;
		var perp = new Vector2(-normal.y, normal.x) * thickness * .5f;
		SetQuad(from + perp, from - perp, to - perp, to + perp, color);
	}

	public void SetDashedLine(Vector2 from, Vector2 to, float thickness, in Color color, float dashLength, float offsetPercent)
	{
		var diff = to - from;
		var dist = diff.length;
		var axis = diff.normalized;
		var perp = axis.turnLeft * (thickness * 0.5f);
		offsetPercent = ((offsetPercent % 1f) + 1f) % 1f;

		var startD = dashLength * offsetPercent * 2f;
		if (startD > dashLength)
			startD -= dashLength * 2f;

		for (float d = startD; d < dist; d += dashLength * 2f)
		{
			var a = from + axis * Mathf.Max(d, 0f);
			var b = from + axis * Mathf.Min(d + dashLength, dist);
			SetQuad(a + perp, b + perp, b - perp, a - perp, color);
		}
	}

	#endregion

	#region Quad

	public void SetQuad(in Quad2D quad, in Color color)
	{
		SetQuad(quad.a, quad.b, quad.c, quad.d, color);
	}

	public void SetQuad(in Vector2 v0, in Vector2 v1, in Vector2 v2, in Vector2 v3, in Color color)
	{
		PushQuad();
		ExpandVertices(_vertexCount + 4);

		// POS
		TransformVertex(ref _vertices[_vertexCount + 0].pos, v0, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 1].pos, v1, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 2].pos, v2, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 3].pos, v3, matrixStack);

		// COL
		_vertices[_vertexCount + 0].col = color;
		_vertices[_vertexCount + 1].col = color;
		_vertices[_vertexCount + 2].col = color;
		_vertices[_vertexCount + 3].col = color;

		// MULT
		_vertices[_vertexCount + 0].mult = 0;
		_vertices[_vertexCount + 1].mult = 0;
		_vertices[_vertexCount + 2].mult = 0;
		_vertices[_vertexCount + 3].mult = 0;

		// WASH
		_vertices[_vertexCount + 0].wash = 0;
		_vertices[_vertexCount + 1].wash = 0;
		_vertices[_vertexCount + 2].wash = 0;
		_vertices[_vertexCount + 3].wash = 0;

		// FILL
		_vertices[_vertexCount + 0].fill = 255;
		_vertices[_vertexCount + 1].fill = 255;
		_vertices[_vertexCount + 2].fill = 255;
		_vertices[_vertexCount + 3].fill = 255;

		_vertexCount += 4;
	}

	public void SetQuad(in Vector2 v0, in Vector2 v1, in Vector2 v2, in Vector2 v3, in Vector2 t0, in Vector2 t1, in Vector2 t2, in Vector2 t3, in Color color, bool washed = false)
	{
		PushQuad();
		ExpandVertices(_vertexCount + 4);

		var mult = (byte)(washed ? 0 : 255);
		var wash = (byte)(washed ? 255 : 0);

		// POS
		TransformVertex(ref _vertices[_vertexCount + 0].pos, v0, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 1].pos, v1, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 2].pos, v2, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 3].pos, v3, matrixStack);

		// TEX
		_vertices[_vertexCount + 0].tex = t0;
		_vertices[_vertexCount + 1].tex = t1;
		_vertices[_vertexCount + 2].tex = t2;
		_vertices[_vertexCount + 3].tex = t3;

		if (graphics.originBottomLeft && (_currentBatch.texture?.isFrameBuffer ?? false))
			VerticalFlipTexture(ref _vertices[_vertexCount + 0].tex, ref _vertices[_vertexCount + 1].tex, ref _vertices[_vertexCount + 2].tex, ref _vertices[_vertexCount + 3].tex);

		// COL
		_vertices[_vertexCount + 0].col = color;
		_vertices[_vertexCount + 1].col = color;
		_vertices[_vertexCount + 2].col = color;
		_vertices[_vertexCount + 3].col = color;

		// MULT
		_vertices[_vertexCount + 0].mult = mult;
		_vertices[_vertexCount + 1].mult = mult;
		_vertices[_vertexCount + 2].mult = mult;
		_vertices[_vertexCount + 3].mult = mult;

		// WASH
		_vertices[_vertexCount + 0].wash = wash;
		_vertices[_vertexCount + 1].wash = wash;
		_vertices[_vertexCount + 2].wash = wash;
		_vertices[_vertexCount + 3].wash = wash;

		// FILL
		_vertices[_vertexCount + 0].fill = 0;
		_vertices[_vertexCount + 1].fill = 0;
		_vertices[_vertexCount + 2].fill = 0;
		_vertices[_vertexCount + 3].fill = 0;

		_vertexCount += 4;
	}

	public void SetQuad(in Vector2 v0, in Vector2 v1, in Vector2 v2, in Vector2 v3, Color c0, Color c1, Color c2, Color c3)
	{
		PushQuad();
		ExpandVertices(_vertexCount + 4);

		// POS
		TransformVertex(ref _vertices[_vertexCount + 0].pos, v0, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 1].pos, v1, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 2].pos, v2, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 3].pos, v3, matrixStack);

		// COL
		_vertices[_vertexCount + 0].col = c0;
		_vertices[_vertexCount + 1].col = c1;
		_vertices[_vertexCount + 2].col = c2;
		_vertices[_vertexCount + 3].col = c3;

		// MULT
		_vertices[_vertexCount + 0].mult = 0;
		_vertices[_vertexCount + 1].mult = 0;
		_vertices[_vertexCount + 2].mult = 0;
		_vertices[_vertexCount + 3].mult = 0;

		// WASH
		_vertices[_vertexCount + 0].wash = 0;
		_vertices[_vertexCount + 1].wash = 0;
		_vertices[_vertexCount + 2].wash = 0;
		_vertices[_vertexCount + 3].wash = 0;

		// FILL
		_vertices[_vertexCount + 0].fill = 255;
		_vertices[_vertexCount + 1].fill = 255;
		_vertices[_vertexCount + 2].fill = 255;
		_vertices[_vertexCount + 3].fill = 255;

		_vertexCount += 4;
	}

	public void SetQuad(in Vector2 v0, in Vector2 v1, in Vector2 v2, in Vector2 v3, in Vector2 t0, in Vector2 t1, in Vector2 t2, in Vector2 t3, Color c0, Color c1, Color c2, Color c3, bool washed = false)
	{
		PushQuad();
		ExpandVertices(_vertexCount + 4);

		var mult = (byte)(washed ? 0 : 255);
		var wash = (byte)(washed ? 255 : 0);

		// POS
		TransformVertex(ref _vertices[_vertexCount + 0].pos, v0, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 1].pos, v1, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 2].pos, v2, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 3].pos, v3, matrixStack);

		// TEX
		_vertices[_vertexCount + 0].tex = t0;
		_vertices[_vertexCount + 1].tex = t1;
		_vertices[_vertexCount + 2].tex = t2;
		_vertices[_vertexCount + 3].tex = t3;

		if (graphics.originBottomLeft && (_currentBatch.texture?.isFrameBuffer ?? false))
			VerticalFlipTexture(ref _vertices[_vertexCount + 0].tex, ref _vertices[_vertexCount + 1].tex, ref _vertices[_vertexCount + 2].tex, ref _vertices[_vertexCount + 3].tex);

		// COL
		_vertices[_vertexCount + 0].col = c0;
		_vertices[_vertexCount + 1].col = c1;
		_vertices[_vertexCount + 2].col = c2;
		_vertices[_vertexCount + 3].col = c3;

		// MULT
		_vertices[_vertexCount + 0].mult = mult;
		_vertices[_vertexCount + 1].mult = mult;
		_vertices[_vertexCount + 2].mult = mult;
		_vertices[_vertexCount + 3].mult = mult;

		// WASH
		_vertices[_vertexCount + 0].wash = wash;
		_vertices[_vertexCount + 1].wash = wash;
		_vertices[_vertexCount + 2].wash = wash;
		_vertices[_vertexCount + 3].wash = wash;

		// FILL
		_vertices[_vertexCount + 0].fill = 0;
		_vertices[_vertexCount + 1].fill = 0;
		_vertices[_vertexCount + 2].fill = 0;
		_vertices[_vertexCount + 3].fill = 0;

		_vertexCount += 4;
	}

	#endregion

	#region Triangle

	public void SetTriangle(in Vector2 v0, in Vector2 v1, in Vector2 v2, in Color color)
	{
		PushTriangle();
		ExpandVertices(_vertexCount + 3);

		// POS
		TransformVertex(ref _vertices[_vertexCount + 0].pos, v0, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 1].pos, v1, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 2].pos, v2, matrixStack);

		// COL
		_vertices[_vertexCount + 0].col = color;
		_vertices[_vertexCount + 1].col = color;
		_vertices[_vertexCount + 2].col = color;

		// MULT
		_vertices[_vertexCount + 0].mult = 0;
		_vertices[_vertexCount + 1].mult = 0;
		_vertices[_vertexCount + 2].mult = 0;
		_vertices[_vertexCount + 3].mult = 0;

		// WASH
		_vertices[_vertexCount + 0].wash = 0;
		_vertices[_vertexCount + 1].wash = 0;
		_vertices[_vertexCount + 2].wash = 0;
		_vertices[_vertexCount + 3].wash = 0;

		// FILL
		_vertices[_vertexCount + 0].fill = 255;
		_vertices[_vertexCount + 1].fill = 255;
		_vertices[_vertexCount + 2].fill = 255;
		_vertices[_vertexCount + 3].fill = 255;

		_vertexCount += 3;
	}

	public void SetTriangle(in Vector2 v0, in Vector2 v1, in Vector2 v2, Color c0, Color c1, Color c2)
	{
		PushTriangle();
		ExpandVertices(_vertexCount + 3);

		// POS
		TransformVertex(ref _vertices[_vertexCount + 0].pos, v0, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 1].pos, v1, matrixStack);
		TransformVertex(ref _vertices[_vertexCount + 2].pos, v2, matrixStack);

		// COL
		_vertices[_vertexCount + 0].col = c0;
		_vertices[_vertexCount + 1].col = c1;
		_vertices[_vertexCount + 2].col = c2;

		// MULT
		_vertices[_vertexCount + 0].mult = 0;
		_vertices[_vertexCount + 1].mult = 0;
		_vertices[_vertexCount + 2].mult = 0;
		_vertices[_vertexCount + 3].mult = 0;

		// WASH
		_vertices[_vertexCount + 0].wash = 0;
		_vertices[_vertexCount + 1].wash = 0;
		_vertices[_vertexCount + 2].wash = 0;
		_vertices[_vertexCount + 3].wash = 0;

		// FILL
		_vertices[_vertexCount + 0].fill = 255;
		_vertices[_vertexCount + 1].fill = 255;
		_vertices[_vertexCount + 2].fill = 255;
		_vertices[_vertexCount + 3].fill = 255;

		_vertexCount += 3;
	}

	#endregion

	#region Rect

	public void SetBox(in Rect rect, in Color color)
	{
		SetQuad(
				new Vector2(rect.x, rect.y),
				new Vector2(rect.x + rect.width, rect.y),
				new Vector2(rect.x + rect.width, rect.y + rect.height),
				new Vector2(rect.x, rect.y + rect.height),
				color);
	}

	public void SetBox(in Vector2 position, in Vector2 size, in Color color)
	{
		SetQuad(
				position,
				position + new Vector2(size.x, 0),
				position + new Vector2(size.x, size.y),
				position + new Vector2(0, size.y),
				color);
	}

	public void SetBox(float x, float y, float width, float height, in Color color)
	{
		SetQuad(
				new Vector2(x, y),
				new Vector2(x + width, y),
				new Vector2(x + width, y + height),
				new Vector2(x, y + height), color);
	}

	public void SetBox(in Rect rect, Color c0, Color c1, Color c2, Color c3)
	{
		SetQuad(
				new Vector2(rect.x, rect.y),
				new Vector2(rect.x + rect.width, rect.y),
				new Vector2(rect.x + rect.width, rect.y + rect.height),
				new Vector2(rect.x, rect.y + rect.height),
				c0, c1, c2, c3);
	}

	public void SetBox(in Vector2 position, in Vector2 size, Color c0, Color c1, Color c2, Color c3)
	{
		SetQuad(
				position,
				position + new Vector2(size.x, 0),
				position + new Vector2(size.x, size.y),
				position + new Vector2(0, size.y),
				c0, c1, c2, c3);
	}

	public void SetBox(float x, float y, float width, float height, Color c0, Color c1, Color c2, Color c3)
	{
		SetQuad(
				new Vector2(x, y),
				new Vector2(x + width, y),
				new Vector2(x + width, y + height),
				new Vector2(x, y + height),
				c0, c1, c2, c3);
	}

	#endregion

	#region Rounded Rect

	public void SetRoundedRect(float x, float y, float width, float height, float r0, float r1, float r2, float r3, in Color color)
	{
		SetRoundedRect(new Rect(x, y, width, height), r0, r1, r2, r3, color);
	}

	public void SetRoundedRect(float x, float y, float width, float height, float radius, in Color color)
	{
		SetRoundedRect(new Rect(x, y, width, height), radius, radius, radius, radius, color);
	}

	public void SetRoundedRect(in Rect rect, float radius, in Color color)
	{
		SetRoundedRect(rect, radius, radius, radius, radius, color);
	}

	public void SetRoundedRect(in Rect rect, float r0, float r1, float r2, float r3, in Color color)
	{
		// clamp
		r0 = Mathf.Min(Mathf.Min(Mathf.Max(0, r0), rect.width / 2f), rect.height / 2f);
		r1 = Mathf.Min(Mathf.Min(Mathf.Max(0, r1), rect.width / 2f), rect.height / 2f);
		r2 = Mathf.Min(Mathf.Min(Mathf.Max(0, r2), rect.width / 2f), rect.height / 2f);
		r3 = Mathf.Min(Mathf.Min(Mathf.Max(0, r3), rect.width / 2f), rect.height / 2f);

		if (r0 <= 0 && r1 <= 0 && r2 <= 0 && r3 <= 0)
		{
			SetBox(rect, color);
		}
		else
		{
			// get corners
			var r0_tl = rect.topLeft;
			var r0_tr = r0_tl + new Vector2(r0, 0);
			var r0_br = r0_tl + new Vector2(r0, r0);
			var r0_bl = r0_tl + new Vector2(0, r0);

			var r1_tl = rect.topRight + new Vector2(-r1, 0);
			var r1_tr = r1_tl + new Vector2(r1, 0);
			var r1_br = r1_tl + new Vector2(r1, r1);
			var r1_bl = r1_tl + new Vector2(0, r1);

			var r2_tl = rect.bottomRight + new Vector2(-r2, -r2);
			var r2_tr = r2_tl + new Vector2(r2, 0);
			var r2_bl = r2_tl + new Vector2(0, r2);
			var r2_br = r2_tl + new Vector2(r2, r2);

			var r3_tl = rect.bottomLeft + new Vector2(0, -r3);
			var r3_tr = r3_tl + new Vector2(r3, 0);
			var r3_bl = r3_tl + new Vector2(0, r3);
			var r3_br = r3_tl + new Vector2(r3, r3);

			// set tris
			{
				while (_indexCount + 30 >= _indices.Length)
					Array.Resize(ref _indices, _indices.Length * 2);

				// top quad
				{
					_indices[_indexCount + 00] = _vertexCount + 00; // r0b
					_indices[_indexCount + 01] = _vertexCount + 03; // r1a
					_indices[_indexCount + 02] = _vertexCount + 05; // r1d

					_indices[_indexCount + 03] = _vertexCount + 00; // r0b
					_indices[_indexCount + 04] = _vertexCount + 05; // r1d
					_indices[_indexCount + 05] = _vertexCount + 01; // r0c
				}

				// left quad
				{
					_indices[_indexCount + 06] = _vertexCount + 02; // r0d
					_indices[_indexCount + 07] = _vertexCount + 01; // r0c
					_indices[_indexCount + 08] = _vertexCount + 10; // r3b

					_indices[_indexCount + 09] = _vertexCount + 02; // r0d
					_indices[_indexCount + 10] = _vertexCount + 10; // r3b
					_indices[_indexCount + 11] = _vertexCount + 09; // r3a
				}

				// right quad
				{
					_indices[_indexCount + 12] = _vertexCount + 05; // r1d
					_indices[_indexCount + 13] = _vertexCount + 04; // r1c
					_indices[_indexCount + 14] = _vertexCount + 07; // r2b

					_indices[_indexCount + 15] = _vertexCount + 05; // r1d
					_indices[_indexCount + 16] = _vertexCount + 07; // r2b
					_indices[_indexCount + 17] = _vertexCount + 06; // r2a
				}

				// bottom quad
				{
					_indices[_indexCount + 18] = _vertexCount + 10; // r3b
					_indices[_indexCount + 19] = _vertexCount + 06; // r2a
					_indices[_indexCount + 20] = _vertexCount + 08; // r2d

					_indices[_indexCount + 21] = _vertexCount + 10; // r3b
					_indices[_indexCount + 22] = _vertexCount + 08; // r2d
					_indices[_indexCount + 23] = _vertexCount + 11; // r3c
				}

				// center quad
				{
					_indices[_indexCount + 24] = _vertexCount + 01; // r0c
					_indices[_indexCount + 25] = _vertexCount + 05; // r1d
					_indices[_indexCount + 26] = _vertexCount + 06; // r2a

					_indices[_indexCount + 27] = _vertexCount + 01; // r0c
					_indices[_indexCount + 28] = _vertexCount + 06; // r2a
					_indices[_indexCount + 29] = _vertexCount + 10; // r3b
				}

				_indexCount += 30;
				_currentBatch.elements += 10;
				_dirty = true;
			}

			// set verts
			{
				ExpandVertices(_vertexCount + 12);

				Array.Fill(_vertices, new Vertex(Vector2.zero, Vector2.zero, color, 0, 0, 255), _vertexCount, 12);

				TransformVertex(ref _vertices[_vertexCount + 00].pos, r0_tr, matrixStack); // 0
				TransformVertex(ref _vertices[_vertexCount + 01].pos, r0_br, matrixStack); // 1
				TransformVertex(ref _vertices[_vertexCount + 02].pos, r0_bl, matrixStack); // 2

				TransformVertex(ref _vertices[_vertexCount + 03].pos, r1_tl, matrixStack); // 3
				TransformVertex(ref _vertices[_vertexCount + 04].pos, r1_br, matrixStack); // 4
				TransformVertex(ref _vertices[_vertexCount + 05].pos, r1_bl, matrixStack); // 5

				TransformVertex(ref _vertices[_vertexCount + 06].pos, r2_tl, matrixStack); // 6
				TransformVertex(ref _vertices[_vertexCount + 07].pos, r2_tr, matrixStack); // 7
				TransformVertex(ref _vertices[_vertexCount + 08].pos, r2_bl, matrixStack); // 8

				TransformVertex(ref _vertices[_vertexCount + 09].pos, r3_tl, matrixStack); // 9
				TransformVertex(ref _vertices[_vertexCount + 10].pos, r3_tr, matrixStack); // 10
				TransformVertex(ref _vertices[_vertexCount + 11].pos, r3_br, matrixStack); // 11

				_vertexCount += 12;
			}

			// TODO  replace with hard-coded values
			var left = Calc.Angle(Vector2.left);
			var right = Calc.Angle(Vector2.right);
			var up = Calc.Angle(Vector2.up);
			var down = Calc.Angle(Vector2.down);

			// top-left corner
			if (r0 > 0)
				SetSemiCircle(r0_br, up, -left, r0, Mathf.Max(3, (int)(r0 / 4)), color);
			else
				SetQuad(r0_tl, r0_tr, r0_br, r0_bl, color);

			// top-right corner
			if (r1 > 0)
				SetSemiCircle(r1_bl, up, right, r1, Mathf.Max(3, (int)(r1 / 4)), color);
			else
				SetQuad(r1_tl, r1_tr, r1_br, r1_bl, color);

			// bottom-right corner
			if (r2 > 0)
				SetSemiCircle(r2_tl, right, down, r2, Mathf.Max(3, (int)(r2 / 4)), color);
			else
				SetQuad(r2_tl, r2_tr, r2_br, r2_bl, color);

			// bottom-left corner
			if (r3 > 0)
				SetSemiCircle(r3_tr, down, left, r3, Mathf.Max(3, (int)(r3 / 4)), color);
			else
				SetQuad(r3_tl, r3_tr, r3_br, r3_bl, color);
		}

	}

	#endregion

	#region Circle

	public void SetSemiCircle(Vector2 center, float startRadians, float endRadians, float radius, int steps, in Color color)
	{
		SetSemiCircle(center, startRadians, endRadians, radius, steps, color, color);
	}

	public void SetSemiCircle(Vector2 center, float startRadians, float endRadians, float radius, int steps, Color centerColor, Color edgeColor)
	{
		var last = Calc.AngleToVector(startRadians, radius);

		for (int i = 1; i <= steps; i++)
		{
			var next = Calc.AngleToVector(startRadians + (endRadians - startRadians) * (i / (float)steps), radius);
			SetTriangle(center + last, center + next, center, edgeColor, edgeColor, centerColor);
			last = next;
		}
	}

	public void SetCircle(Vector2 center, float radius, int steps, in Color color)
	{
		SetCircle(center, radius, steps, color, color);
	}

	public void SetCircle(Vector2 center, float radius, int steps, Color centerColor, Color edgeColor)
	{
		var last = Calc.AngleToVector(0, radius);

		for (int i = 1; i <= steps; i++)
		{
			var next = Calc.AngleToVector((i / (float)steps) * Calc.TAU, radius);
			SetTriangle(center + last, center + next, center, edgeColor, edgeColor, centerColor);
			last = next;
		}
	}

	public void SetRing(Vector2 center, float radius, float thickness, int steps, in Color color)
	{
		var last = Calc.AngleToVector(0, radius);

		for (int i = 1; i <= steps; i++)
		{
			var next = Calc.AngleToVector((i / (float)steps) * Calc.TAU, radius);
			SetLine(center + last, center + next, thickness, color);
			last = next;
		}
	}

	#endregion

	#region Hollow Rect

	// public void SetRect(in Rect rect, float t, in Color color)
	// {
	// 	if (t != 0)
	// 	{
	// 		var tx = Mathf.Min(t, rect.width / 2f);
	// 		var ty = Mathf.Min(t, rect.height / 2f);

	// 		SetBox(rect.x, rect.y, rect.width, ty, color);
	// 		SetBox(rect.x, rect.bottom - ty, rect.width, ty, color);
	// 		SetBox(rect.x, rect.y + ty, tx, rect.height - ty * 2, color);
	// 		SetBox(rect.right - tx, rect.y + ty, tx, rect.height - ty * 2, color);
	// 	}
	// }

	public void SetRect(in Rect rect, float lineThickness, in Color color)
	{
		var outerRect = rect.Expanded(lineThickness);

		// The rectangle
		// 4      5
		//   0  1
		//   2  3
		// 6      7

		ExpandIndices(_indexCount + 24);

		PushIndices(0, 4, 1);
		PushIndices(1, 5, 4);

		PushIndices(1, 5, 3);
		PushIndices(3, 7, 5);

		PushIndices(3, 7, 2);
		PushIndices(2, 6, 7);

		PushIndices(2, 6, 0);
		PushIndices(0, 4, 6);

		ExpandVertices(_vertexCount + 8);

		PushVertex(rect.topLeft, color);
		PushVertex(rect.topRight, color);
		PushVertex(rect.bottomLeft, color);
		PushVertex(rect.bottomRight, color);

		PushVertex(outerRect.topLeft, color);
		PushVertex(outerRect.topRight, color);
		PushVertex(outerRect.bottomLeft, color);
		PushVertex(outerRect.bottomRight, color);

		_currentBatch.elements += 8;
		_dirty = true;
	}

	#endregion

	#region Image

	public void Draw(Texture texture, Vector2 position, in Color color, bool washed = false)
	{
		var halfWidth = (float)texture.width / 2;
		var halfHeight = (float)texture.height / 2;

		SetTexture(texture);
		SetQuad(
			new(position.x - halfWidth, position.y - halfHeight),
			new(position.x + halfWidth, position.y - halfHeight),
			new(position.x + halfWidth, position.y + halfHeight),
			new(position.x - halfWidth, position.y + halfHeight),
			new(0, 0),
			new(1, 0),
			new(1, 1),
			new(0, 1),
		color, washed);
	}

	public void Draw(Texture texture, Vector2 position, Vector2 pivot, float rotation, Vector2 scale, in Color color, bool washed = false)
	{
		var halfWidth = (float)texture.width / 2;
		var halfHeight = (float)texture.height / 2;

		SetTexture(texture);
		SetQuad(
			// position + Vector2.Transform(new(-halfWidth, -halfHeight), t),
			// position + Vector2.Transform(new(+halfWidth, -halfHeight), t),
			// position + Vector2.Transform(new(+halfWidth, +halfHeight), t),
			// position + Vector2.Transform(new(-halfWidth, +halfHeight), t),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x - halfWidth, position.y - halfHeight) * scale, rotation),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x + halfWidth, position.y - halfHeight) * scale, rotation),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x + halfWidth, position.y + halfHeight) * scale, rotation),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x - halfWidth, position.y + halfHeight) * scale, rotation),
			new(0, 0),
			new(1, 0),
			new(1, 1),
			new(0, 1),
		color, washed);
	}

	public void Draw(Texture texture, Rect src, Vector2 position, in Color color, bool washed = false)
	{
		var tx0 = src.x / texture.width;
		var ty0 = src.y / texture.height;
		var tx1 = src.right / texture.width;
		var ty1 = src.bottom / texture.height;

		var halfWidth = src.width / 2;
		var halfHeight = src.height / 2;

		SetTexture(texture);
		SetQuad(
			new(position.x - halfWidth, position.y - halfHeight),
			new(position.x + halfWidth, position.y - halfHeight),
			new(position.x + halfWidth, position.y + halfHeight),
			new(position.x - halfWidth, position.y + halfHeight),
			new(tx0, ty0), new(tx1, ty0), new(tx1, ty1), new(tx0, ty1),
		color, washed);
	}

	public void Draw(Texture texture, Rect src, Vector2 position, Vector2 pivot, float rotation, Vector2 scale, in Color color, bool washed = false)
	{
		var tx0 = src.x / texture.width;
		var ty0 = src.y / texture.height;
		var tx1 = src.right / texture.width;
		var ty1 = src.bottom / texture.height;

		var halfWidth = src.width / 2;
		var halfHeight = src.height / 2;

		SetTexture(texture);
		SetQuad(
			// position + Vector2.Transform(new(-halfWidth, -halfHeight), t),
			// position + Vector2.Transform(new(+halfWidth, -halfHeight), t),
			// position + Vector2.Transform(new(+halfWidth, +halfHeight), t),
			// position + Vector2.Transform(new(-halfWidth, +halfHeight), t),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x - halfWidth, position.y - halfHeight) * scale, rotation),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x + halfWidth, position.y - halfHeight) * scale, rotation),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x + halfWidth, position.y + halfHeight) * scale, rotation),
			Vector2.RotateAroundPoint(pivot, new Vector2(position.x - halfWidth, position.y + halfHeight) * scale, rotation),
			new(tx0, ty0), new(tx1, ty0), new(tx1, ty1), new(tx0, ty1),
		color, washed);
	}

	public void Draw(Subtexture subtex, Vector2 position, in Color color, bool washed = false)
	{
		var offset = subtex.frame.size / 2;
		// var halfWidth = subtex.source.width / 2;
		// var halfHeight = subtex.source.height / 2;

		SetTexture(subtex.texture);
		SetQuad(
			subtex.drawCoords[0] - offset + position,
			subtex.drawCoords[1] - offset + position,
			subtex.drawCoords[2] - offset + position,
			subtex.drawCoords[3] - offset + position,
			subtex.texCoords[0], subtex.texCoords[1], subtex.texCoords[2], subtex.texCoords[3],
		color, washed);

		return;

		// // var halfWidth = subtex.source.width / 2;
		// // var halfHeight = subtex.source.height / 2;

		// SetTexture(subtex.texture);
		// SetQuad(
		// 	new(position.x - halfWidth, position.y - halfHeight),
		// 	new(position.x + halfWidth, position.y - halfHeight),
		// 	new(position.x + halfWidth, position.y + halfHeight),
		// 	new(position.x - halfWidth, position.y + halfHeight),
		// 	subtex.texCoords[0], subtex.texCoords[1], subtex.texCoords[2], subtex.texCoords[3],
		// color, washed);
	}

	public void Draw(Subtexture subtex, Vector2 position, Vector2 pivot, float rotation, Vector2 scale, in Color color, bool washed = false)
	{
		// Draw(subtex, position, color, washed);
		// return;
		var offset = subtex.frame.size / 2;

		SetTexture(subtex.texture);
		SetQuad(
			Vector2.RotateAroundPoint(pivot, subtex.drawCoords[0] - offset + position, rotation),
			Vector2.RotateAroundPoint(pivot, subtex.drawCoords[1] - offset + position, rotation),
			Vector2.RotateAroundPoint(pivot, subtex.drawCoords[2] - offset + position, rotation),
			Vector2.RotateAroundPoint(pivot, subtex.drawCoords[3] - offset + position, rotation),
			// position + Vector2.Transform(new(-halfWidth, -halfHeight), t),
			// position + Vector2.Transform(new(+halfWidth, -halfHeight), t),
			// position + Vector2.Transform(new(+halfWidth, +halfHeight), t),
			// position + Vector2.Transform(new(-halfWidth, +halfHeight), t),
			// Vector2.RotateAroundPoint(pivot, new Vector2(position.x - halfWidth, position.y - halfHeight) * scale, rotation),
			// Vector2.RotateAroundPoint(pivot, new Vector2(position.x + halfWidth, position.y - halfHeight) * scale, rotation),
			// Vector2.RotateAroundPoint(pivot, new Vector2(position.x + halfWidth, position.y + halfHeight) * scale, rotation),
			// Vector2.RotateAroundPoint(pivot, new Vector2(position.x - halfWidth, position.y + halfHeight) * scale, rotation),
			subtex.texCoords[0], subtex.texCoords[1], subtex.texCoords[2], subtex.texCoords[3],
		color, washed);
	}

	#endregion Image

	#region Image

	public void DrawImage(Texture texture,
		in Vector2 pos0, in Vector2 pos1, in Vector2 pos2, in Vector2 pos3,
		in Vector2 uv0, in Vector2 uv1, in Vector2 uv2, in Vector2 uv3,
		Color col0, Color col1, Color col2, Color col3, bool washed = false)
	{
		SetTexture(texture);
		SetQuad(pos0, pos1, pos2, pos3, uv0, uv1, uv2, uv3, col0, col1, col2, col3, washed);
	}

	public void DrawImage(Texture texture,
			in Vector2 pos0, in Vector2 pos1, in Vector2 pos2, in Vector2 pos3,
			in Vector2 uv0, in Vector2 uv1, in Vector2 uv2, in Vector2 uv3,
			Color color, bool washed)
	{
		SetTexture(texture);
		SetQuad(pos0, pos1, pos2, pos3, uv0, uv1, uv2, uv3, color, washed);
	}

	public void DrawImage(Texture texture, in Rect dest, in Color color, bool washed = false)
	{
		SetTexture(texture);
		SetQuad(
			dest.topLeft, dest.topRight, dest.bottomRight, dest.bottomLeft,
			Vector2.zero, Vector2.right, Vector2.downRight, Vector2.down,
		color, washed);
	}

	public void DrawImage(Texture texture, in Rect clip, in Rect dest, in Color color, bool washed = false)
	{
		var tx0 = clip.x / texture.width;
		var ty0 = clip.y / texture.height;
		var tx1 = clip.right / texture.width;
		var ty1 = clip.bottom / texture.height;

		SetTexture(texture);
		SetQuad(
			dest.topLeft, dest.topRight, dest.bottomRight, dest.bottomLeft,
			new(tx0, ty0), new(tx1, ty0), new(tx1, ty1), new(tx0, ty1),
		color, washed);
	}

	public void DrawImage(Texture texture, in Vector2 position, in Color color, bool washed = false)
	{
		SetTexture(texture);
		SetQuad(
				position,
				position + new Vector2(texture.width, 0),
				position + new Vector2(texture.width, texture.height),
				position + new Vector2(0, texture.height),
				new Vector2(0, 0),
				Vector2.right,
				new Vector2(1, 1),
				Vector2.down,
				color, washed);
	}

	public void DrawImage(Texture texture, in Vector2 position, in Vector2 scale, in Vector2 origin, float rotation, in Color color, bool washed = false)
	{
		var was = matrixStack;

		matrixStack = Transform2D.CreateMatrix(position, origin, scale, rotation) * matrixStack;

		SetTexture(texture);
		SetQuad(
				new Vector2(0, 0),
				new Vector2(texture.width, 0),
				new Vector2(texture.width, texture.height),
				new Vector2(0, texture.height),
				new Vector2(0, 0),
				Vector2.right,
				new Vector2(1, 1),
				Vector2.down,
				color, washed);

		matrixStack = was;
	}

	public void DrawImage(Texture texture, in Rect clip, in Vector2 position, in Color color, bool washed = false)
	{
		var tx0 = clip.x / texture.width;
		var ty0 = clip.y / texture.height;
		var tx1 = clip.right / texture.width;
		var ty1 = clip.bottom / texture.height;

		SetTexture(texture);
		SetQuad(
				position,
				position + new Vector2(clip.width, 0),
				position + new Vector2(clip.width, clip.height),
				position + new Vector2(0, clip.height),
				new Vector2(tx0, ty0),
				new Vector2(tx1, ty0),
				new Vector2(tx1, ty1),
				new Vector2(tx0, ty1), color, washed);
	}

	public void DrawImage(Texture texture, in Rect clip, in Vector2 position, in Vector2 scale, in Vector2 origin, float rotation, in Color color, bool washed = false)
	{
		var was = matrixStack;

		matrixStack = Transform2D.CreateMatrix(position, origin, scale, rotation) * matrixStack;

		var tx0 = clip.x / texture.width;
		var ty0 = clip.y / texture.height;
		var tx1 = clip.right / texture.width;
		var ty1 = clip.bottom / texture.height;

		SetTexture(texture);
		SetQuad(
				new Vector2(0, 0),
				new Vector2(clip.width, 0),
				new Vector2(clip.width, clip.height),
				new Vector2(0, clip.height),
				new Vector2(tx0, ty0),
				new Vector2(tx1, ty0),
				new Vector2(tx1, ty1),
				new Vector2(tx0, ty1),
				color, washed);

		matrixStack = was;
	}

	public void DrawImage(Subtexture subtex, in Vector2 position, in Color color, bool washed = false)
	{
		SetTexture(subtex.texture);
		SetQuad(position + subtex.drawCoords[0], position + subtex.drawCoords[1], position + subtex.drawCoords[2], position + subtex.drawCoords[3],
				subtex.texCoords[0], subtex.texCoords[1], subtex.texCoords[2], subtex.texCoords[3],
				color, washed);
	}

	public void DrawImage(Subtexture subtex, in Vector2 position, in Vector2 scale, in Vector2 origin, float rotation, in Color color, bool washed = false)
	{
		var was = matrixStack;

		matrixStack = Transform2D.CreateMatrix(position, origin, scale, rotation) * matrixStack;

		SetTexture(subtex.texture);
		SetQuad(
				subtex.drawCoords[0], subtex.drawCoords[1], subtex.drawCoords[2], subtex.drawCoords[3],
				subtex.texCoords[0], subtex.texCoords[1], subtex.texCoords[2], subtex.texCoords[3],
				color, washed);

		matrixStack = was;
	}

	public void DrawImage(Subtexture subtex, in Rect clip, in Vector2 position, in Vector2 scale, in Vector2 origin, float rotation, in Color color, bool washed = false)
	{
		var (source, frame) = subtex.GetClip(clip);
		var tex = subtex.texture;
		var was = matrixStack;

		matrixStack = Transform2D.CreateMatrix(position, origin, scale, rotation) * matrixStack;

		var px0 = -frame.x;
		var py0 = -frame.y;
		var px1 = -frame.x + source.width;
		var py1 = -frame.y + source.height;

		var tx0 = 0f;
		var ty0 = 0f;
		var tx1 = 0f;
		var ty1 = 0f;

		if (tex is not null)
		{
			tx0 = source.left / tex.width;
			ty0 = source.top / tex.height;
			tx1 = source.right / tex.width;
			ty1 = source.bottom / tex.height;
		}

		SetTexture(subtex.texture);
		SetQuad(
				new Vector2(px0, py0), new Vector2(px1, py0), new Vector2(px1, py1), new Vector2(px0, py1),
				new Vector2(tx0, ty0), new Vector2(tx1, ty0), new Vector2(tx1, ty1), new Vector2(tx0, ty1),
				color, washed);

		matrixStack = was;
	}

	#endregion

	#region Text

	public void DrawString(IFont font, string text, Vector2 position, in Color color, float fontSize, float lineHeight = 1.15f)
	{
		DrawString(font, text.AsSpan(), position, color, fontSize, lineHeight);
	}

	public void DrawString(IFont font, ReadOnlySpan<char> text, Vector2 position, in Color color, float fontSize, float lineSpacing = 1.15f)
	{
		DrawString(font, font.BuildString(text), position, color, font.GetScale(fontSize), lineSpacing);
	}

	public void DrawString(IFont font, StringBuildInfo str, Vector2 position, in Color color, float scale, float lineSpacing = 1.15f)
	{
		var y = 0f;

		font.BeginRender(this);
		foreach (var line in str.lines)
		{
			foreach (var ch in line.chars)
			{
				font.RenderChar(this, ch.ch, position.x + ch.x * scale, position.y + y * lineSpacing * scale, scale, color);
			}

			y += font.lineHeight;
		}
		font.AfterRender(this);
	}

	public void DrawString(IFont font, ReadOnlySpan<char> text, Rect rect, TextAlignment textAlignment, in Color color, float fontSize, float lineSpacing = 1.15f)
	{
		var position = rect.position;

		var scale = font.GetScale(fontSize);
		var str = font.BuildString(text);

		switch (textAlignment)
		{
			case TextAlignment.Right:
				position.x += rect.width - str.width * scale;
				break;
			case TextAlignment.Center:
				position.x += (rect.width - str.width * scale) / 2;
				break;
		}

		position.y += (rect.height - font.lineHeight * str.lines.Count * lineSpacing * scale) / 2;

		DrawString(font, str, position, color, scale, lineSpacing);
	}

	#endregion

	#region Copy Arrays

	/// <summary>
	/// Copies the contents of a Vertex and Index array to this Batcher
	/// </summary>
	public void CopyArray(ReadOnlySpan<Vertex> vertexBuffer, ReadOnlySpan<int> indexBuffer)
	{
		// copy vertices over
		ExpandVertices(_vertexCount + vertexBuffer.Length);
		vertexBuffer.CopyTo(_vertices.AsSpan().Slice(_vertexCount));

		// copy indices over
		while (_indexCount + indexBuffer.Length >= _indices.Length)
			Array.Resize(ref _indices, _indices.Length * 2);
		for (int i = 0, n = _indexCount; i < indexBuffer.Length; i++, n++)
			_indices[n] = _vertexCount + indexBuffer[i];

		// increment
		_vertexCount += vertexBuffer.Length;
		_indexCount += indexBuffer.Length;
		_currentBatch.elements += (uint)(vertexBuffer.Length / 3);
		_dirty = true;
	}

	#endregion

	#region Misc.

	public void CheckeredPattern(in Rect bounds, float cellWidth, float cellHeight, Color a, Color b)
	{
		var odd = false;

		for (float y = bounds.top; y < bounds.bottom; y += cellHeight)
		{
			var cells = 0;
			for (float x = bounds.left; x < bounds.right; x += cellWidth)
			{
				var color = (odd ? a : b);
				if (color.a > 0)
					SetBox(x, y, Mathf.Min(bounds.right - x, cellWidth), Mathf.Min(bounds.bottom - y, cellHeight), color);

				odd = !odd;
				cells++;
			}

			if (cells % 2 == 0)
				odd = !odd;
		}
	}

	#endregion

	#region Internal Utils

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushTriangle()
	{
		while (_indexCount + 3 >= _indices.Length)
			Array.Resize(ref _indices, _indices.Length * 2);

		_indices[_indexCount + 0] = _vertexCount + 0;
		_indices[_indexCount + 1] = _vertexCount + 1;
		_indices[_indexCount + 2] = _vertexCount + 2;

		_indexCount += 3;
		_currentBatch.elements++;
		_dirty = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushQuad()
	{
		int index = _indexCount;
		int vert = _vertexCount;

		while (index + 6 >= _indices.Length)
			Array.Resize(ref _indices, _indices.Length * 2);

		_indices[index + 0] = vert + 0;
		_indices[index + 1] = vert + 1;
		_indices[index + 2] = vert + 2;
		_indices[index + 3] = vert + 0;
		_indices[index + 4] = vert + 2;
		_indices[index + 5] = vert + 3;

		_indexCount += 6;
		_currentBatch.elements += 2;
		_dirty = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ExpandVertices(int index)
	{
		while (index >= _vertices.Length)
		{
			Array.Resize(ref _vertices, _vertices.Length * 2);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushVertex(Vector2 pos, Vector2 tex, Color col, byte mult = 0, byte wash = 0, byte fill = 255)
	{
		TransformVertex(ref _vertices[_vertexCount].pos, pos, matrixStack);
		_vertices[_vertexCount].tex = tex;
		_vertices[_vertexCount].col = col;
		_vertices[_vertexCount].mult = mult;
		_vertices[_vertexCount].wash = wash;
		_vertices[_vertexCount].fill = fill;
		_vertexCount++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushVertex(Vector2 pos, Color col, byte mult = 0, byte wash = 0, byte fill = 255)
	{
		TransformVertex(ref _vertices[_vertexCount].pos, pos, matrixStack);
		_vertices[_vertexCount].col = col;
		_vertices[_vertexCount].mult = mult;
		_vertices[_vertexCount].wash = wash;
		_vertices[_vertexCount].fill = fill;
		_vertexCount++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TransformVertex(ref Vector2 to, in Vector2 position, in Transform2D matrix)
	{
		to.x = (position.x * matrix.x.x) + (position.y * matrix.y.x) + matrix.origin.x;
		to.y = (position.x * matrix.x.y) + (position.y * matrix.y.y) + matrix.origin.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void VerticalFlipTexture(ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uv3)
	{
		uv0.y = 1 - uv0.y;
		uv1.y = 1 - uv1.y;
		uv2.y = 1 - uv2.y;
		uv3.y = 1 - uv3.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ExpandIndices(int index)
	{
		while (index >= _indices.Length)
		{
			Array.Resize(ref _indices, _indices.Length * 2);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushIndex(int index)
	{
		_indices[_indexCount] = _vertexCount + index;
		_indexCount++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushIndices(int i0, int i1, int i2)
	{
		_indices[indexCount + 0] = _vertexCount + i0;
		_indices[indexCount + 1] = _vertexCount + i1;
		_indices[indexCount + 2] = _vertexCount + i2;
		_indexCount += 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushIndices(params int[] indices)
	{
		for (int i = 0; i < indices.Length; i++)
		{
			_indices[indexCount + i] = _vertexCount + _indices[i];
		}
		_indexCount += indices.Length;
	}

	#endregion
}
