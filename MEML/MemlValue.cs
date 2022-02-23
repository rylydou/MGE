using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MGE
{
	/// <summary>
	/// Encapsulates a Meml Value
	/// </summary>
	public abstract class MemlValue
	{
		/// <summary>
		/// The Type of Meml Value
		/// </summary>
		public readonly MemlType type;

		/// <summary>
		/// Creates a new Meml Value of the given type
		/// </summary>
		public MemlValue(MemlType type)
		{
			this.type = type;
		}

		/// <summary>
		/// Creates a Meml Value from a File
		/// </summary>
		public static MemlValue FromFile(string path)
		{
			using var stream = File.OpenRead(path);
			using var reader = new MemlTextReader(stream);
			return reader.ReadObject();
		}

		/// <summary>
		/// Creates a Meml Value from a byte array
		/// </summary>
		public static MemlValue FromBytes(byte[] bytes)
		{
			using var stream = new MemoryStream(bytes);
			using var reader = new MemlBinaryReader(stream);
			return reader.ReadObject();
		}

		/// <summary>
		/// Creates a Meml Value from a String
		/// </summary>
		public static MemlValue FromString(string memlString)
		{
			using var reader = new MemlTextReader(new StringReader(memlString));
			return reader.ReadObject();
		}

		/// <summary>
		/// Returns the bool value of the Meml Value
		/// </summary>
		public abstract bool Bool { get; }

		/// <summary>
		/// Returns the string value of the Meml Value
		/// </summary>
		public abstract string String { get; }

		/// <summary>
		/// Returns the byte value of the Meml Value
		/// </summary>
		public abstract byte Byte { get; }

		/// <summary>
		/// Returns the sbyte value of the Meml Value
		/// </summary>
		public abstract sbyte SByte { get; }

		/// <summary>
		/// Returns the char value of the Meml Value
		/// </summary>
		public abstract char Char { get; }

		/// <summary>
		/// Returns the short value of the Meml Value
		/// </summary>
		public abstract short Short { get; }

		/// <summary>
		/// Returns the ushort value of the Meml Value
		/// </summary>
		public abstract ushort UShort { get; }

		/// <summary>
		/// Returns the int value of the Meml Value
		/// </summary>
		public abstract int Int { get; }

		/// <summary>
		/// Returns the uint value of the Meml Value
		/// </summary>
		public abstract uint UInt { get; }

		/// <summary>
		/// Returns the long value of the Meml Value
		/// </summary>
		public abstract long Long { get; }

		/// <summary>
		/// Returns the ulong value of the Meml Value
		/// </summary>
		public abstract ulong ULong { get; }

		/// <summary>
		/// Returns the float value of the Meml Value
		/// </summary>
		public abstract float Float { get; }

		/// <summary>
		/// Returns the double value of the Meml Value
		/// </summary>
		public abstract double Double { get; }

		/// <summary>
		/// Returns the decimal value of the Meml Value
		/// </summary>
		public abstract decimal Decimal { get; }

		/// <summary>
		/// Returns the bytes value of the Meml Value
		/// </summary>
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
		/// Gets the Meml Value of the given Key, if this is an Object
		/// Otherwise returns a JsonNull value
		/// </summary>
		public abstract MemlValue this[string key] { get; set; }

		/// <summary>
		/// Gets the Meml Value of the given Index, if this is an Array.
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
		public abstract object? underlyingValue { get; }

		/// <summary>
		/// Gets a unique hashed value based on the contents of the Meml Data
		/// </summary>
		// public abstract int GetHashedValue();

		/// <summary>
		/// Creates a new file with the contents of this Meml Value
		/// </summary>
		public void ToFile(string path)
		{
			using var writer = new MemlTextWriter(File.Create(path));
			writer.Meml(this);
		}

		/// <summary>
		/// Creates a byte array with the contents of this Meml Value
		/// </summary>
		public byte[] ToBytes()
		{
			using var stream = new MemoryStream();
			using var writer = new MemlBinaryWriter(stream);
			writer.Meml(this);
			return stream.ToArray();
		}

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

		public static implicit operator MemlValue(List<string> value) => new MemlArray(value);
		public static implicit operator MemlValue(string[] value) => new MemlArray(value);
		public static implicit operator MemlValue(byte[] value) => new MemlValue<byte[]>(MemlType.Binary, value);

		public static implicit operator bool(MemlValue value) => value.Bool;
		public static implicit operator string(MemlValue value) => value.String;

		public static implicit operator byte(MemlValue value) => value.Byte;
		public static implicit operator sbyte(MemlValue value) => value.SByte;
		public static implicit operator char(MemlValue value) => value.Char;
		public static implicit operator short(MemlValue value) => value.Short;
		public static implicit operator ushort(MemlValue value) => value.UShort;
		public static implicit operator int(MemlValue value) => value.Int;
		public static implicit operator uint(MemlValue value) => value.UInt;
		public static implicit operator long(MemlValue value) => value.Long;
		public static implicit operator ulong(MemlValue value) => value.ULong;

		public static implicit operator float(MemlValue value) => value.Float;
		public static implicit operator double(MemlValue value) => value.Double;
		public static implicit operator decimal(MemlValue value) => value.Decimal;

		public static implicit operator byte[](MemlValue value) => value.Bytes;

	}

	/// <summary>
	/// A Null Meml Value
	/// </summary>
	public class MemlNull : MemlValue
	{
		internal static readonly MemlNull _null = new MemlNull();
		internal static readonly byte[] _binary = new byte[0];

		public MemlNull() : base(MemlType.Null) { }

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
	/// A Meml Value with a given C# data type
	/// </summary>
	public class MemlValue<T> : MemlValue
	{
		public readonly T Value;

		public MemlValue(MemlType type, T value) : base(type)
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

		public override object? underlyingValue => Value;

		public override MemlValue this[string key]
		{
			get => MemlNull._null;
			set => throw new InvalidOperationException();
		}

		public override MemlValue this[int index]
		{
			get => MemlNull._null;
			set => throw new InvalidOperationException();
		}
	}
}
