using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MEML;

/// <summary>
/// Reads JSON from a Stream or Path
/// </summary>
public class JsonTextReader : IDataReader, IDisposable
{
	public MemlToken token { get; private set; }
	public object? value { get; private set; }

	readonly TextReader _reader;
	readonly StringBuilder _builder = new();
	readonly bool _disposeStream;

	bool _storedNext;
	long _position;

	public JsonTextReader(string path) : this(File.OpenRead(path))
	{

	}

	public JsonTextReader(Stream stream, bool disposeStream = true) : this(new StreamReader(stream, Encoding.UTF8, true, 4096), disposeStream)
	{

	}

	public JsonTextReader(TextReader reader, bool disposeStream = true)
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
			if (char.IsWhiteSpace(next) || next == ':' || next == ',') continue;

			// Skip comments
			if (next == '/' && (PeekChar(out var p) && p == '/'))
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

				// String
				case '"':
					_builder.Clear();

					while (StepChar(out next) && next != '"')
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
								'"' => '"',
								'\\' => '\\',
								_ => throw new Exception($"'\\{next}' is not a valid escape sequence @{_position}"),
							});
							continue;
						}

						_builder.Append(next);
					}

					var str = _builder.ToString();

					if (char.IsWhiteSpace(next))
					{
						while (PeekChar(out next) && char.IsWhiteSpace(next))
							SkipChar();
					}

					// Is key
					if (PeekChar(out next) && next == ':')
					{
						if (lastToken == MemlToken.ObjectKey) throw new Exception($"Empty value @{_position}");

						token = MemlToken.ObjectKey;
						value = str;
						return true;
					}

					if (str.StartsWith("bin::"))
					{
						token = MemlToken.Binary;
						value = Convert.FromBase64String(str.Substring(5));
						return true;
					}

					token = MemlToken.String;
					value = str;
					return true;

				// Other
				default:
					_builder.Clear();
					_builder.Append(next);

					while (PeekChar(out next) && !("\r\n,:{}[]").Contains(next))
					{
						_builder.Append(next);
						SkipChar();
					}

					break;
			}

			var s = _builder.ToString();

			switch (s)
			{
				case "null": token = MemlToken.Null; value = null; return true;
				case "true": token = MemlToken.Bool; value = true; return true;
				case "false": token = MemlToken.Bool; value = false; return true;
			}

			if ((s[0] >= '0' && s[0] <= '9') || s[0] == '-' || s[0] == '+' || s[0] == '.')
			{
				if (s.Contains('.'))
				{
					if (float.TryParse(s, out float floatValue))
					{
						token = MemlToken.Float;
						value = floatValue;
						return true;
					}

					if (double.TryParse(s, out double doubleValue))
					{
						token = MemlToken.Double;
						value = doubleValue;
						return true;
					}
				}

				if (int.TryParse(s, out int intValue))
				{
					token = MemlToken.Int;
					value = intValue;
					return true;
				}

				if (long.TryParse(s, out long longValue))
				{
					token = MemlToken.Long;
					value = longValue;
					return true;
				}

				if (ulong.TryParse(s, out ulong ulongValue))
				{
					token = MemlToken.ULong;
					value = ulongValue;
					return true;
				}
			}

			throw new Exception($"Invalid value @{_position}\n\tThis might be an unquoeted string");
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
