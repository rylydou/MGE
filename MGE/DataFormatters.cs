using MEML;

namespace MGE;

internal static class DataFormatters
{
	public static Vector2 ReadVector2(StructureValue value)
	{
		return new(value[0].Float, value[1].Float);
	}

	public static StructureValue WriteVector2(Vector2 vector)
	{
		return new StructureArray(vector.x, vector.y);
	}
}
