using System;
using System.Collections.Generic;
using System.Linq;

namespace MEML;

public abstract class StructureValue
{
	/// <summary>
	/// The Type of Structure Value
	/// </summary>
	public readonly StructureType type;

	/// <summary>
	/// Creates a new Structure Value of the given type
	/// </summary>
	public StructureValue(StructureType type)
	{
		this.type = type;
	}

	public abstract bool Bool { get; }
	public abstract string String { get; }

	public abstract byte Byte { get; }
	public abstract sbyte SByte { get; }
	public abstract char Char { get; }
	public abstract short Short { get; }
	public abstract ushort UShort { get; }
	public abstract int Int { get; }
	public abstract uint UInt { get; }
	public abstract long Long { get; }
	public abstract ulong ULong { get; }

	public abstract float Float { get; }
	public abstract double Double { get; }
	public abstract decimal Decimal { get; }

	public abstract byte[] Bytes { get; }

	/// <summary>
	/// Returns the Enum value, or a default value if it isn't one
	/// </summary>
	public T Enum<T>(T defaultValue = default) where T : struct, IConvertible
	{
		if (System.Enum.TryParse<T>(String, true, out var value))
			return value;
		return defaultValue;
	}

	/// <summary>
	/// Gets the Structure Value of the given Key, if this is an Object
	/// Otherwise returns a StructureNull value
	/// </summary>
	public abstract StructureValue this[string key] { get; set; }

	/// <summary>
	/// Gets the Structure Value of the given Index, if this is an Array.
	/// Otherwise returns a JsonNull value.
	/// </summary>
	public abstract StructureValue this[int index] { get; set; }

	/// <summary>
	/// Adds a value if this is an Array.
	/// Otherwise throws an InvalidOprationException
	/// </summary>
	public abstract void Add(StructureValue value);

	/// <summary>
	/// Removes a value if this is an Array.
	/// Otherwise throws an InvalidOprationException
	/// </summary>
	public abstract void Remove(StructureValue value);

	/// <summary>
	/// Returns an Enumerable of all the Keys, or an empty list of this is not an Object
	/// </summary>
	public abstract IEnumerable<string> keys { get; }

	/// <summary>
	/// Returns an Enumerable of all the Values, or an empty list of this is not an Object or Array
	/// </summary>
	public abstract IEnumerable<StructureValue> values { get; }

	/// <summary>
	/// Returns an Enumerable of all the Keys-Value pairs, or an empty list of this is not an Object
	/// </summary>
	public abstract IEnumerable<KeyValuePair<string, StructureValue>> pairs { get; }

	/// <summary>
	/// Returns the total amount of Array Entries or Key-Value Pairs
	/// </summary>
	public abstract int count { get; }

	/// <summary>
	/// The underlying C# Object
	/// </summary>
	public abstract object? underlyingValue { get; }

	/// <summary>
	/// Gets a unique hashed value based on the contents of the Structure Data
	/// </summary>
	// public abstract int GetHashedValue();

	// /// <summary>
	// /// Creates a new file with the contents of this Structure Value
	// /// </summary>
	// public void ToFile(string path)
	// {
	// 	using var writer = new MemlTextWriter(File.Create(path));
	// 	writer.Structure(this);
	// }

	// /// <summary>
	// /// Creates a byte array with the contents of this Structure Value
	// /// </summary>
	// public byte[] ToBytes()
	// {
	// 	using var stream = new MemoryStream();
	// 	using var writer = new MemlBinaryWriter(stream);
	// 	writer.Structure(this);
	// 	return stream.ToArray();
	// }

	public static implicit operator StructureValue(bool value) => new MemlValue<bool>(StructureType.Bool, value);
	public static implicit operator StructureValue(string value) => new MemlValue<string>(StructureType.String, value ?? "");

	public static implicit operator StructureValue(byte value) => new MemlValue<byte>(StructureType.Byte, value);
	public static implicit operator StructureValue(sbyte value) => new MemlValue<sbyte>(StructureType.SByte, value);
	public static implicit operator StructureValue(char value) => new MemlValue<char>(StructureType.Char, value);
	public static implicit operator StructureValue(short value) => new MemlValue<short>(StructureType.Short, value);
	public static implicit operator StructureValue(ushort value) => new MemlValue<ushort>(StructureType.UShort, value);
	public static implicit operator StructureValue(int value) => new MemlValue<int>(StructureType.Int, value);
	public static implicit operator StructureValue(uint value) => new MemlValue<uint>(StructureType.UInt, value);
	public static implicit operator StructureValue(long value) => new MemlValue<long>(StructureType.Long, value);
	public static implicit operator StructureValue(ulong value) => new MemlValue<ulong>(StructureType.ULong, value);

	public static implicit operator StructureValue(float value) => new MemlValue<float>(StructureType.Float, value);
	public static implicit operator StructureValue(double value) => new MemlValue<double>(StructureType.Double, value);
	public static implicit operator StructureValue(decimal value) => new MemlValue<decimal>(StructureType.Decimal, value);

