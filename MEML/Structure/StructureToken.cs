namespace MEML;

public enum StructureToken : byte
{
	Null = 0,

	ObjectStart = 1,
	ObjectEnd = 2,
	ObjectKey = 3,

	ArrayStart = 4,
	ArrayEnd = 5,

	Bool = 128,
	String = 129,

	Byte = 130,
	SByte = 131,
	Char = 132,
	Short = 133,
	UShort = 134,
	Int = 135,
	UInt = 136,
	Long = 137,
	ULong = 138,
	Float = 139,
	Double = 140,
	Decimal = 141,

	Binary = 255,
}
