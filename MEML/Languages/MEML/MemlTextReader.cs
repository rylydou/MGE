using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MEML;

/// <summary>
/// Reads JSON from a Stream or Path
/// </summary>
public class MemlTextReader : IDataReader, IDisposable
{
	public MemlToken token { get; private set; }
	public object? value { get; private set; }

	readonly TextReader _reader;
	readonly StringBuilder _builder = new();
	readonly bool _disposeStream;

	public Dictionary<string, (MemlToken token, object value)> variables = new();

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
					token = MemlToken.ObjectStart;
					return true;
				case '}':
					if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{_position}");
					token = MemlToken.ObjectEnd;
					return true;

				// Array
				case '[':
					token = MemlToken.ArrayStart;
					return true;
				case ']':
					if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{_position}");
					token = MemlToken.ArrayEnd;
					return true;

				// Binary
				case '*':
					_builder.Clear();
					while (StepChar(out next) && next != '*')
					{
						_builder.Append(next);
					}

					token = MemlToken.Binary;
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
								'n' => '\n',
								'r' => '\r',
								't' => '\t',
								'v' => '\v',
								'\'' => '\'',
								'\\' => '\\',
								_ => throw new Exception($"'\\{next}' is not a valid escape sequence @{_position}"),
							});
							continue;
						}

						_builder.Append(next);
					}

					token = MemlToken.String;
					value = _builder.ToString();
					return true;

				// Other
				default:
					_builder.Clear();
					_builder.Append(next);

					while (PeekChar(out next) && !("\r\n\t ,:{}[]#").Contains(next))
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
				if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{_position}");

				if (str.StartsWith('$'))
				{
					// Read variable value
					Read();
					object varValue;
					switch (token)
					{
						default: varValue = value!; break;
						case MemlToken.ObjectStart: varValue = this.ReadObject(); break;
						case MemlToken.ArrayStart: varValue = this.ReadArray(); break;

						case MemlToken.ObjectKey:
						case MemlToken.ObjectEnd:
						case MemlToken.ArrayEnd:
							throw new Exception($"Unexpected {token}");
					}

					variables.Add(str.Substring(1), (token, varValue));

					// Actually return something useful
					return Read();
				}

				token = MemlToken.ObjectKey;
				value = str;
				return true;
			}
			else
			{
				// Variable use
				if (str.StartsWith('$'))
				{
					var varName = str.Substring(1);

					if (!variables.TryGetValue(varName, out var varValue))
						throw new Exception($"Variable '{varName}' is not defined @{_position}");

					token = varValue.token;
					value = varValue.value;
					return true;
				}

				switch (str)
				{
					case "null": token = MemlToken.Null; value = null; return true;
					case "true": token = MemlToken.Bool; value = true; return true;
					case "false": token = MemlToken.Bool; value = false; return true;
					default:
						// Number
						switch (str.Last())
						{
							case 'b': token = MemlToken.SByte; value = sbyte.Parse(str.Substring(0, str.Length - 1)); break;
							case 'B': token = MemlToken.Byte; value = byte.Parse(str.Substring(0, str.Length - 1)); break;

							case 'c': token = MemlToken.Char; value = char.Parse(str.Substring(0, str.Length - 1)); break;
							case 's': token = MemlToken.Short; value = short.Parse(str.Substring(0, str.Length - 1)); break;
							case 'S': token = MemlToken.UShort; value = ushort.Parse(str.Substring(0, str.Length - 1)); break;
							case 'I': token = MemlToken.UInt; value = uint.Parse(str.Substring(0, str.Length - 1)); break;
							case 'l': token = MemlToken.Long; value = long.Parse(str.Substring(0, str.Length - 1)); break;
							case 'L': token = MemlToken.ULong; value = ulong.Parse(str.Substring(0, str.Length - 1)); break;

							case 'd': token = MemlToken.Double; value = double.Parse(str.Substring(0, str.Length - 1)); break;
							case 'm': token = MemlToken.Decimal; value = decimal.Parse(str.Substring(0, str.Length - 1)); break;

							default:
								if (str.Contains('.'))
								{
									token = MemlToken.Float;
									value = float.Parse(str);
								}
								else
								{
									token = MemlToken.Int;
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
