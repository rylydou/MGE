using System;
using System.Diagnostics.CodeAnalysis;

namespace MEML;

public static class DataReaderAndWriterFunctions
{
	/// <summary>
	/// Reads an Meml Object from the Stream and returns it
	/// </summary>
	/// <param name="into">An optional object to read into. If null, it creates a new JsonObject</param>
	public static MemlValue ReadObject(this IDataReader reader, MemlObject? into = null)
	{
		var result = into ?? new MemlObject();
		var opened = false;

		while (reader.Read() && reader.token != MemlToken.ObjectEnd)
		{
			if (!opened && reader.token == MemlToken.ObjectStart)
			{
				opened = true;
				continue;
			}

			if (reader.token != MemlToken.ObjectKey) throw new Exception($"Expected Object Key");

			var key = reader.value as string;
			if (string.IsNullOrEmpty(key)) throw new Exception($"Invalid Object Key");

			result[key] = reader.ReadValue();
		}

		return result;
	}

	/// <summary>
	/// Tries to read a JsonObject from the Stream
	/// </summary>
	public static bool TryReadObject(this IDataReader reader, [MaybeNullWhen(false)] out MemlValue obj)
	{
		try
		{
			obj = reader.ReadObject();
			return true;
		}
		catch { }

		// FIXME  this seems like the MaybeNullWhen attribute doesn't work?
		// #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		obj = null;
		// #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		return false;

	}

	/// <summary>
	/// Reads a JsonArray from the Stream
	/// </summary>
	public static MemlValue ReadArray(this IDataReader reader)
	{
		var arr = new MemlArray();
		while (reader.Read() && reader.token != MemlToken.ArrayEnd)
			arr.Add(reader.CurrentValue());
		return arr;
	}

	/// <summary>
	/// Reads a JsonValue from the Stream
	/// </summary>
	public static MemlValue ReadValue(this IDataReader reader)
	{
		reader.Read();
		return reader.CurrentValue();
	}

	public static MemlValue CurrentValue(this IDataReader reader)
	{
		switch (reader.token)
		{
			case MemlToken.Null: return new MemlValueNull();

			case MemlToken.Bool: return (bool)(reader.value!);
			case MemlToken.String: return (string)(reader.value!);

			case MemlToken.Byte: return (byte)(reader.value!);
			case MemlToken.SByte: return (sbyte)(reader.value!);
			case MemlToken.Char: return (char)(reader.value!);
			case MemlToken.Short: return (short)(reader.value!);
			case MemlToken.UShort: return (ushort)(reader.value!);
			case MemlToken.Int: return (int)(reader.value!);
			case MemlToken.UInt: return (uint)(reader.value!);
			case MemlToken.Long: return (long)(reader.value!);
			case MemlToken.ULong: return (ulong)(reader.value!);

			case MemlToken.Float: return (float)(reader.value!);
			case MemlToken.Double: return (double)(reader.value!);
			case MemlToken.Decimal: return (decimal)(reader.value!);

			case MemlToken.Binary: return (byte[])(reader.value!);

			case MemlToken.ObjectStart: return reader.ReadObject();

			case MemlToken.ArrayStart: return reader.ReadArray();

			case MemlToken.ObjectKey:
			case MemlToken.ObjectEnd:
			case MemlToken.ArrayEnd:
				throw new Exception($"Unexpected {reader.token}");
		}

		return new MemlValueNull();
	}

	public static void Write(this IDataWriter writer, MemlValue value)
	{
		if (value is not null)
		{
			switch (value.type)
			{
				case MemlType.Object:
					writer.ObjectBegin();
					foreach (var pair in value.pairs)
					{
						writer.Key(pair.Key);
						writer.Write(pair.Value);
					}
					writer.ObjectEnd();
					return;

				case MemlType.Array:
					writer.ArrayBegin();
					foreach (var item in value.values)
						writer.Write(item);
					writer.ArrayEnd();
					return;

				case MemlType.Bool:
					writer.Value(value.@bool);
					return;

				case MemlType.String:
					writer.Value(value.@string);
					return;

				default:
					{
						if (value is MemlValue<bool> Bool)
						{
							writer.Value(Bool.@bool);
							return;
						}
						else if (value is MemlValue<decimal> Decimal)
						{
							writer.Value(Decimal.@decimal);
							return;
						}
						else if (value is MemlValue<float> Float)
						{
							writer.Value(Float.@float);
							return;
						}
						else if (value is MemlValue<double> Double)
						{
							writer.Value(Double.@double);
							return;
						}
						else if (value is MemlValue<byte> Byte)
						{
							writer.Value(Byte.@byte);
							return;
						}
						else if (value is MemlValue<char> Char)
						{
							writer.Value(Char.@char);
							return;
						}
						else if (value is MemlValue<short> Short)
						{
							writer.Value(Short.@short);
							return;
						}
						else if (value is MemlValue<ushort> UShort)
						{
							writer.Value(UShort.@ushort);
							return;
						}
						else if (value is MemlValue<int> Int)
						{
							writer.Value(Int.@int);
							return;
						}
						else if (value is MemlValue<uint> UInt)
						{
							writer.Value(UInt.@uint);
							return;
						}
						else if (value is MemlValue<long> Long)
						{
							writer.Value(Long.@long);
							return;
						}
						else if (value is MemlValue<ulong> ULong)
						{
							writer.Value(ULong.@ulong);
							return;
						}
					}
					break;
				case MemlType.Binary:
					if (value is MemlValue<byte[]> Bytes)
					{
						writer.Value(Bytes.bytes);
						return;
					}
					break;
			}
		}

		writer.Null();
	}
}
