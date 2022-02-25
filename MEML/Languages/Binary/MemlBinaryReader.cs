using System;
using System.IO;

namespace MEML
{
	public class MemlBinaryReader : DataReader, IDisposable
	{
		readonly BinaryReader _reader;
		readonly bool _disposeStream = true;
		uint _objectSize;

		public MemlBinaryReader(string path) : this(File.OpenRead(path), true) { }

		public MemlBinaryReader(Stream stream, bool disposeStream = true) : this(new BinaryReader(stream), disposeStream) { }

		public MemlBinaryReader(BinaryReader reader, bool disposeStream = true)
		{
			this._reader = reader;
			this._disposeStream = disposeStream;
		}

		public override bool Read()
		{
			if (_reader.BaseStream.Position < _reader.BaseStream.Length)
			{
				var token = (MemlBinaryWriter.BinaryTokens)_reader.ReadByte();

				switch (token)
				{
					case MemlBinaryWriter.BinaryTokens.Null:
						value = null;
						base.token = StructureToken.Null;
						break;
					case MemlBinaryWriter.BinaryTokens.ObjectStart:
						_objectSize = _reader.ReadUInt32(); // skip byte size
						value = null;
						base.token = StructureToken.ObjectStart;
						break;
					case MemlBinaryWriter.BinaryTokens.ObjectEnd:
						value = null;
						base.token = StructureToken.ObjectEnd;
						break;
					case MemlBinaryWriter.BinaryTokens.ObjectKey:
						value = _reader.ReadString();
						base.token = StructureToken.ObjectKey;
						break;
					case MemlBinaryWriter.BinaryTokens.ArrayStart:
						_objectSize = _reader.ReadUInt32(); // skip byte size
						value = null;
						base.token = StructureToken.ArrayStart;
						break;
					case MemlBinaryWriter.BinaryTokens.ArrayEnd:
						value = null;
						base.token = StructureToken.ArrayEnd;
						break;
					case MemlBinaryWriter.BinaryTokens.Boolean:
						value = _reader.ReadBoolean();
						base.token = StructureToken.Bool;
						break;
					case MemlBinaryWriter.BinaryTokens.String:
						value = _reader.ReadString();
						base.token = StructureToken.String;
						break;
					case MemlBinaryWriter.BinaryTokens.Byte:
						value = _reader.ReadByte();
						base.token = StructureToken.Byte;
						break;
					case MemlBinaryWriter.BinaryTokens.SByte:
						value = _reader.ReadSByte();
						base.token = StructureToken.SByte;
						break;
					case MemlBinaryWriter.BinaryTokens.Char:
						value = _reader.ReadChar();
						base.token = StructureToken.Char;
						break;
					case MemlBinaryWriter.BinaryTokens.Short:
						value = _reader.ReadInt16();
						base.token = StructureToken.Short;
						break;
					case MemlBinaryWriter.BinaryTokens.UShort:
						value = _reader.ReadUInt16();
						base.token = StructureToken.UShort;
						break;
					case MemlBinaryWriter.BinaryTokens.Int:
						value = _reader.ReadInt32();
						base.token = StructureToken.Int;
						break;
					case MemlBinaryWriter.BinaryTokens.UInt:
						value = _reader.ReadUInt32();
						base.token = StructureToken.UInt;
						break;
					case MemlBinaryWriter.BinaryTokens.Long:
						value = _reader.ReadInt64();
						base.token = StructureToken.Long;
						break;
					case MemlBinaryWriter.BinaryTokens.ULong:
						value = _reader.ReadUInt64();
						base.token = StructureToken.ULong;
						break;
					case MemlBinaryWriter.BinaryTokens.Decimal:
						value = _reader.ReadDecimal();
						base.token = StructureToken.Decimal;
						break;
					case MemlBinaryWriter.BinaryTokens.Float:
						value = _reader.ReadSingle();
						base.token = StructureToken.Float;
						break;
					case MemlBinaryWriter.BinaryTokens.Double:
						value = _reader.ReadDouble();
						base.token = StructureToken.Double;
						break;
					case MemlBinaryWriter.BinaryTokens.Binary:
						{
							var len = _reader.ReadInt32();
							value = _reader.ReadBytes(len);
							base.token = StructureToken.Binary;
							break;
						}
				}

				return true;
			}

			return false;
		}

		public override void SkipValue()
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
}
