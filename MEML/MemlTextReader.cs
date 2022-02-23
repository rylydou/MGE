using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MGE
{
	/// <summary>
	/// Reads JSON from a Stream or Path
	/// </summary>
	public class MemlTextReader : MemlReader, IDisposable
	{
		readonly TextReader _reader;
		readonly StringBuilder _builder = new();
		readonly bool _disposeStream;

		// in the case where the value of a previous key is completely empty, we want to
		// return null, and then store the current value for the next Read call
		// this only matters for non-strict JSON
		bool _storedNext;
		string? _storedString;
		MemlToken _storedToken;
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

		public override long position => _position;

		public override bool Read()
		{
			value = null;
			var lastToken = token;

			if (_storedNext)
			{
				value = _storedString;
				token = _storedToken;
				_storedNext = false;
				return true;
			}

			while (StepChar(out var next))
			{
				// Skip whitespace and characters we don't care about
				if (char.IsWhiteSpace(next) || next == ':') continue;

				// a comment
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
						if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{position}");
						token = MemlToken.ObjectEnd;
						return true;

					// Array
					case '[':
						if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{position}");
						token = MemlToken.ArrayStart;
						return true;
					case ']': token = MemlToken.ArrayEnd; return true;

					case '*':
						{
							while (StepChar(out next) && next != '*')
							{
								_builder.Append(next);
							}

							token = MemlToken.Binary;
							value = Convert.FromBase64String(_builder.ToString(1, _builder.Length - 1));
							return true;
						}

					// A string
					case '\'':
						{
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
										_ => throw new Exception($"'\\{next}' is not a valid escape sequence @{position}"),
									});
									continue;
								}

								_builder.Append(next);
							}

							token = MemlToken.String;
							value = _builder.ToString();
							return true;
						}

					// Other value
					default:
						{
							_builder.Clear();
							_builder.Append(next);

							while (PeekChar(out next) && !("\r\n,:{}[]#").Contains(next))
							{
								_builder.Append(next);
								SkipChar();
							}

							break;
						}
				}

				// Check if this entry is a KEY
				var isKey = false;
				{
					if (char.IsWhiteSpace(next))
					{
						while (PeekChar(out next) && char.IsWhiteSpace(next))
							SkipChar();
					}

					if (PeekChar(out next) && next == ':')
						isKey = true;
				}

				var str = _builder.ToString();

				// Is a key
				if (isKey)
				{
					if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{position}");

					token = MemlToken.ObjectKey;
					value = str;
					return true;
				}
				else
				{
					// null value
					if (str.Length <= 0 || str.Equals("null", StringComparison.OrdinalIgnoreCase))
					{
						token = MemlToken.Null;
						return true;
					}
					// true value
					else if (str.Equals("true", StringComparison.OrdinalIgnoreCase))
					{
						token = MemlToken.Bool;
						value = true;
						return true;
					}
					// false value
					else if (str.Equals("false", StringComparison.OrdinalIgnoreCase))
					{
						token = MemlToken.Bool;
						value = false;
						return true;
					}
					// Number: 123.456f or (int) 123456
					else if ((str[0] >= '0' && str[0] <= '9') || str[0] == '-' || str[0] == '+' || str[0] == '.')
					{
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

					throw new Exception($"Invalid value @{position}\nmight be an unquoeted string");
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

		public void Dispose()
		{
			if (_disposeStream)
				_reader.Dispose();
		}
	}
}
