using System;
using System.IO;

namespace MGE
{
	public class MemlBinaryReader : MemlReader, IDisposable
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

		public override long position => _reader.BaseStream.Position;

		public override bool Read()
		{
			if (_reader.BaseStream.Position < _reader.BaseStream.Length)
			{
				var token = (MemlBinaryWriter.BinaryTokens)_reader.ReadByte();

				switch (token)
				{
					case MemlBinaryWriter.BinaryTokens.Null:
						value = null;
						base.token = MemlToken.Null;
						break;
					case MemlBinaryWriter.BinaryTokens.ObjectStart:
						_objectSize = _reader.ReadUInt32(); // skip byte size
						value = null;
						base.token = MemlToken.ObjectStart;
						break;
					case MemlBinaryWriter.BinaryTokens.ObjectEnd:
						value = null;
						base.token = MemlToken.ObjectEnd;
						break;
					case MemlBinaryWriter.BinaryTokens.ObjectKey:
						value = _reader.ReadString();
						base.token = MemlToken.ObjectKey;
						break;
					case MemlBinaryWriter.BinaryTokens.ArrayStart:
						_objectSize = _reader.ReadUInt32(); // skip byte size
						value = null;
						base.token = MemlToken.ArrayStart;
						break;
					case MemlBinaryWriter.BinaryTokens.ArrayEnd:
						value = null;
						base.token = MemlToken.ArrayEnd;
						break;
					case MemlBinaryWriter.BinaryTokens.Boolean:
						value = _reader.ReadBoolean();
						base.token = MemlToken.Boolean;
						break;
					case MemlBinaryWriter.BinaryTokens.String:
						value = _reader.ReadString();
						base.token = MemlToken.String;
						break;
					case MemlBinaryWriter.BinaryTokens.Byte:
						value = _reader.ReadByte();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Char:
						value = _reader.ReadChar();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Short:
						value = _reader.ReadInt16();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.UShort:
						value = _reader.ReadUInt16();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Int:
						value = _reader.ReadInt32();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.UInt:
						value = _reader.ReadUInt32();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Long:
						value = _reader.ReadInt64();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.ULong:
						value = _reader.ReadUInt64();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Decimal:
						value = _reader.ReadDecimal();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Float:
						value = _reader.ReadSingle();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Double:
						value = _reader.ReadDouble();
						base.token = MemlToken.Number;
						break;
					case MemlBinaryWriter.BinaryTokens.Binary:
						{
							var len = _reader.ReadInt32();
							value = _reader.ReadBytes(len);
							base.token = MemlToken.Binary;
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
}
