using System.Linq;
using System.Reflection;
using MEML;

namespace MGE;

public class Prefab
{
	static StructureConverter _converter;

	static Prefab()
	{
		_converter = Util.GetStructureConverter();
		_converter.memberFinder = (type) =>
			type.GetMembers(StructureConverter.suggestedBindingFlags)
			.Where(m => m.GetCustomAttribute<PropAttribute>() is not null || m.GetCustomAttribute<HiddenPropAttribute>() is not null);
	}

	StructureValue _prefab;

	public Prefab(StructureValue prefab)
	{
		_prefab = prefab;
	}

	public Node CreateInstance()
	{
		return _converter.CreateObjectFromStructure<Node>(_prefab) ?? throw new Exception("Create instance of prefab", "Prefab data is null");
	}

	public T CreateInstance<T>()
	{
		return _converter.CreateObjectFromStructure<T>(_prefab);
	}
}
