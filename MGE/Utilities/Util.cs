using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using MEML;

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

	public static MemlConverter GetStructureConverter()
	{
		var _ = new MemlConverter();

		_.RegisterConverter<Vector2>(_ => new MemlArray(_.x, _.y), _ => new Vector2(_[0], _[1]));
		_.RegisterConverter<Vector2Int>(_ => new MemlArray(_.x, _.y), _ => new Vector2Int(_[0], _[1]));

		_.RegisterConverter<Vector3>(_ => new MemlArray(_.x, _.y, _.z), _ => new Vector3(_[0], _[1], _[2]));

		_.RegisterConverter<Rect>(_ => new MemlArray(_.x, _.y, _.width, _.height), _ => new Rect(_[0], _[1], _[2], _[3]));
		_.RegisterConverter<RectInt>(_ => new MemlArray(_.x, _.y, _.width, _.height), _ => new RectInt(_[0], _[1], _[2], _[3]));

		_.RegisterConverter<Color>(_ => "#" + _.ToHexStringRGBA(), _ => Color.FromHexStringRGBA(_));

		return _;
	}

	public static Stream EmbeddedResource(string resourceName)
	{
		var assembly = Assembly.GetCallingAssembly() ?? throw new Exception();
		return EmbeddedResource(assembly, resourceName);
	}

	public static Stream EmbeddedResource(Assembly assembly, string resourceName)
	{
		var fullname = assembly.GetName().Name + "." + resourceName;
		var path = fullname.Replace('/', '.').Replace('\\', '.');

		var stream = assembly.GetManifestResourceStream(path);
		if (stream is null)
			throw new Exception($"Embedded Resource '{resourceName}' doesn't exist");

		return stream;
	}

	public static string EmbeddedResourceText(string resourceName)
	{
		var assembly = Assembly.GetCallingAssembly() ?? throw new Exception();

		using var stream = EmbeddedResource(assembly, resourceName);
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}

	public static string EmbeddedResourceText(Assembly assembly, string resourceName)
	{
		using var stream = EmbeddedResource(assembly, resourceName);
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}
