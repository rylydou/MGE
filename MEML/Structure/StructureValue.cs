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

	public abstract bool @bool { get; }
	public abstract string @string { get; }

	public abstract byte @byte { get; }
	public abstract sbyte @sbyte { get; }
	public abstract char @char { get; }
	public abstract short @short { get; }
	public abstract ushort @ushort { get; }
	public abstract int @int { get; }
	public abstract uint @uint { get; }
	public abstract long @long { get; }
	public abstract ulong @ulong { get; }

	public abstract float @float { get; }
	public abstract double @double { get; }
	public abstract decimal @decimal { get; }

	public abstract byte[] bytes { get; }

	/// <summary>
	/// Returns the Enum value, or a default value if it isn't one
	/// </summary>
	public T Enum<T>(T defaultValue = default) where T : struct, IConvertible
	{
		if (System.Enum.TryParse<T>(@string, true, out var value))
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
	public abstract object? rawValue { get; }

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

	public static implicit operator StructureValue(bool value) => new StructureValue<bool>(StructureType.Bool, value);
	public static implicit operator StructureValue(string value) => new StructureValue<string>(StructureType.String, value ?? "");

	public static implicit operator StructureValue(byte value) => new StructureValue<byte>(StructureType.Byte, value);
	public static implicit operator StructureValue(sbyte value) => new StructureValue<sbyte>(StructureType.SByte, value);
	public static implicit operator StructureValue(char value) => new StructureValue<char>(StructureType.Char, value);
	public static implicit operator StructureValue(short value) => new StructureValue<short>(StructureType.Short, value);
	public static implicit operator StructureValue(ushort value) => new StructureValue<ushort>(StructureType.UShort, value);
	public static implicit operator StructureValue(int value) => new StructureValue<int>(StructureType.Int, value);
	public static implicit operator StructureValue(uint value) => new StructureValue<uint>(StructureType.UInt, value);
	public static implicit operator StructureValue(long value) => new StructureValue<long>(StructureType.Long, value);
	public static implicit operator StructureValue(ulong value) => new StructureValue<ulong>(StructureType.ULong, value);

	public static implicit operator StructureValue(float value) => new StructureValue<float>(StructureType.Float, value);
	public static implicit operator StructureValue(double value) => new StructureValue<double>(StructureType.Double, value);
	public static implicit operator StructureValue(decimal value) => new StructureValue<decimal>(StructureType.Decimal, value);

	public static implicit operator StructureValue(byte[] value) => new StructureValue<byte[]>(StructureType.Binary, value);

	public static implicit operator bool(StructureValue value) => value.@bool;
	public static implicit operator string(StructureValue value) => value.@string;

	public static implicit operator byte(StructureValue value) => value.@byte;
	public static implicit operator sbyte(StructureValue value) => value.@sbyte;
	public static implicit operator char(StructureValue value) => value.@char;
	public static implicit operator short(StructureValue value) => value.@short;
	public static implicit operator ushort(StructureValue value) => value.@ushort;
	public static implicit operator int(StructureValue value) => value.@int;
	public static implicit operator uint(StructureValue value) => value.@uint;
	public static implicit operator long(StructureValue value) => value.@long;
	public static implicit operator ulong(StructureValue value) => value.@ulong;

	public static implicit operator float(StructureValue value) => value.@float;
	public static implicit operator double(StructureValue value) => value.@double;
	public static implicit operator decimal(StructureValue value) => value.@decimal;

	public static implicit operator byte[](StructureValue value) => value.bytes;

}

/// <summary>
/// A Null Structure Value
/// </summary>
public class StructureValueNull : StructureValue
{
	internal static readonly StructureValueNull _null = new StructureValueNull();
	internal static readonly byte[] _binary = new byte[0];

	public StructureValueNull() : base(StructureType.Null) { }

	public override bool @bool => false;
	public override string @string => string.Empty;

	public override byte @byte => 0;
	public override sbyte @sbyte => 0;
	public override char @char => (char)0;
	public override short @short => 0;
	public override ushort @ushort => 0;
	public override int @int => 0;
	public override uint @uint => 0;
	public override long @long => 0;
	public override ulong @ulong => 0;

	public override float @float => 0;
	public override double @double => 0;
	public override decimal @decimal => 0;

	public override byte[] bytes => _binary;

	public override object? rawValue => null;

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
public class StructureValue<T> : StructureValue
{
	public readonly T value;

	public StructureValue(StructureType type, T value) : base(type)
	{
		this.value = value;
	}

	public override bool @bool => (value is bool b ? b : false);
	public override string @string => Convert.ToString(rawValue)!;

	public override float @float => Convert.ToSingle(rawValue);
	public override double @double => Convert.ToDouble(rawValue);
	public override decimal @decimal => Convert.ToDecimal(rawValue);

	public override byte @byte => Convert.ToByte(rawValue);
	public override sbyte @sbyte => Convert.ToSByte(rawValue);
	public override char @char => Convert.ToChar(rawValue);
	public override short @short => Convert.ToInt16(rawValue);
	public override ushort @ushort => Convert.ToUInt16(rawValue);
	public override int @int => Convert.ToInt32(rawValue);
	public override uint @uint => Convert.ToUInt32(rawValue);
	public override long @long => Convert.ToInt64(rawValue);
	public override ulong @ulong => Convert.ToUInt64(rawValue);

	public override byte[] bytes => (byte[])(rawValue!);

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

	public override object? rawValue => value;

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
