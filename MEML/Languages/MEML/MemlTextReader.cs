using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MEML;

/// <summary>
/// Reads JSON from a Stream or Path
/// </summary>
public class MemlTextReader : IDataReader, IDisposable
{
	public StructureToken token { get; private set; }
	public object? value { get; private set; }

	readonly TextReader _reader;
	readonly StringBuilder _builder = new();
	readonly bool _disposeStream;

	bool _storedNext;
	long _position;

	public MemlTextReader(string path) : this(File.OpenRead(path))
	{

	}

	public MemlTextReader(Stream stream, bool disposeStream = true) : this(new StreamReader(stream, Encoding.UTF8, true, 4096), disposeStream)
	{

	}

	public MemlTextReader(TextReader reader, bool disposeStream = true)
	{
		this._reader = reader;
		this._disposeStream = disposeStream;
		_position = 0;
	}

	public bool Read()
	{
		value = null;
		var lastToken = token;

		if (_storedNext)
		{
			_storedNext = false;
			return true;
		}

		while (StepChar(out var next))
		{
			// Skip whitespace
			if (char.IsWhiteSpace(next) || next == ':') continue;

			// Skip comments
			if (next == '#')
			{
				while (StepChar(out next) && next != '\n' && next != '\r') continue;
				continue;
			}

			switch (next)
			{
				// Object
				case '{':
					token = StructureToken.ObjectStart;
					return true;
				case '}':
					if (lastToken == StructureToken.ObjectKey) throw new Exception($"Empty value @{_position}");
					token = StructureToken.ObjectEnd;
					return true;

				// Array
				case '[':
					token = StructureToken.ArrayStart;
					return true;
				case ']':
					if (lastToken == StructureToken.ObjectKey) throw new Exception($"Empty value @{_position}");
					token = StructureToken.ArrayEnd;
					return true;

				// Binary
				case '*':
					_builder.Clear();
					while (StepChar(out next) && next != '*')
					{
						_builder.Append(next);
					}

					token = StructureToken.Binary;
					value = Convert.FromBase64String(_builder.ToString());
					return true;

				// String
				case '\'':
					_builder.Clear();
					while (StepChar(out next) && next != '\'')
					{
						if (next == '\\')
						{
							StepChar(out next);
							_builder.Append(next switch
							{
								'n' => _builder.Append('\n'),
								'r' => _builder.Append('\r'),
								't' => _builder.Append('\t'),
								'v' => _builder.Append('\v'),
								'\'' => _builder.Append('\''),
								'\\' => _builder.Append('\\'),
								_ => throw new Exception($"'\\{next}' is not a valid escape sequence @{_position}"),
							});
							continue;
						}

						_builder.Append(next);
					}

					token = StructureToken.String;
					value = _builder.ToString();
					return true;

				// Other
				default:
					_builder.Clear();
					_builder.Append(next);

					while (PeekChar(out next) && !("\r\n,:{}[]#").Contains(next))
					{
						_builder.Append(next);
						SkipChar();
					}

					break;
			}

			// Check if this entry is a KEY
			var isKey = false;
			if (char.IsWhiteSpace(next))
			{
				while (PeekChar(out next) && char.IsWhiteSpace(next))
					SkipChar();
			}

			if (PeekChar(out next) && next == ':')
				isKey = true;

			var str = _builder.ToString();

			// Key
			if (isKey)
			{
				if (lastToken == StructureToken.ObjectKey) throw new Exception($"Empty value @{_position}");

				token = StructureToken.ObjectKey;
				value = str;
				return true;
			}
			else
			{
				switch (str)
				{
					case "null": token = StructureToken.Null; value = null; return true;
					case "true": token = StructureToken.Bool; value = true; return true;
					case "false": token = StructureToken.Bool; value = false; return true;
					default:
						// Number
						switch (str.Last())
						{
							case 'b': token = StructureToken.SByte; value = sbyte.Parse(str.Substring(0, str.Length - 1)); break;
							case 'B': token = StructureToken.Byte; value = byte.Parse(str.Substring(0, str.Length - 1)); break;

							case 'c': token = StructureToken.Char; value = char.Parse(str.Substring(0, str.Length - 1)); break;
							case 's': token = StructureToken.Short; value = short.Parse(str.Substring(0, str.Length - 1)); break;
							case 'S': token = StructureToken.UShort; value = ushort.Parse(str.Substring(0, str.Length - 1)); break;
							case 'I': token = StructureToken.UInt; value = uint.Parse(str.Substring(0, str.Length - 1)); break;
							case 'l': token = StructureToken.Long; value = long.Parse(str.Substring(0, str.Length - 1)); break;
							case 'L': token = StructureToken.ULong; value = ulong.Parse(str.Substring(0, str.Length - 1)); break;

							case 'd': token = StructureToken.Double; value = double.Parse(str.Substring(0, str.Length - 1)); break;
							case 'm': token = StructureToken.Decimal; value = decimal.Parse(str.Substring(0, str.Length - 1)); break;

							default:
								if (str.Contains('.'))
								{
									token = StructureToken.Float;
									value = float.Parse(str);
								}
								else
								{
									token = StructureToken.Int;
									value = int.Parse(str);
								}
								break;
						}

						return true;
				}

				throw new Exception($"Invalid value @{_position}\n	This might be an unquoeted string");
			}
		}

		return false;

		bool SkipChar()
		{
			return StepChar(out _);
		}

		bool StepChar(out char next)
		{
			int read = _reader.Read();
			next = (char)read;
			_position++;
			return read >= 0;
		}

		bool PeekChar(out char next)
		{
			int read = _reader.Peek();
			next = (char)read;
			return read >= 0;
		}
	}

	public void SkipValue()
	{
		Read();
	}

	public void Dispose()
	{
		if (_disposeStream)
			_reader.Dispose();
	}
}
