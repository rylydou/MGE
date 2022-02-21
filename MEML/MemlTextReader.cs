using System;
using System.IO;
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
				// skip whitespace and characters we don't care about
				if (char.IsWhiteSpace(next) || next == ':' || next == ',')
					continue;

				// a comment
				if (next == '#' || (next == '/' && (PeekChar(out var p) && p == '/')))
				{
					while (StepChar(out next) && next != '\n' && next != '\r')
						continue;
					continue;
				}

				var isEncapsulated = false;

				switch (next)
				{
					// object
					case '{':
						token = MemlToken.ObjectStart;
						return true;

					case '}':

						// if we found an object-end after a key
						// set the value of that last key to null, and store this value for next time
						if (lastToken == MemlToken.ObjectKey)
						{
							_storedNext = true;
							_storedToken = MemlToken.ObjectEnd;
							_storedString = null;

							value = null;
							token = MemlToken.Null;
							return true;
						}

						token = MemlToken.ObjectEnd;
						return true;

					// array
					case '[':
						token = MemlToken.ArrayStart;
						return true;

					case ']':

						// if we found an array-end after a key
						// set the value of that last key to null, and store this value for next time
						if (lastToken == MemlToken.ObjectKey)
						{
							_storedNext = true;
							_storedToken = MemlToken.ArrayEnd;
							_storedString = null;

							value = null;
							token = MemlToken.Null;
							return true;
						}

						token = MemlToken.ArrayEnd;
						return true;

					// an encapsulated string
					case '\'':
						{
							_builder.Clear();

							while (StepChar(out next) && next != '\'')
							{
								if (next == '\\')
								{
									StepChar(out next);
									if (next == 'n')
										_builder.Append('\n');
									else if (next == 'r')
										_builder.Append('\r');
									else
										_builder.Append(next);
									continue;
								}

								_builder.Append(next);
							}

							isEncapsulated = true;
							break;
						}

					// other value
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

				// check if this entry is a KEY
				bool isKey = false;
				{
					if (char.IsWhiteSpace(next))
					{
						while (PeekChar(out next) && char.IsWhiteSpace(next))
							SkipChar();
					}

					if (PeekChar(out next) && next == ':')
						isKey = true;
				}

				// is a key
				if (isKey)
				{
					// if we found an key after a key
					// set the value of that last key to null, and store this value for next time
					if (lastToken == MemlToken.ObjectKey)
					{
						_storedNext = true;
						_storedToken = MemlToken.ObjectKey;
						_storedString = _builder.ToString();

						value = null;
						token = MemlToken.Null;
						return true;
					}

					token = MemlToken.ObjectKey;
					value = _builder.ToString();
					return true;
				}
				// is an ecnapsulated string
				else if (isEncapsulated)
				{
					var str = _builder.ToString();

					if (str.StartsWith("bin::"))
					{
						token = MemlToken.Binary;
						value = Convert.FromBase64String(str.Substring(5));
						return true;
					}
					else
					{
						token = MemlToken.String;
						value = str;
						return true;
					}
				}
				else
				{
					var str = _builder.ToString();

					// null value
					if (str.Length <= 0 || str.Equals("null", StringComparison.OrdinalIgnoreCase))
					{
						token = MemlToken.Null;
						return true;
					}
					// true value
					else if (str.Equals("true", StringComparison.OrdinalIgnoreCase))
					{
						token = MemlToken.Boolean;
						value = true;
						return true;
					}
					// false value
					else if (str.Equals("false", StringComparison.OrdinalIgnoreCase))
					{
						token = MemlToken.Boolean;
						value = false;
						return true;
					}
					// could be a number value ...
					// this is kinda ugly ... but we just try to fit it into the smallest number type it can be
					else if ((str[0] >= '0' && str[0] <= '9') || str[0] == '-' || str[0] == '+' || str[0] == '.')
					{
						token = MemlToken.Number;

						// decimal, float, double
						if (str.Contains('.'))
						{
							if (float.TryParse(str, out float floatValue))
							{
								value = floatValue;
								return true;
							}
							else if (double.TryParse(str, out double doubleValue))
							{
								value = doubleValue;
								return true;
							}
						}
						else if (int.TryParse(str, out int intValue))
						{
							value = intValue;
							return true;
						}
						else if (long.TryParse(str, out long longValue))
						{
							value = longValue;
							return true;
						}
						else if (ulong.TryParse(str, out ulong ulongValue))
						{
							value = ulongValue;
							return true;
						}
					}

					// fallback to string
					token = MemlToken.String;
					value = str;
					return true;
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
