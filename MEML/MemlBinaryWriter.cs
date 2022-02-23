using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MGE
{
	public class MemlBinaryWriter : MemlWriter, IDisposable
	{
		public enum BinaryTokens : byte
		{
			Null = 0,

			ObjectStart = 1,
			ObjectEnd = 2,
			ObjectKey = 3,

			ArrayStart = 4,
			ArrayEnd = 5,

			Boolean = 128,
			String = 129,

			Byte = 130,
			SByte = 131,
			Char = 132,
			Short = 133,
			UShort = 134,
			Int = 135,
			UInt = 136,
			Long = 137,
			ULong = 138,
			Float = 139,
			Double = 140,
			Decimal = 141,

			Binary = 255,
		}

		readonly BinaryWriter _writer;
		readonly Stack<long> _containerPositionStack = new();

		public MemlBinaryWriter(string path)
		{
			_writer = new BinaryWriter(File.Create(path), Encoding.UTF8);
		}

		public MemlBinaryWriter(Stream stream)
		{
			_writer = new BinaryWriter(stream, Encoding.UTF8);
		}

		void ContainerBegin(BinaryTokens token)
		{
			_writer.Write((byte)token);
			_containerPositionStack.Push(_writer.BaseStream.Position);
			_writer.Write((uint)0); // byte size
		}

		void ContainerEnd(BinaryTokens token)
		{
			_writer.Write((byte)token);

			var current = _writer.BaseStream.Position;
			var offset = _containerPositionStack.Pop();

			_writer.BaseStream.Seek(offset, SeekOrigin.Begin);
			_writer.Write((uint)(current - offset - 4)); // byte size (minus 4 since we want to skip the actual byte size value)
			_writer.BaseStream.Seek(current, SeekOrigin.Begin);
		}

		public override void ObjectBegin()
		{
			ContainerBegin(BinaryTokens.ObjectStart);
		}

		public override void ObjectEnd()
		{
			ContainerEnd(BinaryTokens.ObjectEnd);
		}

		public override void ArrayBegin()
		{
			ContainerBegin(BinaryTokens.ArrayStart);
		}

		public override void ArrayEnd()
		{
			ContainerEnd(BinaryTokens.ArrayEnd);
		}

		public override void Key(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new Exception("Object Key cannot be empty");

			_writer.Write((byte)BinaryTokens.ObjectKey);
			_writer.Write(name);
		}

		public override void Null()
		{
			_writer.Write((byte)BinaryTokens.Null);
		}

		public override void Value(bool value)
		{
			_writer.Write((byte)BinaryTokens.Boolean);
			_writer.Write(value);
		}

		public override void Value(byte value)
		{
			_writer.Write((byte)BinaryTokens.Byte);
			_writer.Write(value);
		}

		public override void Value(sbyte value)
		{
			_writer.Write((byte)BinaryTokens.SByte);
			_writer.Write(value);
		}

		public override void Value(char value)
		{
			_writer.Write((byte)BinaryTokens.Char);
			_writer.Write(value);
		}

		public override void Value(short value)
		{
			_writer.Write((byte)BinaryTokens.Short);
			_writer.Write(value);
		}

		public override void Value(ushort value)
		{
			_writer.Write((byte)BinaryTokens.UShort);
			_writer.Write(value);
		}

		public override void Value(int value)
		{
			_writer.Write((byte)BinaryTokens.Int);
			_writer.Write(value);
		}

		public override void Value(uint value)
		{
			_writer.Write((byte)BinaryTokens.UInt);
			_writer.Write(value);
		}

		public override void Value(long value)
		{
			_writer.Write((byte)BinaryTokens.Long);
			_writer.Write(value);
		}

		public override void Value(ulong value)
		{
			_writer.Write((byte)BinaryTokens.ULong);
			_writer.Write(value);
		}

		public override void Value(decimal value)
		{
			_writer.Write((byte)BinaryTokens.Decimal);
			_writer.Write(value);
		}

		public override void Value(float value)
		{
			_writer.Write((byte)BinaryTokens.Float);
			_writer.Write(value);
		}

		public override void Value(double value)
		{
			_writer.Write((byte)BinaryTokens.Double);
			_writer.Write(value);
		}

		public override void Value(string value)
		{
			_writer.Write((byte)BinaryTokens.String);
			_writer.Write(value ?? "");
		}

		public override void Value(ReadOnlySpan<byte> value)
		{
			_writer.Write((byte)BinaryTokens.Binary);
			_writer.Write(value.Length);
			_writer.Write(value);
		}

		public void Dispose()
		{
			_writer.Dispose();
		}
	}
}
