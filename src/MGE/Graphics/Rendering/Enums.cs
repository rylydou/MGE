using System;

namespace MGE;

/// <summary>
/// Clear Flags
/// </summary>
[Flags]
public enum Clear
{
	None = 0,
	Color = 1,
	Depth = 2,
	Stencil = 4,
	All = 7
}

/// <summary>
/// Blend Operations
/// </summary>
public enum BlendOperations : byte
{
	Add,
	Subtract,
	ReverseSubtract,
	Min,
	Max
}

/// <summary>
/// Blend Factors
/// </summary>
public enum BlendFactors : byte
{
	Zero,
	One,
	SrcColor,
	OneMinusSrcColor,
	DstColor,
	OneMinusDstColor,
	SrcAlpha,
	OneMinusSrcAlpha,
	DstAlpha,
	OneMinusDstAlpha,
	ConstantColor,
	OneMinusConstantColor,
	ConstantAlpha,
	OneMinusConstantAlpha,
	SrcAlphaSaturate,
	Src1Color,
	OneMinusSrc1Color,
	Src1Alpha,
	OneMinusSrc1Alpha
}

public enum BlendMask : byte
{
	None = 0,
	Red = 1,
	Green = 2,
	Blue = 4,
	Alpha = 8,
	RGB = Red | Green | Blue,
	RGBA = Red | Green | Blue | Alpha,
}

/// <summary>
/// Blend Mode
/// </summary>
public struct BlendMode
{
	public BlendOperations colorOperation;
	public BlendFactors colorSource;
	public BlendFactors colorDestination;
	public BlendOperations alphaOperation;
	public BlendFactors alphaSource;
	public BlendFactors alphaDestination;
	public BlendMask mask;
	public Color color;

	public BlendMode(BlendOperations operation, BlendFactors source, BlendFactors destination)
	{
		colorOperation = alphaOperation = operation;
		colorSource = alphaSource = source;
		colorDestination = alphaDestination = destination;
		mask = BlendMask.RGBA;
		color = Color.white;
	}

	public BlendMode(
		BlendOperations colorOperation, BlendFactors colorSource, BlendFactors colorDestination,
		BlendOperations alphaOperation, BlendFactors alphaSource, BlendFactors alphaDestination,
		BlendMask mask, Color color)
	{
		this.colorOperation = colorOperation;
		this.colorSource = colorSource;
		this.colorDestination = colorDestination;
		this.alphaOperation = alphaOperation;
		this.alphaSource = alphaSource;
		this.alphaDestination = alphaDestination;
		this.mask = mask;
		this.color = color;
	}

	public static readonly BlendMode Normal = new BlendMode(BlendOperations.Add, BlendFactors.One, BlendFactors.OneMinusSrcAlpha);
	public static readonly BlendMode Add = new BlendMode(BlendOperations.Add, BlendFactors.One, BlendFactors.DstAlpha);
	public static readonly BlendMode Subtract = new BlendMode(BlendOperations.ReverseSubtract, BlendFactors.One, BlendFactors.One);
	public static readonly BlendMode Multiply = new BlendMode(BlendOperations.Add, BlendFactors.DstColor, BlendFactors.OneMinusSrcAlpha);
	public static readonly BlendMode Screen = new BlendMode(BlendOperations.Add, BlendFactors.One, BlendFactors.OneMinusSrcColor);

	public static bool operator ==(BlendMode a, BlendMode b)
	{
		return
			a.colorOperation == b.colorOperation &&
			a.colorSource == b.colorSource &&
			a.colorDestination == b.colorDestination &&
			a.alphaOperation == b.alphaOperation &&
			a.alphaSource == b.alphaSource &&
			a.alphaDestination == b.alphaDestination &&
			a.mask == b.mask &&
			a.color == b.color;
	}

	public static bool operator !=(BlendMode a, BlendMode b) => !(a == b);

	public override bool Equals(object? obj) => (obj is BlendMode mode) && (this == mode);

	public override int GetHashCode()
	{
		return HashCode.Combine(
			colorOperation,
			colorSource,
			colorDestination,
			alphaOperation,
			alphaSource,
			alphaDestination,
			mask,
			color);
	}
}

/// <summary>
/// Compare Methods used during Rendering
/// </summary>
public enum Compare
{
	/// <summary>
	/// The Comparison is ignored
	/// </summary>
	None,

	/// <summary>
	/// The Comparison always passes.
	/// </summary>
	Always,

	/// <summary>
	/// The Comparison never passes.
	/// </summary>
	Never,

	/// <summary>
	/// Passes if the value is less than the stored value.
	/// </summary>
	Less,

	/// <summary>
	/// Passes if the value is equal to the stored value.
	/// </summary>
	Equal,

	/// <summary>
	/// Passes if the value is less than or equal to the stored value.
	/// </summary>
	LessOrEqual,

	/// <summary>
	/// Passes if the value is greater than the stored value.
	/// </summary>
	Greater,

	/// <summary>
	/// Passes if the value is not equal to the stored value.
	/// </summary>
	NotEqual,

	/// <summary>
	/// Passes if the value is greater than or equal to the stored value.
	/// </summary>
	GreaterOrEqual
}

/// <summary>
/// Cull Modes
/// </summary>
public enum CullMode
{
	None = 0,
	Front = 1,
	Back = 2,
	Both = 3
}
