using System.Linq;
using System.Reflection;
using MEML;

namespace MGE;

public class Prefab
{
	static StructureConverter _converter = new()
	{
		memberFinder = (type) =>
			type.GetMembers(StructureConverter.suggestedBindingFlags)
			.Where(m => m.GetCustomAttribute<PropAttribute>() is not null || m.GetCustomAttribute<HiddenPropAttribute>() is not null)
	};

	StructureValue _prefab;

	public Prefab(StructureValue prefab)
	{
		_prefab = prefab;
	}

	public object CreateInstance()
	{
		return _converter.CreateObjectFromStructure(_prefab) ?? throw new Exception("Create instance of prefab", "Prefab data is null");
	}

	public T CreateInstance<T>()
	{
		return _converter.CreateObjectFromStructure<T>(_prefab);
	}
}
