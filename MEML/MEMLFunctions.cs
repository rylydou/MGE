using System;
using System.Diagnostics.CodeAnalysis;

namespace MEML;

public static class DataReaderAndWriterFunctions
{
	/// <summary>
	/// Reads an Meml Object from the Stream and returns it
	/// </summary>
	/// <param name="into">An optional object to read into. If null, it creates a new JsonObject</param>
	public static StructureValue ReadObject(this IDataReader reader)
	{
		var result = new StructureObject();
		var opened = false;

		while (reader.Read() && reader.token != StructureToken.ObjectEnd)
		{
			if (!opened && reader.token == StructureToken.ObjectStart)
			{
				opened = true;
				continue;
			}

			if (reader.token != StructureToken.ObjectKey) throw new Exception($"Expected Object Key");

			var key = reader.value as string;
			if (string.IsNullOrEmpty(key)) throw new Exception($"Invalid Object Key");

			result[key] = reader.ReadValue();
		}

		return result;
	}

	/// <summary>
	/// Tries to read a JsonObject from the Stream
	/// </summary>
	public static bool TryReadObject(this IDataReader reader, [MaybeNullWhen(false)] out StructureValue obj)
	{
		try
		{
			obj = reader.ReadObject();
			return true;
		}
		catch { }

		// FIXME: this seems like the MaybeNullWhen attribute doesn't work?
		// #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		obj = null;
		// #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		return false;

	}

	/// <summary>
	/// Reads a JsonArray from the Stream
	/// </summary>
	public static StructureValue ReadArray(this IDataReader reader)
	{
		var arr = new StructureArray();
		while (reader.Read() && reader.token != StructureToken.ArrayEnd)
			arr.Add(reader.CurrentValue());
		return arr;
	}

	/// <summary>
	/// Reads a JsonValue from the Stream
	/// </summary>
	public static StructureValue ReadValue(this IDataReader reader)
	{
		reader.Read();
		return reader.CurrentValue();
	}

	public static StructureValue CurrentValue(this IDataReader reader)
	{
		switch (reader.token)
		{
			case StructureToken.Null: return new StructureValueNull();

			case StructureToken.Bool: return (bool)(reader.value!);
			case StructureToken.String: return (string)(reader.value!);

			case StructureToken.Byte: return (byte)(reader.value!);
			case StructureToken.SByte: return (sbyte)(reader.value!);
			case StructureToken.Char: return (char)(reader.value!);
			case StructureToken.Short: return (short)(reader.value!);
			case StructureToken.UShort: return (ushort)(reader.value!);
			case StructureToken.Int: return (int)(reader.value!);
			case StructureToken.UInt: return (uint)(reader.value!);
			case StructureToken.Long: return (long)(reader.value!);
			case StructureToken.ULong: return (ulong)(reader.value!);

			case StructureToken.Float: return (float)(reader.value!);
			case StructureToken.Double: return (double)(reader.value!);
			case StructureToken.Decimal: return (decimal)(reader.value!);

			case StructureToken.Binary: return (byte[])(reader.value!);

			case StructureToken.ObjectStart: return reader.ReadObject();

			case StructureToken.ArrayStart: return reader.ReadArray();

			case StructureToken.ObjectKey:
			case StructureToken.ObjectEnd:
			case StructureToken.ArrayEnd:
				throw new Exception($"Unexpected {reader.token}");
		}

		return new StructureValueNull();
	}

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
						if (value is StructureValue<bool> Bool)
						{
							writer.Value(Bool.Bool);
							return;
						}
						else if (value is StructureValue<decimal> Decimal)
						{
							writer.Value(Decimal.Decimal);
							return;
						}
						else if (value is StructureValue<float> Float)
						{
							writer.Value(Float.Float);
							return;
						}
						else if (value is StructureValue<double> Double)
						{
							writer.Value(Double.Double);
							return;
						}
						else if (value is StructureValue<byte> Byte)
						{
							writer.Value(Byte.Byte);
							return;
						}
						else if (value is StructureValue<char> Char)
						{
							writer.Value(Char.Char);
							return;
						}
						else if (value is StructureValue<short> Short)
						{
							writer.Value(Short.Short);
							return;
						}
						else if (value is StructureValue<ushort> UShort)
						{
							writer.Value(UShort.UShort);
							return;
						}
						else if (value is StructureValue<int> Int)
						{
							writer.Value(Int.Int);
							return;
						}
						else if (value is StructureValue<uint> UInt)
						{
							writer.Value(UInt.UInt);
							return;
						}
						else if (value is StructureValue<long> Long)
						{
							writer.Value(Long.Long);
							return;
						}
						else if (value is StructureValue<ulong> ULong)
						{
							writer.Value(ULong.ULong);
							return;
						}
					}
					break;
				case StructureType.Binary:
					if (value is StructureValue<byte[]> Bytes)
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
