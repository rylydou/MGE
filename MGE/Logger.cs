using System;
using System.Collections;
using System.IO;
using System.Text;
using MGE.Debug;

namespace MGE
{
	public static class Logger
	{
#if MGE_ENABLE_FILE_LOGGING
		static readonly DateTime startTime = DateTime.Now;

		static StreamWriter _logWritter;
		static StreamWriter logWritter
		{
			get
			{
				if (_logWritter is null)
					_logWritter = new StreamWriter(Environment.CurrentDirectory + "/client.log");
				return _logWritter;
			}
		}

		public static string lastEntry;
		public static StringBuilder currentEntry = new StringBuilder();
		public static ushort currentEntryRepetedCount;
#endif

		public static void Log(object obj, object source = null)
		{
#if MGE_ENABLE_LOGGING
			StartEntry();

			if (source is object)
			{
				Write(source.ToString());
				Write(" - ");
			}
			Write(obj is object ? obj.ToString() : "(null)");
			WriteNewline();

			EndEntry();
#endif
		}

		public static void Separator()
		{
#if MGE_ENABLE_LOGGING
			WriteNewline();
#endif
		}

		public static void LogVar(string name, object value, object source = null)
		{
#if MGE_ENABLE_LOGGING
			StartEntry();

			if (value is IEnumerable enumerable && !(value is string))
			{
				if (source is object)
				{
					Write(source.ToString());
					Write(" - ");
				}
				Write(name);
				Write(" [");

				if (enumerable is null)
				{
					Write(" (null)");
					return;
				}

				var isEmpty = true;
				foreach (var item in (IEnumerable)enumerable)
				{
					isEmpty = false;
					WriteNewline();
					WriteIndent();
					Write(item is object ? item.ToString() : "(null)");
				}

				if (isEmpty)
				{
					WriteNewline();
					WriteIndent();
					Write("(empty)");
				}

				WriteNewline();
				Write("]");
				WriteNewline();
			}
			else
			{
				if (source is object)
				{
					Write(source.ToString());
					Write(" - ");
				}
				Write(string.IsNullOrEmpty(name) ? value?.GetType()?.ToString() : name.ToString());
				if (!name.EndsWith('?')) Write(":");
				Write(" ");
				Write(value is object ? value.ToString() : "(null)");

				WriteNewline();
			}

			EndEntry();
#endif
		}

		public static void LogWarn(string msg, object source = null)
		{
#if MGE_ENABLE_LOGGING
			StartEntry();

			if (source is object)
			{
				Write(source.ToString());
				Write(" ");
			}
			Write("WARN - ");
			Write(msg);
			WriteNewline();
			Write(Environment.StackTrace);
			WriteNewline();

			EndEntry();
#endif
		}

		public static void LogError(string msg, object source = null)
		{
#if MGE_ENABLE_LOGGING
			StartEntry();

			if (source is object)
			{
				Write(source.ToString());
				Write(" ");
			}
			Write("ERROR - ");
			Write(msg);
			WriteNewline();
			Write(Environment.StackTrace);
			WriteNewline();

			EndEntry();
#endif
		}

		public static void Trace()
		{
#if MGE_ENABLE_LOGGING
			StartEntry();

			Write(Environment.StackTrace);
			WriteNewline();

			EndEntry();
#endif
		}

		public static void StartHeader(string header)
		{
#if MGE_ENABLE_LOGGING
			WriteNewline();
			Write("--- ");
			Write(header.ToUpper());
			Write(" ---");
			WriteNewline();
#endif
		}

#if MGE_ENABLE_LOGGING
		internal static void Write(char letter) => currentEntry.Append(letter);
		internal static void Write(string text) => currentEntry.Append(text);
		internal static void WriteIndent() => currentEntry.Append('\t');
		internal static void WriteNewline() => currentEntry.Append(Environment.NewLine);

		internal static void StartEntry() => currentEntry.Clear();

		internal static void EndEntry()
		{
			// WriteNewline();
			var text = currentEntry.ToString();
			if (lastEntry == text)
			{
				currentEntryRepetedCount++;
				return;
			}
			lastEntry = text;

#if MGE_ENABLE_FILE_LOGGING
			logWritter?.Write(text);
#endif
			Console.Write(text);
			// Terminal.Write(text);

			currentEntryRepetedCount = 0;
			logWritter?.Flush();
		}
#endif
	}
}