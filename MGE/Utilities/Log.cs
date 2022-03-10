using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MGE;

public static class Log
{
	public enum LogLevel
	{
		System,
		Assert,
		Error,
		Warning,
		Info,
		Debug,
		Trace
	}

	static class LogColor
	{
		public const string BLACK = "30";
		public const string DARK_BLUE = "34";
		public const string DARK_GREEN = "32";
		public const string DARK_CYAN = "36";
		public const string DARK_RED = "31";
		public const string DARK_MAGENTA = "35";
		public const string DARK_YELLOW = "33";
		public const string GRAY = "37";
		public const string DARK_GRAY = "90";
		public const string BLUE = "94";
		public const string GREEN = "92";
		public const string CYAN = "96";
		public const string RED = "91";
		public const string MAGENTA = "95";
		public const string YELLOW = "93";
		public const string WHITE = "97";
	}

	struct LogAttribute
	{
		public string name;
		public string color;
	}

	const int STD_OUTPUT_HANDLE = -11;
	const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

	[DllImport("kernel32.dll")]
	static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[DllImport("kernel32.dll")]
	static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern IntPtr GetStdHandle(int nStdHandle);

	static readonly StringBuilder _log;
	static readonly LogAttribute[] _logAttributes;
	static readonly bool _colorEnabled;

	public static LogLevel verbosity = LogLevel.Trace;
	public static bool printToConsole = true;

	static Log()
	{
		_log = new StringBuilder();
		_logAttributes = new[]
		{
			new LogAttribute { name = "SYSTEM", color = LogColor.CYAN },
			new LogAttribute { name = "ASSERT", color = LogColor.MAGENTA },
			new LogAttribute { name = "ERROR ", color = LogColor.RED },
			new LogAttribute { name = "WARN  ", color = LogColor.YELLOW },
			new LogAttribute { name = "INFO  ", color = LogColor.GREEN },
			new LogAttribute { name = "DEBUG ", color = LogColor.CYAN },
			new LogAttribute { name = "TRACE ", color = LogColor.GRAY }
		};

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var stdOut = GetStdHandle(STD_OUTPUT_HANDLE);

			_colorEnabled = GetConsoleMode(stdOut, out var outConsoleMode) && SetConsoleMode(stdOut, outConsoleMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
		}
		else
		{
			_colorEnabled = true;
		}

		Log.System($"Logging Enabled ({Enum.GetName(typeof(LogLevel), verbosity)})" + (_colorEnabled ? " with color" : ""));
	}

	static void LogInternal(LogLevel logLevel, string message, string callerFilePath, int callerLineNumber)
	{
		if (verbosity < logLevel) return;

		var logAttribute = _logAttributes[(int)logLevel];
		var callsite = $"{Path.GetFileName(callerFilePath)}:{callerLineNumber.ToString()}";

		if (printToConsole)
		{
			Console.WriteLine(
				_colorEnabled ?
				$"\u001b[{LogColor.DARK_GRAY}m{DateTime.Now.ToString("HH:mm:ss")} {callsite,-18} \u001b[{logAttribute.color}m{message}\u001b[0m" :
				$"{DateTime.Now.ToString("HH:mm:ss")} {logAttribute.name} {callsite,-18} {message}");
		}

		_log.Append($"{DateTime.Now.ToString("HH:mm:ss")} {logAttribute.name} {callsite,-18} {message}");

		if ((logLevel == LogLevel.Error) || (logLevel == LogLevel.Assert))
		{
			Debugger.Break();
		}
	}

	[Conditional("TRACE")]
	public static void Trace(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.Trace, message, callerFilePath, callerLineNumber);
	}

	[Conditional("DEBUG")]
	public static void Debug(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.Debug, message, callerFilePath, callerLineNumber);
	}

	public static void Info(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.Info, message, callerFilePath, callerLineNumber);
	}

	public static void Warning(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.Warning, message, callerFilePath, callerLineNumber);
	}

	public static void Error(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.Error, message, callerFilePath, callerLineNumber);
	}

	public static void Assert(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.Assert, message, callerFilePath, callerLineNumber);
	}

	public static void System(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.System, message, callerFilePath, callerLineNumber);
	}

	static Stack<(string name, Stopwatch stopwatch)> _stopwatchStack = new();
	public static void StartStopwatch(string name)
	{
		var stopwatch = new Stopwatch();
		_stopwatchStack.Push(new(name, stopwatch));
		stopwatch.Start();
	}

	public static void EndStopwatch()
	{
		var stopwatch = _stopwatchStack.Pop();
		stopwatch.stopwatch.Stop();
		Log.Info($"⏱ {stopwatch.name,-16} {stopwatch.stopwatch.ElapsedMilliseconds,5}ms");
	}
}
