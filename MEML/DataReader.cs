using System;
using System.Diagnostics.CodeAnalysis;

namespace MEML
{
	// Make an interface
	public abstract class DataReader
	{
		/// <summary>
		/// The current Token
		/// </summary>
		public StructureToken token { get; protected set; }

		/// <summary>
		/// The current Value
		/// </summary>
		public object? value { get; protected set; }

		/// <summary>
		/// Reads an Meml Object from the Stream and returns it
		/// </summary>
		/// <param name="into">An optional object to read into. If null, it creates a new JsonObject</param>
		public StructureValue ReadObject()
		{
			var result = new StructureObject();
			var opened = false;

			while (Read() && token != StructureToken.ObjectEnd)
			{
				if (!opened && token == StructureToken.ObjectStart)
				{
					opened = true;
					continue;
				}

				if (token != StructureToken.ObjectKey)
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
		public bool TryReadObject([MaybeNullWhen(false)] out StructureValue obj)
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
		public StructureValue ReadArray()
		{
			var arr = new StructureArray();
			while (Read() && token != StructureToken.ArrayEnd)
				arr.Add(CurrentValue());
			return arr;
		}

		/// <summary>
		/// Reads a JsonValue from the Stream
		/// </summary>
		public StructureValue ReadValue()
		{
			Read();
			return CurrentValue();
		}

		StructureValue CurrentValue()
		{
			switch (token)
			{
				case StructureToken.Null: return new MemlNull();

				case StructureToken.Bool: return (bool)(value!);
				case StructureToken.String: return (string)(value!);

				case StructureToken.Byte: return (byte)(value!);
				case StructureToken.SByte: return (sbyte)(value!);
				case StructureToken.Char: return (char)(value!);
				case StructureToken.Short: return (short)(value!);
				case StructureToken.UShort: return (ushort)(value!);
				case StructureToken.Int: return (int)(value!);
				case StructureToken.UInt: return (uint)(value!);
				case StructureToken.Long: return (long)(value!);
				case StructureToken.ULong: return (ulong)(value!);

				case StructureToken.Float: return (float)(value!);
				case StructureToken.Double: return (double)(value!);
				case StructureToken.Decimal: return (decimal)(value!);

				case StructureToken.Binary: return (byte[])(value!);

				case StructureToken.ObjectStart: return ReadObject();

				case StructureToken.ArrayStart: return ReadArray();

				case StructureToken.ObjectKey:
				case StructureToken.ObjectEnd:
				case StructureToken.ArrayEnd:
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
