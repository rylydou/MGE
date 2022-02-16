using System;
using System.Diagnostics;
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
		public const string Black = "30";
		public const string DarkBlue = "34";
		public const string DarkGreen = "32";
		public const string DarkCyan = "36";
		public const string DarkRed = "31";
		public const string DarkMagenta = "35";
		public const string DarkYellow = "33";
		public const string Gray = "37";
		public const string DarkGray = "90";
		public const string Blue = "94";
		public const string Green = "92";
		public const string Cyan = "96";
		public const string Red = "91";
		public const string Magenta = "95";
		public const string Yellow = "93";
		public const string White = "97";
	}

	struct LogAttribute
	{
		public string Name;
		public string Color;
	}

	private const int STD_OUTPUT_HANDLE = -11;
	private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

	[DllImport("kernel32.dll")]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[DllImport("kernel32.dll")]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	private static readonly StringBuilder log;
	private static readonly LogAttribute[] logAttributes;
	private static readonly bool colorEnabled;

	public static LogLevel verbosity = LogLevel.Trace;
	public static bool printToConsole = true;

	static Log()
	{
		log = new StringBuilder();
		logAttributes = new[]
		{
			new LogAttribute { Name = "SYSTEM", Color = LogColor.Cyan },
			new LogAttribute { Name = "ASSERT", Color = LogColor.Magenta },
			new LogAttribute { Name = "ERROR ", Color = LogColor.Red },
			new LogAttribute { Name = "WARN  ", Color = LogColor.Yellow },
			new LogAttribute { Name = "INFO  ", Color = LogColor.Green },
			new LogAttribute { Name = "DEBUG ", Color = LogColor.Cyan },
			new LogAttribute { Name = "TRACE ", Color = LogColor.White }
		};

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var stdOut = GetStdHandle(STD_OUTPUT_HANDLE);

			colorEnabled = GetConsoleMode(stdOut, out var outConsoleMode) &&
										 SetConsoleMode(stdOut, outConsoleMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
		}
		else
		{
			colorEnabled = false;
		}

		Log.System($"Logging Enabled ({Enum.GetName(typeof(LogLevel), verbosity)})");
	}

	static void LogInternal(LogLevel logLevel, string message, string callerFilePath, int callerLineNumber)
	{
		if (verbosity < logLevel) return;

		var logAttribute = logAttributes[(int)logLevel];
		var callsite = $"{Path.GetFileName(callerFilePath)}:{callerLineNumber.ToString()}";

		if (printToConsole)
		{
			Console.WriteLine(
				colorEnabled ?
				$"\u001b[{LogColor.Gray}m{DateTime.Now.ToString("HH:mm:ss")} \u001b[{logAttribute.Color}m{logAttribute.Name}\u001b[{LogColor.Gray}m {callsite,-32} \u001b[{LogColor.White}m{message}\u001b[0m" :
				$"{DateTime.Now.ToString("HH:mm:ss")} {logAttribute.Name} {callsite,-32} {message}");
		}

		log.Append($"{DateTime.Now.ToString("HH:mm:ss")} {logAttribute.Name} {callsite,-32} {message}");

		if ((logLevel == LogLevel.Error) || (logLevel == LogLevel.Assert))
		{
			Debugger.Break();
		}
	}

	public static void Trace(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		LogInternal(LogLevel.Trace, message, callerFilePath, callerLineNumber);
	}

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
}
