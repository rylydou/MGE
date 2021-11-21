using System;
using YamlDotNet.Serialization;

namespace MGE.Serialization;

public static class Serializer
{
	static ISerializer yamlSerializer = new SerializerBuilder().WithTypeInspector<TypeInspector>(i => new TypeInspector(i)).Build();
	static IDeserializer yamlDeserializer = new DeserializerBuilder().WithTypeInspector<TypeInspector>(i => new TypeInspector(i)).Build();

	public static string Serialize(object obj) => yamlSerializer.Serialize(obj);

	public static object? Deserialize(string text, Type type) => yamlDeserializer.Deserialize(text, type);
	public static T Deserialize<T>(string text) => yamlDeserializer.Deserialize<T>(text);
}
