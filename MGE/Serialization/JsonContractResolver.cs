using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MGE.Serialization;

public class JsonContractResolver : DefaultContractResolver
{
	static void Ignore(JsonProperty property)
	{
		var prop = (PropAttribute)property.AttributeProvider!.GetAttributes(typeof(PropAttribute), true).FirstOrDefault()!;

		if (prop is null)
		{
			property.Ignored = true;
			return;
		}

		// property.Ignored = false;

		if (prop.name is not null) property.PropertyName = prop.name;
		if (prop.order is not null) property.Order = prop.order;
	}

	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		JsonProperty property = base.CreateProperty(member, memberSerialization);
		Ignore(property);
		return property;
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
		foreach (var property in properties) Ignore(property);
		return properties;
	}
}
