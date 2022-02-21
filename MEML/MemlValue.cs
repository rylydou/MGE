using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

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

		public static MemlValue FromObject(object? obj, Type? impliedType = null)
		{
			if (obj is null) return new MemlNull();

			var type = obj.GetType();

			if (type.IsPrimitive)
			{
				if (obj is float @float) return new MemlValue<float>(MemlType.Number, @float);
				if (obj is double @double) return new MemlValue<double>(MemlType.Number, @double);
				if (obj is byte @byte) return new MemlValue<byte>(MemlType.Number, @byte);
				if (obj is char @char) return new MemlValue<char>(MemlType.Number, @char);
				if (obj is short @short) return new MemlValue<short>(MemlType.Number, @short);
				if (obj is ushort @ushort) return new MemlValue<ushort>(MemlType.Number, @ushort);
				if (obj is int @int) return new MemlValue<int>(MemlType.Number, @int);
				if (obj is uint @uint) return new MemlValue<uint>(MemlType.Number, @uint);
				if (obj is long @long) return new MemlValue<long>(MemlType.Number, @long);
				if (obj is ulong @ulong) return new MemlValue<ulong>(MemlType.Number, @ulong);

				if (obj is bool @bool) return new MemlValue<bool>(MemlType.Bool, @bool);
			}

			if (obj is string @string) return new MemlValue<string>(MemlType.String, @string);

			if (obj is byte[] binary) return new MemlValue<byte[]>(MemlType.Binary, binary);

			if (obj is ICollection collection)
			{
				var memlArray = new MemlArray();
				var contentType = type.IsArray ? type.GetElementType() : GetCollectionElementType(type);

				foreach (var item in collection)
				{
					memlArray.Add(FromObject(item, contentType));
				}

				return memlArray;
			}

			var memlObject = new MemlObject();

			if (type != impliedType)
			{
				memlObject["!"] = type.FullName;
			}

			var members = type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty);
			foreach (var member in members)
			{
				object? varibleValue;
				Type varibleType;

				if (member is FieldInfo fieldInfo)
				{
					varibleValue = fieldInfo.GetValue(obj);
					varibleType = fieldInfo.FieldType;
				}
				else if (member is PropertyInfo propInfo)
				{
					varibleValue = propInfo.GetValue(obj);
					varibleType = propInfo.PropertyType;
				}
				else continue;

				var memlValue = MemlValue.FromObject(varibleValue, varibleType);
				memlObject[member.Name] = memlValue;
			}

			return memlObject;
		}

		static Type? GetCollectionElementType(Type type)
		{
			var etype = typeof(ICollection<>);
			foreach (var bt in type.GetInterfaces())
				if (bt.IsGenericType && bt.GetGenericTypeDefinition() == etype)
					return bt.GetGenericArguments()[0];

			return null;
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
		public static MemlValue FromString(string jsonString)
		{
			using var reader = new MemlTextReader(new StringReader(jsonString));
			return reader.ReadObject();
		}

		/// <summary>
		/// Returns true if the Meml value is Null
		/// </summary>
		public bool IsNull => type == MemlType.Null;

		/// <summary>
		/// Returns true if the Meml Value is a Bool
		/// </summary>
		public bool IsBool => type == MemlType.Bool;

		/// <summary>
		/// Returns true if the Meml Value is a Number
		/// </summary>
		public bool IsNumber => type == MemlType.Number;

		/// <summary>
		/// Returns true if the Meml Value is a String
		/// </summary>
		public bool IsString => type == MemlType.String;

		/// <summary>
		/// Returns true if the Meml Value is an Object
		/// </summary>
		public bool IsObject => type == MemlType.Object;

		/// <summary>
		/// Returns true if the Meml Value is an Array
		/// </summary>
		public bool IsArray => type == MemlType.Array;

		/// <summary>
		/// Returns true if the Meml Value is a Binary Data
		/// </summary>
		public bool IsBinary => type == MemlType.Binary;

		/// <summary>
		/// Returns the bool value of the Meml Value
		/// </summary>
		public abstract bool Bool { get; }

		/// <summary>
		/// Returns the byte value of the Meml Value
		/// </summary>
		public abstract byte Byte { get; }

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
		/// Returns the decimal value of the Meml Value
		/// </summary>
		public abstract decimal Decimal { get; }

		/// <summary>
		/// Returns the float value of the Meml Value
		/// </summary>
		public abstract float Float { get; }

		/// <summary>
		/// Returns the double value of the Meml Value
		/// </summary>
		public abstract double Double { get; }

		/// <summary>
		/// Returns the string value of the Meml Value
		/// </summary>
		public abstract string String { get; }

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
		/// Returns the bool value, or a default value if it isn't one
		/// </summary>
		public bool BoolOrDefault(bool defaultValue) => IsBool ? Bool : defaultValue;

		/// <summary>
		/// Returns the byte value, or a default value if it isn't one
		/// </summary>
		public byte ByteOrDefault(byte defaultValue) => IsNumber ? Byte : defaultValue;

		/// <summary>
		/// Returns the char value, or a default value if it isn't one
		/// </summary>
		public char CharOrDefault(char defaultValue) => IsNumber ? Char : defaultValue;

		/// <summary>
		/// Returns the short value, or a default value if it isn't one
		/// </summary>
		public short ShortOrDefault(short defaultValue) => IsNumber ? Short : defaultValue;

		/// <summary>
		/// Returns the ushort value, or a default value if it isn't one
		/// </summary>
		public ushort UShortOrDefault(ushort defaultValue) => IsNumber ? UShort : defaultValue;

		/// <summary>
		/// Returns the int value, or a default value if it isn't one
		/// </summary>
		public int IntOrDefault(int defaultValue) => IsNumber ? Int : defaultValue;

		/// <summary>
		/// Returns the uint value, or a default value if it isn't one
		/// </summary>
		public uint UIntOrDefault(uint defaultValue) => IsNumber ? UInt : defaultValue;

		/// <summary>
		/// Returns the long value, or a default value if it isn't one
		/// </summary>
		public long LongOrDefault(long defaultValue) => IsNumber ? Long : defaultValue;

		/// <summary>
		/// Returns the ulong value, or a default value if it isn't one
		/// </summary>
		public ulong ULongOrDefault(ulong defaultValue) => IsNumber ? ULong : defaultValue;

		/// <summary>
		/// Returns the decimal value, or a default value if it isn't one
		/// </summary>
		public decimal DecimalOrDefault(decimal defaultValue) => IsNumber ? Decimal : defaultValue;

		/// <summary>
		/// Returns the float value, or a default value if it isn't one
		/// </summary>
		public float FloatOrDefault(float defaultValue) => IsNumber ? Float : defaultValue;

		/// <summary>
		/// Returns the double value, or a default value if it isn't one
		/// </summary>
		public double DoubleOrDefault(double defaultValue) => IsNumber ? Double : defaultValue;

		/// <summary>
		/// Returns the string value, or a default value if it isn't one
		/// </summary>
		public string StringOrDefault(string defaultValue) => IsString ? String : defaultValue;

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
		public abstract IEnumerable<string> Keys { get; }

		/// <summary>
		/// Returns an Enumerable of all the Values, or an empty list of this is not an Object or Array
		/// </summary>
		public abstract IEnumerable<MemlValue> Values { get; }

		/// <summary>
		/// Returns an Enumerable of all the Keys-Value pairs, or an empty list of this is not an Object
		/// </summary>
		public abstract IEnumerable<KeyValuePair<string, MemlValue>> Pairs { get; }

		/// <summary>
		/// Returns the total amount of Array Entries or Key-Value Pairs
		/// </summary>
		public abstract int Count { get; }

		/// <summary>
		/// The underlying C# Object
		/// </summary>
		public abstract object? UnderlyingValue { get; }

		/// <summary>
		/// Gets a unique hashed value based on the contents of the Meml Data
		/// </summary>
		// public abstract int GetHashedValue();

		/// <summary>
		/// Clones the Meml Value
		/// </summary>
		/// <returns></returns>
		public abstract MemlValue Clone();

		/// <summary>
		/// Creates a new file with the contents of this Meml Value
		/// </summary>
		public void ToFile(string path, bool asJson = false)
		{
			using var writer = new MemlTextWriter(File.Create(path), asJson);
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
		public static implicit operator MemlValue(decimal value) => new MemlValue<decimal>(MemlType.Number, value);
		public static implicit operator MemlValue(float value) => new MemlValue<float>(MemlType.Number, value);
		public static implicit operator MemlValue(double value) => new MemlValue<double>(MemlType.Number, value);
		public static implicit operator MemlValue(byte value) => new MemlValue<byte>(MemlType.Number, value);
		public static implicit operator MemlValue(char value) => new MemlValue<char>(MemlType.Number, value);
		public static implicit operator MemlValue(short value) => new MemlValue<short>(MemlType.Number, value);
		public static implicit operator MemlValue(ushort value) => new MemlValue<ushort>(MemlType.Number, value);
		public static implicit operator MemlValue(int value) => new MemlValue<int>(MemlType.Number, value);
		public static implicit operator MemlValue(uint value) => new MemlValue<uint>(MemlType.Number, value);
		public static implicit operator MemlValue(long value) => new MemlValue<long>(MemlType.Number, value);
		public static implicit operator MemlValue(ulong value) => new MemlValue<ulong>(MemlType.Number, value);
		public static implicit operator MemlValue(string? value) => new MemlValue<string>(MemlType.String, value ?? "");
		public static implicit operator MemlValue(List<string> value) => new MemlArray(value);
		public static implicit operator MemlValue(string[] value) => new MemlArray(value);
		public static implicit operator MemlValue(byte[] value) => new MemlValue<byte[]>(MemlType.Binary, value);

		public static implicit operator bool(MemlValue value) => value.Bool;
		public static implicit operator float(MemlValue value) => value.Float;
		public static implicit operator double(MemlValue value) => value.Double;
		public static implicit operator byte(MemlValue value) => value.Byte;
		public static implicit operator char(MemlValue value) => value.Char;
		public static implicit operator short(MemlValue value) => value.Short;
		public static implicit operator ushort(MemlValue value) => value.UShort;
		public static implicit operator int(MemlValue value) => value.Int;
		public static implicit operator uint(MemlValue value) => value.UInt;
		public static implicit operator long(MemlValue value) => value.Long;
		public static implicit operator ulong(MemlValue value) => value.ULong;
		public static implicit operator string(MemlValue value) => value.String;
		public static implicit operator byte[](MemlValue value) => value.Bytes;

	}

	/// <summary>
	/// A Null Meml Value
	/// </summary>
	public class MemlNull : MemlValue
	{
		internal static readonly MemlNull _null = new MemlNull();
		internal static readonly byte[] _binary = new byte[0];

		public MemlNull() : base(MemlType.Null)
		{

		}

		public override bool Bool => false;
		public override decimal Decimal => 0;
		public override float Float => 0;
		public override double Double => 0;
		public override byte Byte => 0;
		public override char Char => (char)0;
		public override short Short => 0;
		public override ushort UShort => 0;
		public override int Int => 0;
		public override uint UInt => 0;
		public override long Long => 0;
		public override ulong ULong => 0;
		public override string String => string.Empty;
		public override byte[] Bytes => _binary;
		public override object? UnderlyingValue => null;
		// public override int GetHashedValue() => 0;

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

		public override MemlValue Clone()
		{
			return _null;
		}

		public override int Count => 0;
		public override IEnumerable<string> Keys => Enumerable.Empty<string>();
		public override IEnumerable<MemlValue> Values => Enumerable.Empty<MemlValue>();
		public override IEnumerable<KeyValuePair<string, MemlValue>> Pairs => Enumerable.Empty<KeyValuePair<string, MemlValue>>();
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

		public override decimal Decimal
		{
			get
			{
				if (IsNumber)
				{
					if (Value is decimal value)
						return value;
					return Convert.ToDecimal(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && decimal.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override float Float
		{
			get
			{
				if (IsNumber)
				{
					if (Value is float value)
						return value;
					return Convert.ToSingle(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && float.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override double Double
		{
			get
			{
				if (IsNumber)
				{
					if (Value is double value)
						return value;
					return Convert.ToDouble(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && double.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override short Short
		{
			get
			{
				if (IsNumber)
				{
					if (Value is short value)
						return value;
					return Convert.ToInt16(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && short.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override byte Byte
		{
			get
			{
				if (IsNumber)
				{
					if (Value is byte value)
						return value;
					return Convert.ToByte(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && byte.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override char Char
		{
			get
			{
				if (IsNumber)
				{
					if (Value is char value)
						return value;
					return Convert.ToChar(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && char.TryParse(value, out var n))
					return n;

				return (char)0;
			}
		}

		public override ushort UShort
		{
			get
			{
				if (IsNumber)
				{
					if (Value is ushort value)
						return value;
					return Convert.ToUInt16(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && ushort.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override int Int
		{
			get
			{
				if (IsNumber)
				{
					if (Value is int value)
						return value;
					return Convert.ToInt32(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && int.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override uint UInt
		{
			get
			{
				if (IsNumber)
				{
					if (Value is uint value)
						return value;
					return Convert.ToUInt32(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && uint.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override long Long
		{
			get
			{
				if (IsNumber)
				{
					if (Value is long value)
						return value;
					return Convert.ToInt64(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && long.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override ulong ULong
		{
			get
			{
				if (IsNumber)
				{
					if (Value is ulong value)
						return value;
					return Convert.ToUInt64(Value, NumberFormatInfo.InvariantInfo);
				}
				else if (IsString && Value is string value && ulong.TryParse(value, out var n))
					return n;

				return 0;
			}
		}

		public override string String
		{
			get
			{
				if (IsString && Value is string str)
					return str;
				else if (Value != null)
					return Value.ToString() ?? "";
				return "";
			}
		}

		public override byte[] Bytes
		{
			get
			{
				if (IsBinary && Value is byte[] bytes)
					return bytes;
				return MemlNull._binary;
			}
		}

		public override void Add(MemlValue value)
		{
			throw new InvalidOperationException();
		}

		public override void Remove(MemlValue value)
		{
			throw new InvalidOperationException();
		}

		public override int Count => 0;
		public override IEnumerable<string> Keys => Enumerable.Empty<string>();
		public override IEnumerable<MemlValue> Values => Enumerable.Empty<MemlValue>();
		public override IEnumerable<KeyValuePair<string, MemlValue>> Pairs => Enumerable.Empty<KeyValuePair<string, MemlValue>>();

		public override object? UnderlyingValue => Value;

		// public override int GetHashedValue()
		// {
		// 	if (IsString)
		// 		return Calc.StaticStringHash(String);

		// 	if (IsNumber)
		// 		return Int;

		// 	if (IsBool)
		// 		return (Bool ? 1 : 0);

		// 	if (IsBinary)
		// 		return (int)Calc.Adler32(0, Bytes);

		// 	return 0;
		// }

		public override MemlValue Clone()
		{
			if (IsString)
				return String;

			if (IsNumber)
			{
				if (Value is float Float)
					return Float;
				if (Value is double Double)
					return Double;
				if (Value is byte Byte)
					return Byte;
				if (Value is char Char)
					return Char;
				if (Value is short Short)
					return Short;
				if (Value is ushort UShort)
					return UShort;
				if (Value is int Int)
					return Int;
				if (Value is uint UInt)
					return UInt;
				if (Value is long Long)
					return Long;
				if (Value is ulong ULong)
					return ULong;
			}

			if (IsBool)
				return Bool;

			if (IsBinary)
			{
				var bytes = new byte[Bytes.Length];
				System.Array.Copy(Bytes, 0, bytes, 0, bytes.Length);
				return bytes;
			}

			return MemlNull._null;
		}

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
