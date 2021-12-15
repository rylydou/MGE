using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MGE;

public static class Debug
{
	public static void Log(string message) => Trace.WriteLine(message);

	public static void Log(object? obj, [CallerArgumentExpression("obj")] string message = "") => Trace.WriteLine($"{message}: {obj?.ToString() ?? "(null)"}");

	public static void LogList(IEnumerable list, [CallerArgumentExpression("list")] string message = "")
	{
		Trace.Write(message);
		Trace.WriteLine(":");

		var index = 0;
		foreach (var item in list)
		{
			Trace.Write("  ");
			Trace.Write(index);
			Trace.Write(". ");
			Trace.WriteLine(item?.ToString() ?? "(null)");

			index++;
		}
	}

	public static void LogDict(IDictionary dict, [CallerArgumentExpression("dict")] string message = "")
	{
		Trace.Write(message);
		Trace.Write(" (");
		Trace.Write(dict.Count);
		Trace.WriteLine("):");

		foreach (IDictionaryEnumerator pair in dict)
		{
			Trace.Write("  ");
			Trace.Write(pair.Key?.ToString() ?? "(null)");
			Trace.Write(": ");
			Trace.WriteLine(pair.Value?.ToString() ?? "(null)");
		}
	}
}
