using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MGE;

public static class Util
{
	public static bool FindType(string assemblyName, string fullTypeName, [MaybeNullWhen(false)] out Type type)
	{
		var asm = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == assemblyName);

		if (asm is null)
		{
			type = null;
			return false;
		}

		type = asm.GetType(fullTypeName);

		if (type is null) return false;
		return true;
	}

	public static Type FindType(string assemblyName, string fullTypeName)
	{
		var asm =
			AppDomain.CurrentDomain.GetAssemblies()
			.SingleOrDefault(assembly => assembly.GetName().Name == assemblyName) ??
			throw new Exception($"Assembly '{assemblyName}' not found");

		return
			asm.GetType(fullTypeName) ??
			throw new Exception($"Type '{fullTypeName}' not found in '{assemblyName}'");
	}
}
