using System.Linq;
using System.Reflection;
using MEML;

namespace MGE;

public class Prefab
{
	public static MemlConverter converter;

	static Prefab()
	{
		converter = Util.GetStructureConverter();
		converter.memberFinder = (type) =>
			type.GetMembers(MemlConverter.suggestedBindingFlags)
			.Where(m => m.GetCustomAttribute<PropAttribute>() is not null || m.GetCustomAttribute<HiddenPropAttribute>() is not null);
	}

	MemlValue _prefab;

	public Prefab(MemlValue prefab)
	{
		_prefab = prefab;
	}

	public Node CreateInstance(params object?[] args)
	{
		return converter.CreateObjectFromStructure<Node>(_prefab, args) ?? throw new Exception("Create instance of prefab", "Prefab data is null");
	}

	public T CreateInstance<T>(params object?[] args)
	{
		return converter.CreateObjectFromStructure<T>(_prefab, args);
	}
}
