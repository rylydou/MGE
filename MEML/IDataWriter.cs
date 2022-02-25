using System;

namespace MEML
{
	public interface IDataWriter
	{
		void Key(string name);
		void ObjectBegin();
		void ObjectEnd();
		void ArrayBegin();
		void ArrayEnd();

		void Null();
		void Value(bool value);
		void Value(byte value);
		void Value(sbyte value);
		void Value(char value);
		void Value(short value);
		void Value(ushort value);
		void Value(int value);
		void Value(uint value);
		void Value(long value);
		void Value(ulong value);
		void Value(float value);
		void Value(double value);
		void Value(decimal value);
		void Value(string value);

		void Value(byte[] value) => Value(value.AsSpan());
		void Value(ReadOnlySpan<byte> value);
	}
}
