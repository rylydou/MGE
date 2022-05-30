using System;
using System.IO;

namespace MEML;

public class MemlBinaryReader : IMemlReader, IDisposable
{
	readonly BinaryReader _reader;
	readonly bool _disposeStream = true;
	uint _objectSize;

	public MemlToken token { get; private set; }
	public object? value { get; private set; }

	public MemlBinaryReader(string path) : this(File.OpenRead(path), true) { }

	public MemlBinaryReader(Stream stream, bool disposeStream = true) : this(new BinaryReader(stream), disposeStream) { }

	public MemlBinaryReader(BinaryReader reader, bool disposeStream = true)
	{
		this._reader = reader;
		this._disposeStream = disposeStream;
	}

	public bool Read()
	{
		if (_reader.BaseStream.Position < _reader.BaseStream.Length)
		{
			var token = (MemlToken)_reader.ReadByte();

			switch (token)
			{
				case MemlToken.Null:
					value = null;
					token = MemlToken.Null;
					break;
				case MemlToken.ObjectStart:
					_objectSize = _reader.ReadUInt32(); // skip byte size
					value = null;
					token = MemlToken.ObjectStart;
					break;
				case MemlToken.ObjectEnd:
					value = null;
					token = MemlToken.ObjectEnd;
					break;
				case MemlToken.ObjectKey:
					value = _reader.ReadString();
					token = MemlToken.ObjectKey;
					break;
				case MemlToken.ArrayStart:
					_objectSize = _reader.ReadUInt32(); // skip byte size
					value = null;
					token = MemlToken.ArrayStart;
					break;
				case MemlToken.ArrayEnd:
					value = null;
					token = MemlToken.ArrayEnd;
					break;
				case MemlToken.Bool:
					value = _reader.ReadBoolean();
					token = MemlToken.Bool;
					break;
				case MemlToken.String:
					value = _reader.ReadString();
					token = MemlToken.String;
					break;
				case MemlToken.Byte:
					value = _reader.ReadByte();
					token = MemlToken.Byte;
					break;
				case MemlToken.SByte:
					value = _reader.ReadSByte();
					token = MemlToken.SByte;
					break;
				case MemlToken.Char:
					value = _reader.ReadChar();
					token = MemlToken.Char;
					break;
				case MemlToken.Short:
					value = _reader.ReadInt16();
					token = MemlToken.Short;
					break;
				case MemlToken.UShort:
					value = _reader.ReadUInt16();
					token = MemlToken.UShort;
					break;
				case MemlToken.Int:
					value = _reader.ReadInt32();
					token = MemlToken.Int;
					break;
				case MemlToken.UInt:
					value = _reader.ReadUInt32();
					token = MemlToken.UInt;
					break;
				case MemlToken.Long:
					value = _reader.ReadInt64();
					token = MemlToken.Long;
					break;
				case MemlToken.ULong:
					value = _reader.ReadUInt64();
					token = MemlToken.ULong;
					break;
				case MemlToken.Decimal:
					value = _reader.ReadDecimal();
					token = MemlToken.Decimal;
					break;
				case MemlToken.Float:
					value = _reader.ReadSingle();
					token = MemlToken.Float;
					break;
				case MemlToken.Double:
					value = _reader.ReadDouble();
					token = MemlToken.Double;
					break;
				case MemlToken.Binary:
					{
						var len = _reader.ReadInt32();
						value = _reader.ReadBytes(len);
						token = MemlToken.Binary;
						break;
					}
			}

			return true;
		}

		return false;
	}

	public void SkipValue()
	{
		if (Read())
		{
			if (token == MemlToken.ObjectStart || token == MemlToken.ObjectEnd)
				_reader.BaseStream.Seek(_objectSize, SeekOrigin.Current);
		}
	}

	public void Dispose()
	{
		if (_disposeStream)
			_reader.Dispose();
	}
}
