using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MGE;

public static class Debug
{
	public static void Log(object? obj) => Trace.WriteLine(obj?.ToString() ?? "(null)");
	public static void LogVariable(object? obj, [CallerArgumentExpression("obj")] string message = "Variable") => Trace.WriteLine($"{message}: {obj?.ToString() ?? "(null)"}");
}
