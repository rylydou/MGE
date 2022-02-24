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

		public override long position => _position;

		public override bool Read()
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

						token = MemlToken.String;
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
					if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{_position}");

					token = MemlToken.ObjectKey;
					value = str;
					return true;
				}
				else
				{
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

		public void Dispose()
		{
			if (_disposeStream)
				_reader.Dispose();
		}
	}
}
