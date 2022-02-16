using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
// using YamlDotNet.Serialization;

namespace MGE.Serialization;

public static class Serializer
{
	// static ISerializer yamlSerializer = new SerializerBuilder().WithTypeInspector<TypeInspector>(i => new TypeInspector(i)).Build();
	// static IDeserializer yamlDeserializer = new DeserializerBuilder().WithTypeInspector<TypeInspector>(i => new TypeInspector(i)).Build();

	static JsonSerializer jsonSerializer = new()
	{
		TypeNameHandling = TypeNameHandling.Auto,
		Formatting = Formatting.Indented,
		ContractResolver = new JsonContractResolver(),
		NullValueHandling = NullValueHandling.Include,
		FloatFormatHandling = FloatFormatHandling.Symbol,
		MissingMemberHandling = MissingMemberHandling.Ignore,
		ReferenceLoopHandling = ReferenceLoopHandling.Error,
		TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
		FloatParseHandling = FloatParseHandling.Double,
		StringEscapeHandling = StringEscapeHandling.Default,
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
		ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
		ObjectCreationHandling = ObjectCreationHandling.Auto,
		MetadataPropertyHandling = MetadataPropertyHandling.Default,
		DateFormatHandling = DateFormatHandling.IsoDateFormat,
		DateParseHandling = DateParseHandling.DateTimeOffset,
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		DateTimeZoneHandling = DateTimeZoneHandling.Utc,
	};

	public static string Serialize(object? obj)
	{
		var sb = new StringBuilder(256);
		var sw = new StringWriter(sb);
		using (var jsonWriter = new JsonTextWriter(sw))
		{
			jsonWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonWriter, obj);
		}
		return sb.ToString();
	}

	public static T Deserialize<T>(string json)
	{
		var sw = new StringReader(json);
		using (var jsonReader = new JsonTextReader(sw))
		{
			return jsonSerializer.Deserialize<T>(jsonReader) ?? throw new MGEException("Value is null, how?");
		}
	}

	public static object? Deserialize(string json, Type type)
	{
		var sw = new StringReader(json);
		using (var jsonReader = new JsonTextReader(sw))
		{
			return jsonSerializer.Deserialize(jsonReader, type);
		}
	}
}
