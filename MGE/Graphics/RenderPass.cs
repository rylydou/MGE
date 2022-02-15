namespace MGE.Graphics;

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

public class RenderPass
{
	public RenderTexture target;

	public RectInt? viewport;

	public RectInt? scissor;
}