	public static implicit operator StructureValue(byte[] value) => new MemlValue<byte[]>(StructureType.Binary, value);

	public static implicit operator bool(StructureValue value) => value.Bool;
	public static implicit operator string(StructureValue value) => value.String;

	public static implicit operator byte(StructureValue value) => value.Byte;
	public static implicit operator sbyte(StructureValue value) => value.SByte;
	public static implicit operator char(StructureValue value) => value.Char;
	public static implicit operator short(StructureValue value) => value.Short;
	public static implicit operator ushort(StructureValue value) => value.UShort;
	public static implicit operator int(StructureValue value) => value.Int;
	public static implicit operator uint(StructureValue value) => value.UInt;
	public static implicit operator long(StructureValue value) => value.Long;
	public static implicit operator ulong(StructureValue value) => value.ULong;

	public static implicit operator float(StructureValue value) => value.Float;
	public static implicit operator double(StructureValue value) => value.Double;
	public static implicit operator decimal(StructureValue value) => value.Decimal;

	public static implicit operator byte[](StructureValue value) => value.Bytes;

}

/// <summary>
/// A Null Structure Value
/// </summary>
public class StructureValueNull : StructureValue
{
	internal static readonly StructureValueNull _null = new StructureValueNull();
	internal static readonly byte[] _binary = new byte[0];

	public StructureValueNull() : base(StructureType.Null) { }

	public override bool Bool => false;
	public override string String => string.Empty;

	public override byte Byte => 0;
	public override sbyte SByte => 0;
	public override char Char => (char)0;
	public override short Short => 0;
	public override ushort UShort => 0;
	public override int Int => 0;
	public override uint UInt => 0;
	public override long Long => 0;
	public override ulong ULong => 0;

	public override float Float => 0;
	public override double Double => 0;
	public override decimal Decimal => 0;

	public override byte[] Bytes => _binary;

	public override object? underlyingValue => null;

	public override void Add(StructureValue value)
	{
		throw new InvalidOperationException();
	}

	public override void Remove(StructureValue value)
	{
		throw new InvalidOperationException();
	}

	public override StructureValue this[string key]
	{
		get => _null;
		set => throw new InvalidOperationException();
	}

	public override StructureValue this[int index]
	{
		get => _null;
		set => throw new InvalidOperationException();
	}

	public override int count => 0;
	public override IEnumerable<string> keys => Enumerable.Empty<string>();
	public override IEnumerable<StructureValue> values => Enumerable.Empty<StructureValue>();
	public override IEnumerable<KeyValuePair<string, StructureValue>> pairs => Enumerable.Empty<KeyValuePair<string, StructureValue>>();
}

/// <summary>
/// A Structure Value with a given C# data type
/// </summary>
public class MemlValue<T> : StructureValue
{
	public readonly T Value;

	public MemlValue(StructureType type, T value) : base(type)
	{
		Value = value;
	}

	public override bool Bool => (Value is bool value ? value : false);
	public override string String { get => (string)(underlyingValue ?? throw new Exception()); }

	public override float Float { get => (float)(underlyingValue ?? throw new Exception()); }
	public override double Double { get => (double)(underlyingValue ?? throw new Exception()); }
	public override decimal Decimal { get => (decimal)(underlyingValue ?? throw new Exception()); }

	public override byte Byte { get => (byte)(underlyingValue ?? throw new Exception()); }
	public override sbyte SByte { get => (sbyte)(underlyingValue ?? throw new Exception()); }
	public override char Char { get => (char)(underlyingValue ?? throw new Exception()); }
	public override short Short { get => (short)(underlyingValue ?? throw new Exception()); }
	public override ushort UShort { get => (ushort)(underlyingValue ?? throw new Exception()); }
	public override int Int { get => (int)(underlyingValue ?? throw new Exception()); }
	public override uint UInt { get => (uint)(underlyingValue ?? throw new Exception()); }
	public override long Long { get => (long)(underlyingValue ?? throw new Exception()); }
	public override ulong ULong { get => (ulong)(underlyingValue ?? throw new Exception()); }

	public override byte[] Bytes { get => (byte[])(underlyingValue ?? throw new Exception()); }

	public override void Add(StructureValue value)
	{
		throw new InvalidOperationException();
	}

	public override void Remove(StructureValue value)
	{
		throw new InvalidOperationException();
	}

	public override int count => 0;
	public override IEnumerable<string> keys => Enumerable.Empty<string>();
	public override IEnumerable<StructureValue> values => Enumerable.Empty<StructureValue>();
	public override IEnumerable<KeyValuePair<string, StructureValue>> pairs => Enumerable.Empty<KeyValuePair<string, StructureValue>>();

	public override object? underlyingValue => Value;

	public override StructureValue this[string key]
	{
		get => StructureValueNull._null;
		set => throw new InvalidOperationException();
	}

	public override StructureValue this[int index]
	{
		get => StructureValueNull._null;
		set => throw new InvalidOperationException();
	}
}
