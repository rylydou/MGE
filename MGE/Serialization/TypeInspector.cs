using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace MGE.Serialization;

public class TypeInspector : TypeInspectorSkeleton
{
	readonly ITypeInspector innerTypeDescriptor;

	public TypeInspector(ITypeInspector innerTypeDescriptor)
	{
		this.innerTypeDescriptor = innerTypeDescriptor;
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		return innerTypeDescriptor.GetProperties(type, container).Where(p => p.GetCustomAttribute<PropAttribute>() is not null);
	}
}
