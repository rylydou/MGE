using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace MGE;

public static class Debug
{
	public static TextWriter stdout = System.Console.Out;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void _Write(char ch) => stdout.Write(ch);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void _Write(string text) => stdout.Write(text);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void _Newline() => stdout.WriteLine();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogMsg(string msg)
	{
		_Write(msg);
		_Newline();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static string ObjAsString(object? obj)
	{
		if (obj is float f) return f.ToString("F4");
		else if (obj is double d) return d.ToString("F4");

		if (obj is null) return "(null)";

		try
		{
			return obj.ToString() ?? "(nothing)";
		}
		catch (System.Exception)
		{
			return "(error)";
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void WriteValue(string? label, string value)
	{
		if (label is not null)
		{
			_Write(label);
			_Write(": ");
		}

		_Write(value);
		_Newline();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogVal(object? value, [CallerArgumentExpression("value")] string? label = null)
	{
		WriteValue(label, ObjAsString(value));
	}

	public static void LogList(IEnumerable value, [CallerArgumentExpression("value")] string label = "list")
	{
		Trace.Write(label);
		Trace.WriteLine(":");

		var index = 0;
		foreach (var item in value)
		{
			Trace.Write(string.Format("0,2", index));
			Trace.Write(". ");
			Trace.WriteLine(item ?? "(null)");

			index++;
		}
	}

	public static void LogDict(IDictionary dict, [CallerArgumentExpression("dict")] string label = "list")
	{
		Trace.Write(label);
		Trace.Write(" (");
		Trace.Write(dict.Count);
		Trace.WriteLine("):");

		foreach (DictionaryEntry pair in dict)
		{
			Trace.Write("  ");
			Trace.Write(pair.Key ?? "(null)");
			Trace.Write(": ");
			Trace.WriteLine(pair.Value ?? "(null)");
		}
	}

	#region Timer

	static Stack<(string name, Stopwatch stopwatch)> _timerStack = new();

	public static void StartStopwatch(string name = "Timmer")
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		_timerStack.Push((name, stopwatch));
	}

	public static void EndStopwatch()
	{
		var (name, stopwatch) = _timerStack.Pop();
		stopwatch.Stop();

		Trace.Write("⏱ ");
		Trace.Write(name);
		Trace.Write(": ");
		if (stopwatch.ElapsedMilliseconds >= 10000) // After 10 seconds write the time in seconds
		{
			Trace.WriteLine(stopwatch.Elapsed.ToString(@"s\.ffff"));
		}
		else
		{
			Trace.Write(stopwatch.ElapsedMilliseconds);
			Trace.WriteLine("ms");
		}
	}

	#endregion Timer
}
