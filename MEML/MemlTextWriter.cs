using System;
using System.IO;

namespace MGE
{
	/// <summary>
	/// Writes Meml to a string format
	/// </summary>
	public class MemlTextWriter : MemlWriter, IDisposable
	{
		/// <summary>
		/// The New Line character to use
		/// </summary>
		public string newlineString = "\n";

		/// <summary>
		/// The Tab character to use for indents
		/// </summary>
		public string tabString = "\t";

		/// <summary>
		/// Whether the output should be Verbose
		/// </summary>
		public bool verbose = true;

		/// <summary>
		/// Whether the output should be Strict (true) Meml or not
		/// When false, the output will look more like hjson
		/// </summary>
		public bool asJson = false;

		readonly TextWriter _writer;
		int _depth = 0;
		bool _wasValue;
		bool _wasBracket;

		public MemlTextWriter(string path, bool asJson = false) : this(File.Create(path), asJson)
		{
		}

		public MemlTextWriter(Stream stream, bool asJson = false) : this(new StreamWriter(stream), asJson)
		{
		}

		public MemlTextWriter(TextWriter writer, bool asJson = false)
		{
			this._writer = writer;
			this.asJson = asJson;
		}

		void Next(bool isValue = false, bool isKey = false, bool isBracket = false)
		{
			if (_wasValue && (asJson || !verbose))
				_writer.Write(",");
			if ((_wasValue || (_wasBracket && !isBracket) || isKey) && verbose)
				Newline();
			if (_wasBracket && isBracket && verbose)
				_writer.Write(" ");

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

			if (verbose)
			{
				if (_wasBracket)
					_writer.Write(" ");
				else
					Newline();
			}

			_writer.Write(id);

			_wasValue = true;
			_wasBracket = false;
		}

		public override void Key(string name)
		{
			Next(isKey: true);
			EscapedString(name, true);

			_writer.Write(":");
			if (verbose)
				_writer.Write(" ");
		}

		public override void ObjectBegin()
		{
			ContainerBegin('{');
		}

		public override void ObjectEnd()
		{
			ContainerEnd('}');
		}

		public override void ArrayBegin()
		{
			ContainerBegin('[');
		}

		public override void ArrayEnd()
		{
			ContainerEnd(']');
		}

		public override void Comment(string text)
		{
			if (!asJson && verbose && text.Length > 0)
			{
				ReadOnlySpan<char> span = text;
				int last = 0;
				int next;
				while ((next = text.IndexOf('\n', last)) >= 0)
				{
					_writer.Write("# ");
					_writer.Write(span.Slice(last, next - last));
					Newline();
					last = next + 1;
				}

				_writer.Write("# ");
				_writer.Write(span.Slice(last));
			}
		}

		public override void Null()
		{
			Next(isValue: true);
			_writer.Write("null");
		}

		public override void Value(bool value)
		{
			Next(isValue: true);
			_writer.Write(value ? "true" : "false");
		}

		public override void Value(byte value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(char value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(short value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(ushort value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(int value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(uint value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(long value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(ulong value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(decimal value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(float value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(double value)
		{
			Next(isValue: true);
			_writer.Write(value);
		}

		public override void Value(string value)
		{
			Next(isValue: true);
			EscapedString(value, false);
		}

		public override void Value(ReadOnlySpan<byte> value)
		{
			Next(isValue: true);
			_writer.Write('"');
			_writer.Write("bin::");
			_writer.Write(Convert.ToBase64String(value, Base64FormattingOptions.None));
			_writer.Write('"');
		}

		bool StringContainsAny(string value, string chars)
		{
			for (int i = 0; i < chars.Length; i++)
				if (value.Contains(chars[i]))
					return true;
			return false;
		}

		void EscapedString(string value, bool isKey)
		{
			var encapsulate = asJson || !isKey || StringContainsAny(value, ":#{}[],\'\"\n\r") || (value.Length > 0 && char.IsWhiteSpace(value[0])) || value.Length <= 0;

			if (encapsulate)
			{
				_writer.Write(asJson ? '"' : '\'');

				for (int i = 0; i < value.Length; i++)
				{
					switch (value[i])
					{
						case '\n': _writer.Write(@"\n"); break;
						case '\t': _writer.Write(@"\t"); break;
						case '\r': _writer.Write(@"\r"); break;
						case '\'': _writer.Write(@"\'"); break;
						case '\"': _writer.Write(@"\"""); break;
						case '\\': _writer.Write(@"\"); break;

						default:
							_writer.Write(value[i]);
							break;
					}
				}

				_writer.Write(asJson ? '"' : '\'');
			}
			else
			{
				_writer.Write(value);
			}
		}

		public void Dispose()
		{
			_writer.Dispose();
		}
	}
}
