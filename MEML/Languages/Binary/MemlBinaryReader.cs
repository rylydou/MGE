using System;
using System.IO;

namespace MEML;

public class MemlBinaryReader : IDataReader, IDisposable
{
	readonly BinaryReader _reader;
	readonly bool _disposeStream = true;
	uint _objectSize;

	public StructureToken token { get; private set; }
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
			var token = (StructureToken)_reader.ReadByte();

			switch (token)
			{
				case StructureToken.Null:
					value = null;
					token = StructureToken.Null;
					break;
				case StructureToken.ObjectStart:
					_objectSize = _reader.ReadUInt32(); // skip byte size
					value = null;
					token = StructureToken.ObjectStart;
					break;
				case StructureToken.ObjectEnd:
					value = null;
					token = StructureToken.ObjectEnd;
					break;
				case StructureToken.ObjectKey:
					value = _reader.ReadString();
					token = StructureToken.ObjectKey;
					break;
				case StructureToken.ArrayStart:
					_objectSize = _reader.ReadUInt32(); // skip byte size
					value = null;
					token = StructureToken.ArrayStart;
					break;
				case StructureToken.ArrayEnd:
					value = null;
					token = StructureToken.ArrayEnd;
					break;
				case StructureToken.Bool:
					value = _reader.ReadBoolean();
					token = StructureToken.Bool;
					break;
				case StructureToken.String:
					value = _reader.ReadString();
					token = StructureToken.String;
					break;
				case StructureToken.Byte:
					value = _reader.ReadByte();
					token = StructureToken.Byte;
					break;
				case StructureToken.SByte:
					value = _reader.ReadSByte();
					token = StructureToken.SByte;
					break;
				case StructureToken.Char:
					value = _reader.ReadChar();
					token = StructureToken.Char;
					break;
				case StructureToken.Short:
					value = _reader.ReadInt16();
					token = StructureToken.Short;
					break;
				case StructureToken.UShort:
					value = _reader.ReadUInt16();
					token = StructureToken.UShort;
					break;
				case StructureToken.Int:
					value = _reader.ReadInt32();
					token = StructureToken.Int;
					break;
				case StructureToken.UInt:
					value = _reader.ReadUInt32();
					token = StructureToken.UInt;
					break;
				case StructureToken.Long:
					value = _reader.ReadInt64();
					token = StructureToken.Long;
					break;
				case StructureToken.ULong:
					value = _reader.ReadUInt64();
					token = StructureToken.ULong;
					break;
				case StructureToken.Decimal:
					value = _reader.ReadDecimal();
					token = StructureToken.Decimal;
					break;
				case StructureToken.Float:
					value = _reader.ReadSingle();
					token = StructureToken.Float;
					break;
				case StructureToken.Double:
					value = _reader.ReadDouble();
					token = StructureToken.Double;
					break;
				case StructureToken.Binary:
					{
						var len = _reader.ReadInt32();
						value = _reader.ReadBytes(len);
						token = StructureToken.Binary;
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
			if (token == StructureToken.ObjectStart || token == StructureToken.ObjectEnd)
				_reader.BaseStream.Seek(_objectSize, SeekOrigin.Current);
		}
	}

	public void Dispose()
	{
		if (_disposeStream)
			_reader.Dispose();
	}
}
