namespace MGE;

public enum IndexElementSize
{
	/// <summary>
	/// 16-bit short/ushort indices.
	/// </summary>
	SixteenBits,
	/// <summary>
	/// 32-bit int/uint indices.
	/// </summary>
	ThirtyTwoBits
}

/// <summary>
/// A Structure which describes a single Render Pass / Render Call
/// </summary>
public struct RenderPass
{
	/// <summary>
	/// Render Target
	/// </summary>
	public RenderTarget target;

	/// <summary>
	/// Render Viewport
	/// </summary>
	public RectInt? viewport;

	/// <summary>
	/// Material to use
	/// </summary>
	public Material material;

	/// <summary>
	/// Mesh to use
	/// </summary>
	public Mesh mesh;

	/// <summary>
	/// The Index to begin rendering from the Mesh
	/// </summary>
	public uint meshIndexStart;

	/// <summary>
	/// The total number of Indices to draw from the Mesh
	/// </summary>
	public uint meshIndexCount;

	/// <summary>
	/// The total number of Indices to draw from the Mesh
	/// </summary>
	public IndexElementSize indexElementSize;

	/// <summary>
	/// The total number of Instances to draw from the Mesh
	/// </summary>
	public uint meshInstanceCount;

	/// <summary>
	/// The Render State Blend Mode
	/// </summary>
	public BlendMode blendMode;

	/// <summary>
	/// The Render State Culling Mode
	/// </summary>
	public CullMode cullMode;

	/// <summary>
	/// The Render State Depth comparison Function
	/// </summary>
	public Compare depthFunction;

	/// <summary>
	/// The Render State Scissor Rectangle
	/// </summary>
	public RectInt? scissor;

	/// <summary>
	/// Creates a Render Pass based on the given mesh and material
	/// </summary>
	public RenderPass(RenderTarget target, Mesh mesh, Material material)
	{
		this.target = target;
		viewport = null;
		this.mesh = mesh;
		this.material = material;
		meshIndexStart = 0;
		meshIndexCount = mesh.indexCount;
		meshInstanceCount = mesh.instanceCount;
		scissor = null;
		blendMode = BlendMode.Normal;
		depthFunction = Compare.None;
		cullMode = CullMode.None;
		indexElementSize = IndexElementSize.ThirtyTwoBits;  // Default to 32 bits.
	}

	public void Render()
	{
		App.graphics.Render(ref this);
	}

	public void Render(Graphics graphics)
	{
		graphics.Render(ref this);
	}
}
