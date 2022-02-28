using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MEML;

public class MemlBinaryWriter : IDataWriter, IDisposable
{
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

	void ContainerBegin(StructureToken token)
	{
		_writer.Write((byte)token);
		_containerPositionStack.Push(_writer.BaseStream.Position);
		_writer.Write((uint)0); // byte size
	}

	void ContainerEnd(StructureToken token)
	{
		_writer.Write((byte)token);

		var current = _writer.BaseStream.Position;
		var offset = _containerPositionStack.Pop();

		_writer.BaseStream.Seek(offset, SeekOrigin.Begin);
		_writer.Write((uint)(current - offset - 4)); // byte size (minus 4 since we want to skip the actual byte size value)
		_writer.BaseStream.Seek(current, SeekOrigin.Begin);
	}

	public void ObjectBegin()
	{
		ContainerBegin(StructureToken.ObjectStart);
	}

	public void ObjectEnd()
	{
		ContainerEnd(StructureToken.ObjectEnd);
	}

	public void ArrayBegin()
	{
		ContainerBegin(StructureToken.ArrayStart);
	}

	public void ArrayEnd()
	{
		ContainerEnd(StructureToken.ArrayEnd);
	}

	public void Key(string name)
	{
		if (string.IsNullOrEmpty(name))
			throw new Exception("Object Key cannot be empty");

		_writer.Write((byte)StructureToken.ObjectKey);
		_writer.Write(name);
	}

	public void Null()
	{
		_writer.Write((byte)StructureToken.Null);
	}

	public void Value(bool value)
	{
		_writer.Write((byte)StructureToken.Bool);
		_writer.Write(value);
	}

	public void Value(byte value)
	{
		_writer.Write((byte)StructureToken.Byte);
		_writer.Write(value);
	}

	public void Value(sbyte value)
	{
		_writer.Write((byte)StructureToken.SByte);
		_writer.Write(value);
	}

	public void Value(char value)
	{
		_writer.Write((byte)StructureToken.Char);
		_writer.Write(value);
	}

	public void Value(short value)
	{
		_writer.Write((byte)StructureToken.Short);
		_writer.Write(value);
	}

	public void Value(ushort value)
	{
		_writer.Write((byte)StructureToken.UShort);
		_writer.Write(value);
	}

	public void Value(int value)
	{
		_writer.Write((byte)StructureToken.Int);
		_writer.Write(value);
	}

	public void Value(uint value)
	{
		_writer.Write((byte)StructureToken.UInt);
		_writer.Write(value);
	}

	public void Value(long value)
	{
		_writer.Write((byte)StructureToken.Long);
		_writer.Write(value);
	}

	public void Value(ulong value)
	{
		_writer.Write((byte)StructureToken.ULong);
		_writer.Write(value);
	}

	public void Value(decimal value)
	{
		_writer.Write((byte)StructureToken.Decimal);
		_writer.Write(value);
	}

	public void Value(float value)
	{
		_writer.Write((byte)StructureToken.Float);
		_writer.Write(value);
	}

	public void Value(double value)
	{
		_writer.Write((byte)StructureToken.Double);
		_writer.Write(value);
	}

	public void Value(string value)
	{
		_writer.Write((byte)StructureToken.String);
		_writer.Write(value ?? "");
	}

	public void Value(ReadOnlySpan<byte> value)
	{
		_writer.Write((byte)StructureToken.Binary);
		_writer.Write(value.Length);
		_writer.Write(value);
	}

	public void Dispose()
	{
		_writer.Dispose();
	}
}
