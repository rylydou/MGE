using System;
using System.Collections.Generic;
using System.Linq;

namespace MEML;

public abstract class MemlValue
{
	/// <summary>
	/// The Type of Structure Value
	/// </summary>
	public readonly MemlType type;

	/// <summary>
	/// Creates a new Structure Value of the given type
	/// </summary>
	public MemlValue(MemlType type)
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
	public abstract MemlValue this[string key] { get; set; }

	/// <summary>
	/// Gets the Structure Value of the given Index, if this is an Array.
	/// Otherwise returns a JsonNull value.
	/// </summary>
	public abstract MemlValue this[int index] { get; set; }

	/// <summary>
	/// Adds a value if this is an Array.
	/// Otherwise throws an InvalidOprationException
	/// </summary>
	public abstract void Add(MemlValue value);

	/// <summary>
	/// Removes a value if this is an Array.
	/// Otherwise throws an InvalidOprationException
	/// </summary>
	public abstract void Remove(MemlValue value);

	/// <summary>
	/// Returns an Enumerable of all the Keys, or an empty list of this is not an Object
	/// </summary>
	public abstract IEnumerable<string> keys { get; }

	/// <summary>
	/// Returns an Enumerable of all the Values, or an empty list of this is not an Object or Array
	/// </summary>
	public abstract IEnumerable<MemlValue> values { get; }

	/// <summary>
	/// Returns an Enumerable of all the Keys-Value pairs, or an empty list of this is not an Object
	/// </summary>
	public abstract IEnumerable<KeyValuePair<string, MemlValue>> pairs { get; }

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

	public static implicit operator MemlValue(bool value) => new MemlValue<bool>(MemlType.Bool, value);
	public static implicit operator MemlValue(string value) => new MemlValue<string>(MemlType.String, value ?? "");

	public static implicit operator MemlValue(byte value) => new MemlValue<byte>(MemlType.Byte, value);
	public static implicit operator MemlValue(sbyte value) => new MemlValue<sbyte>(MemlType.SByte, value);
	public static implicit operator MemlValue(char value) => new MemlValue<char>(MemlType.Char, value);
	public static implicit operator MemlValue(short value) => new MemlValue<short>(MemlType.Short, value);
	public static implicit operator MemlValue(ushort value) => new MemlValue<ushort>(MemlType.UShort, value);
	public static implicit operator MemlValue(int value) => new MemlValue<int>(MemlType.Int, value);
	public static implicit operator MemlValue(uint value) => new MemlValue<uint>(MemlType.UInt, value);
	public static implicit operator MemlValue(long value) => new MemlValue<long>(MemlType.Long, value);
	public static implicit operator MemlValue(ulong value) => new MemlValue<ulong>(MemlType.ULong, value);

	public static implicit operator MemlValue(float value) => new MemlValue<float>(MemlType.Float, value);
	public static implicit operator MemlValue(double value) => new MemlValue<double>(MemlType.Double, value);
	public static implicit operator MemlValue(decimal value) => new MemlValue<decimal>(MemlType.Decimal, value);

	public static implicit operator MemlValue(byte[] value) => new MemlValue<byte[]>(MemlType.Binary, value);

	public static implicit operator bool(MemlValue value) => value.@bool;
	public static implicit operator string(MemlValue value) => value.@string;

	public static implicit operator byte(MemlValue value) => value.@byte;
	public static implicit operator sbyte(MemlValue value) => value.@sbyte;
	public static implicit operator char(MemlValue value) => value.@char;
	public static implicit operator short(MemlValue value) => value.@short;
	public static implicit operator ushort(MemlValue value) => value.@ushort;
	public static implicit operator int(MemlValue value) => value.@int;
	public static implicit operator uint(MemlValue value) => value.@uint;
	public static implicit operator long(MemlValue value) => value.@long;
	public static implicit operator ulong(MemlValue value) => value.@ulong;

	public static implicit operator float(MemlValue value) => value.@float;
	public static implicit operator double(MemlValue value) => value.@double;
	public static implicit operator decimal(MemlValue value) => value.@decimal;

	public static implicit operator byte[](MemlValue value) => value.bytes;

}

/// <summary>
/// A Null Structure Value
/// </summary>
public class MemlValueNull : MemlValue
{
	internal static readonly MemlValueNull _null = new MemlValueNull();
	internal static readonly byte[] _binary = new byte[0];

	public MemlValueNull() : base(MemlType.Null) { }

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

	public override void Add(MemlValue value)
	{
		throw new InvalidOperationException();
	}

	public override void Remove(MemlValue value)
	{
		throw new InvalidOperationException();
	}

	public override MemlValue this[string key]
	{
		get => _null;
		set => throw new InvalidOperationException();
	}

	public override MemlValue this[int index]
	{
		get => _null;
		set => throw new InvalidOperationException();
	}

	public override int count => 0;
	public override IEnumerable<string> keys => Enumerable.Empty<string>();
	public override IEnumerable<MemlValue> values => Enumerable.Empty<MemlValue>();
	public override IEnumerable<KeyValuePair<string, MemlValue>> pairs => Enumerable.Empty<KeyValuePair<string, MemlValue>>();
}

/// <summary>
/// A Structure Value with a given C# data type
/// </summary>
public class MemlValue<T> : MemlValue
{
	public readonly T value;

	public MemlValue(MemlType type, T value) : base(type)
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

	public override void Add(MemlValue value)
	{
		throw new InvalidOperationException();
	}

	public override void Remove(MemlValue value)
	{
		throw new InvalidOperationException();
	}

	public override int count => 0;
	public override IEnumerable<string> keys => Enumerable.Empty<string>();
	public override IEnumerable<MemlValue> values => Enumerable.Empty<MemlValue>();
	public override IEnumerable<KeyValuePair<string, MemlValue>> pairs => Enumerable.Empty<KeyValuePair<string, MemlValue>>();

	public override object? rawValue => value;

	public override MemlValue this[string key]
	{
		get => MemlValueNull._null;
		set => throw new InvalidOperationException();
	}

	public override MemlValue this[int index]
	{
		get => MemlValueNull._null;
		set => throw new InvalidOperationException();
	}
}
