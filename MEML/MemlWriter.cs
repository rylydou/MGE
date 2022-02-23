using System;

namespace MGE
{
	/// <summary>
	/// Writes Meml to a string format
	/// </summary>
	public abstract class MemlWriter
	{
		public abstract void Key(string name);
		public abstract void ObjectBegin();
		public abstract void ObjectEnd();
		public abstract void ArrayBegin();
		public abstract void ArrayEnd();

		public abstract void Null();
		public abstract void Value(bool value);
		public abstract void Value(byte value);
		public abstract void Value(sbyte value);
		public abstract void Value(char value);
		public abstract void Value(short value);
		public abstract void Value(ushort value);
		public abstract void Value(int value);
		public abstract void Value(uint value);
		public abstract void Value(long value);
		public abstract void Value(ulong value);
		public abstract void Value(float value);
		public abstract void Value(double value);
		public abstract void Value(decimal value);
		public abstract void Value(string value);

		public void Value(byte[] value) => Value(value.AsSpan());
		public abstract void Value(ReadOnlySpan<byte> value);

		public void Meml(MemlValue value)
		{
			if (value != null)
			{
				switch (value.type)
				{
					case MemlType.Object:
						ObjectBegin();
						foreach (var pair in value.pairs)
						{
							Key(pair.Key);
							Meml(pair.Value);
						}
						ObjectEnd();
						return;

					case MemlType.Array:
						ArrayBegin();
						foreach (var item in value.values)
							Meml(item);
						ArrayEnd();
						return;

					case MemlType.Bool:
						Value(value.Bool);
						return;

					case MemlType.String:
						Value(value.String);
						return;

					default:
						{
							if (value is MemlValue<bool> Bool)
							{
								Value(Bool.Bool);
								return;
							}
							else if (value is MemlValue<decimal> Decimal)
							{
								Value(Decimal.Decimal);
								return;
							}
							else if (value is MemlValue<float> Float)
							{
								Value(Float.Float);
								return;
							}
							else if (value is MemlValue<double> Double)
							{
								Value(Double.Double);
								return;
							}
							else if (value is MemlValue<byte> Byte)
							{
								Value(Byte.Byte);
								return;
							}
							else if (value is MemlValue<char> Char)
							{
								Value(Char.Char);
								return;
							}
							else if (value is MemlValue<short> Short)
							{
								Value(Short.Short);
								return;
							}
							else if (value is MemlValue<ushort> UShort)
							{
								Value(UShort.UShort);
								return;
							}
							else if (value is MemlValue<int> Int)
							{
								Value(Int.Int);
								return;
							}
							else if (value is MemlValue<uint> UInt)
							{
								Value(UInt.UInt);
								return;
							}
							else if (value is MemlValue<long> Long)
							{
								Value(Long.Long);
								return;
							}
							else if (value is MemlValue<ulong> ULong)
							{
								Value(ULong.ULong);
								return;
							}
						}
						break;
					case MemlType.Binary:
						if (value is MemlValue<byte[]> Bytes)
						{
							Value(Bytes.Bytes);
							return;
						}
						break;
				}
			}

			Null();
		}
	}
}
