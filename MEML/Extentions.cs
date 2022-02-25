namespace MEML;

public static class Extentions
{
	public static void Write(this IDataWriter writer, StructureValue value)
	{
		if (value != null)
		{
			switch (value.type)
			{
				case StructureType.Object:
					writer.ObjectBegin();
					foreach (var pair in value.pairs)
					{
						writer.Key(pair.Key);
						writer.Write(pair.Value);
					}
					writer.ObjectEnd();
					return;

				case StructureType.Array:
					writer.ArrayBegin();
					foreach (var item in value.values)
						writer.Write(item);
					writer.ArrayEnd();
					return;

				case StructureType.Bool:
					writer.Value(value.Bool);
					return;

				case StructureType.String:
					writer.Value(value.String);
					return;

				default:
					{
						if (value is MemlValue<bool> Bool)
						{
							writer.Value(Bool.Bool);
							return;
						}
						else if (value is MemlValue<decimal> Decimal)
						{
							writer.Value(Decimal.Decimal);
							return;
						}
						else if (value is MemlValue<float> Float)
						{
							writer.Value(Float.Float);
							return;
						}
						else if (value is MemlValue<double> Double)
						{
							writer.Value(Double.Double);
							return;
						}
						else if (value is MemlValue<byte> Byte)
						{
							writer.Value(Byte.Byte);
							return;
						}
						else if (value is MemlValue<char> Char)
						{
							writer.Value(Char.Char);
							return;
						}
						else if (value is MemlValue<short> Short)
						{
							writer.Value(Short.Short);
							return;
						}
						else if (value is MemlValue<ushort> UShort)
						{
							writer.Value(UShort.UShort);
							return;
						}
						else if (value is MemlValue<int> Int)
						{
							writer.Value(Int.Int);
							return;
						}
						else if (value is MemlValue<uint> UInt)
						{
							writer.Value(UInt.UInt);
							return;
						}
						else if (value is MemlValue<long> Long)
						{
							writer.Value(Long.Long);
							return;
						}
						else if (value is MemlValue<ulong> ULong)
						{
							writer.Value(ULong.ULong);
							return;
						}
					}
					break;
				case StructureType.Binary:
					if (value is MemlValue<byte[]> Bytes)
					{
						writer.Value(Bytes.Bytes);
						return;
					}
					break;
			}
		}

		writer.Null();
	}
}
