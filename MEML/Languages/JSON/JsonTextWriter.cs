using System;
using System.IO;

namespace MEML;

/// <summary>
/// Writes Meml to a string format
/// </summary>
public class JsonTextWriter : IMemlWriter, IDisposable
{
	public string newlineString = "\n";
	public string tabString = "\t";

	public bool prettyPrint = true;

	readonly TextWriter _writer;
	int _depth = 0;
	bool _wasValue;
	bool _wasBracket;

	public JsonTextWriter(string path) : this(File.Create(path))
	{
	}

	public JsonTextWriter(Stream stream) : this(new StreamWriter(stream))
	{
	}

	public JsonTextWriter(TextWriter writer)
	{
		this._writer = writer;
	}

	void Next(bool isValue = false, bool isKey = false, bool isBracket = false)
	{
		if (_wasValue)
			_writer.Write(",");

		if (prettyPrint)
		{
			if ((_wasValue || (_wasBracket && !isBracket) || isKey))
				Newline();

			if (_wasBracket && isBracket)
				_writer.Write(' ');
		}

		_wasValue = isValue;
		_wasBracket = isBracket;
	}

	void Newline()
	{
		_writer.Write(newlineString);
		for (int i = 0; i < _depth; i++)
			_writer.Write(tabString);
	}

	void ContainerBegin(char id)
	{
		Next(isBracket: true);
		_writer.Write(id);
		_depth++;
	}

	void ContainerEnd(char id)
	{
		_depth--;

		if (prettyPrint)
		{
			if (_wasBracket)
				_writer.Write(' ');
			else
				Newline();
		}

		_writer.Write(id);

		_wasValue = true;
		_wasBracket = false;
	}

	public void Key(string name)
	{
		Next(isKey: true);

		_writer.Write('"');
		_writer.Write(name);
		_writer.Write("\": ");
	}

	public void ObjectBegin()
	{
		ContainerBegin('{');
	}

	public void ObjectEnd()
	{
		ContainerEnd('}');
	}

	public void ArrayBegin()
	{
		ContainerBegin('[');
	}

	public void ArrayEnd()
	{
		ContainerEnd(']');
	}

	public void Null()
	{
		Next(isValue: true);
		_writer.Write("null");
	}

	public void Value(bool value)
	{
		Next(isValue: true);
		_writer.Write(value ? "true" : "false");
	}

	public void Value(byte value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(sbyte value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(char value)
	{
		Next(isValue: true);
		_writer.Write((short)value);
	}

	public void Value(short value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(ushort value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(int value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(uint value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(long value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(ulong value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(float value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(double value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(decimal value)
	{
		Next(isValue: true);
		_writer.Write(value);
	}

	public void Value(string value)
	{
		Next(isValue: true);

		_writer.Write('"');

		for (int i = 0; i < value.Length; i++)
		{
			switch (value[i])
			{
				case '\n': _writer.Write(@"\n"); break;
				case '\r': _writer.Write(@"\r"); break;
				case '\t': _writer.Write(@"\t"); break;
				case '\v': _writer.Write(@"\v"); break;
				case '\"': _writer.Write(@""""); break;
				case '\\': _writer.Write(@"\"); break;

				default: _writer.Write(value[i]); break;
			}
		}

		_writer.Write('"');
	}

	public void Value(ReadOnlySpan<byte> value)
	{
		Next(isValue: true);
		// An unusual enough pattern to block any text starting with it
		_writer.Write("\"bin::");
		_writer.Write(Convert.ToBase64String(value, Base64FormattingOptions.None));
		_writer.Write('"');
	}

	public void Dispose()
	{
		_writer.Dispose();
	}
}
