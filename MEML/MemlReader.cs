using System;
using System.Diagnostics.CodeAnalysis;

namespace MGE
{
	/// <summary>
	/// Reads JSON from a Stream or Path
	/// </summary>
	public abstract class MemlReader
	{
		/// <summary>
		/// The current Token
		/// </summary>
		public MemlToken token { get; protected set; }

		/// <summary>
		/// The current Value
		/// </summary>
		public object? value { get; protected set; }

		/// <summary>
		/// The current Position in the Stream
		/// </summary>
		public abstract long position { get; }

		/// <summary>
		/// Reads an Meml Object from the Stream and returns it
		/// </summary>
		/// <param name="into">An optional object to read into. If null, it creates a new JsonObject</param>
		public MemlValue ReadObject()
		{
			var result = new MemlObject();
			var opened = false;

			while (Read() && token != MemlToken.ObjectEnd)
			{
				if (!opened && token == MemlToken.ObjectStart)
				{
					opened = true;
					continue;
				}

				if (token != MemlToken.ObjectKey)
					throw new Exception($"Expected Object Key");

				var key = value as string;
				if (string.IsNullOrEmpty(key))
					throw new Exception($"Invalid Object Key");

				result[key] = ReadValue();
			}

			return result;
		}

		/// <summary>
		/// Tries to read a JsonObject from the Stream
		/// </summary>
		public bool TryReadObject([MaybeNullWhen(false)] out MemlValue obj)
		{
			try
			{
				obj = ReadObject();
			}
			catch
			{
				// FIXME: this seems like the MaybeNullWhen attribute doesn't work?
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
				obj = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reads a JsonArray from the Stream
		/// </summary>
		public MemlValue ReadArray()
		{
			var arr = new MemlArray();
			while (Read() && token != MemlToken.ArrayEnd)
				arr.Add(CurrentValue());
			return arr;
		}

		/// <summary>
		/// Reads a JsonValue from the Stream
		/// </summary>
		public MemlValue ReadValue()
		{
			Read();
			return CurrentValue();
		}

		MemlValue CurrentValue()
		{
			switch (token)
			{
				case MemlToken.Null:
					return new MemlNull();

				case MemlToken.Boolean:
					if (value is bool Bool)
						return Bool;
					break;

				case MemlToken.Number:
					if (value is byte Byte)
						return Byte;
					if (value is char Char)
						return Char;
					if (value is short Short)
						return Short;
					if (value is ushort UShort)
						return UShort;
					if (value is int Int)
						return Int;
					if (value is uint UInt)
						return UInt;
					if (value is long Long)
						return Long;
					if (value is ulong ULong)
						return ULong;
					if (value is decimal Decimal)
						return Decimal;
					if (value is float Float)
						return Float;
					if (value is double Double)
						return Double;
					break;

				case MemlToken.String:
					if (value is string String)
						return String;
					break;

				case MemlToken.Binary:
					if (value is byte[] Bytes)
						return Bytes;
					break;

				case MemlToken.ObjectStart:
					return ReadObject();

				case MemlToken.ArrayStart:
					return ReadArray();

				case MemlToken.ObjectKey:
				case MemlToken.ObjectEnd:
				case MemlToken.ArrayEnd:
					throw new Exception($"Unexpected {token}");
			}

			return new MemlNull();
		}

		/// <summary>
		/// Skips the current Value
		/// </summary>
		public virtual void SkipValue()
		{
			ReadValue();
		}

		/// <summary>
		/// Reads the next Token in the Stream
		/// </summary>
		public abstract bool Read();
	}
}
