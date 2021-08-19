using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace MGE.Serialization
{
	public static class Serializer
	{
		// TODO: Don't use BinaryFormatter
		public static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();
		static JsonSerializer _jsonSerializer;
		public static JsonSerializer jsonSerializer
		{
			get
			{
				if (_jsonSerializer is null)
				{
					_jsonSerializer = new JsonSerializer()
					{
						TypeNameHandling = TypeNameHandling.Objects,
						Formatting = Formatting.Indented,
					};
					AddConverter(new Vector2JsonConverter());
					AddConverter(new Vector2IntJsonConverter());
					AddConverter(new RectJsonConverter());
					AddConverter(new RectIntJsonConverter());
					AddConverter(new ColorJsonConverter());
					AddConverter(new FolderJsonConverter());
				}
				return _jsonSerializer;
			}
		}

		public static void AddConverter(JsonConverter converter)
		{
			jsonSerializer.Converters.Add(converter);
		}

		public static void SerializeReadable(TextWriter writer, object obj)
		{
			jsonSerializer.Serialize(writer, obj);
		}

		public static T DeserializeReadable<T>(TextReader reader)
		{
			T result = default;
			using (var jsonReader = new JsonTextReader(reader))
				result = jsonSerializer.Deserialize<T>(jsonReader);
			return result;
		}

		public static void SerializeBinary(Stream stream, object obj)
		{
			binaryFormatter.Serialize(stream, obj);
		}

		public static T DeserializeBinary<T>(Stream stream)
		{
			return (T)binaryFormatter.Deserialize(stream);
		}
	}
}