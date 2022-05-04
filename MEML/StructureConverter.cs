using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace MEML;

public class ObjectMember
{
	public delegate object? GetValue(object? obj);
	public delegate void SetValue(object? obj, object? value);

	public string name;
	public Type type;
	public GetValue getValue;
	public SetValue setValue;

	public ObjectMember(string name, Type type, GetValue getValue, SetValue setValue)
	{
		this.name = name;
		this.type = type;
		this.getValue = getValue;
		this.setValue = setValue;
	}
}

public class StructureConverter
{
	const DynamicallyAccessedMemberTypes dynamicallyAccessedMemberTypes =
		DynamicallyAccessedMemberTypes.PublicFields |
		DynamicallyAccessedMemberTypes.PublicProperties |
		DynamicallyAccessedMemberTypes.PublicConstructors |
		DynamicallyAccessedMemberTypes.PublicParameterlessConstructor |
		DynamicallyAccessedMemberTypes.PublicMethods |
		DynamicallyAccessedMemberTypes.NonPublicFields |
		DynamicallyAccessedMemberTypes.NonPublicProperties |
		DynamicallyAccessedMemberTypes.NonPublicConstructors |
		DynamicallyAccessedMemberTypes.NonPublicMethods
	;

	public delegate void OnUnusedValue(string name);
	public OnUnusedValue onUnusedValue = (name) => Trace.TraceWarning($"Unused value: {name}");

	public const BindingFlags suggestedBindingFlags =
		BindingFlags.Instance |
		BindingFlags.NonPublic |
		BindingFlags.Public |
		BindingFlags.GetField |
		BindingFlags.SetField |
		BindingFlags.GetProperty |
		BindingFlags.SetProperty;

	public delegate IEnumerable<MemberInfo> GetMembers(Type type);
	public GetMembers memberFinder = DefualtGetMembers;
	public static IEnumerable<MemberInfo> DefualtGetMembers(Type type)
	{
		return type.GetMembers(suggestedBindingFlags);
	}

	public delegate ObjectMember? MemberConverter(MemberInfo member);
	public MemberConverter memberConverter = DefualtMemberConverter;
	public static ObjectMember? DefualtMemberConverter(MemberInfo member)
	{
		if (member is FieldInfo field)
		{
			if (field.IsInitOnly | field.IsLiteral) return null;

			return new(member.Name, field.FieldType, (obj) => field.GetValue(obj), (obj, value) => field.SetValue(obj, value));
		}
		else if (member is PropertyInfo property)
		{
			if (property.GetMethod is null) return null;

			return new(member.Name, property.PropertyType, (obj) => property.GetValue(obj), (obj, value) => property.SetValue(obj, value));
		}

		return null;
	}

	public delegate Type? TypeFinder(string asmName, string fullTypeName);
	public TypeFinder typeFinder = (asmName, fullTypeName) =>
	{
		var asm =
			AppDomain.CurrentDomain.GetAssemblies()
			.SingleOrDefault(assembly => assembly.GetName().Name == asmName) ??
			throw new Exception($"Assembly '{asmName}' not found");

		return
			asm.GetType(fullTypeName) ??
			throw new Exception($"Type '{fullTypeName}' not found in '{asmName}'");
	};

	public delegate StructureValue ToStructureHandler<T>(T obj);
	delegate StructureValue ToStructureHandler(object obj);

	public delegate T ToObjectHandler<T>(StructureValue value);
	delegate object ToObjectHandler(StructureValue value);

	Dictionary<Type, (ToStructureHandler toStructure, ToObjectHandler toObject)> _converters = new();

	public void RegisterConverter<T>(ToStructureHandler<T> toStructure, ToObjectHandler<T> toObject) where T : notnull
	{
		StructureValue ToStructure(object obj) => toStructure.Invoke((T)obj);
		object ToObject(StructureValue value) => toObject.Invoke(value);

		_converters.Add(typeof(T), (ToStructure, ToObject));
	}

	public StructureValue CreateStructureFromObject(object? obj, Type? impliedType = null)
	{
		if (obj is null) return new StructureValueNull();

		var type = obj.GetType();

		if (_converters.TryGetValue(type, out var converter))
		{
			return converter.toStructure.Invoke(obj);
		}

		if (type.IsPrimitive)
		{
			if (obj is bool @bool) return new StructureValue<bool>(StructureType.Bool, @bool);

			if (obj is float @float) return new StructureValue<float>(StructureType.Float, @float);
			if (obj is double @double) return new StructureValue<double>(StructureType.Double, @double);
			if (obj is int @int) return new StructureValue<int>(StructureType.Int, @int);
			if (obj is uint @uint) return new StructureValue<uint>(StructureType.UInt, @uint);
			if (obj is byte @byte) return new StructureValue<byte>(StructureType.Byte, @byte);
			if (obj is char @char) return new StructureValue<char>(StructureType.Char, @char);
			if (obj is short @short) return new StructureValue<short>(StructureType.Short, @short);
			if (obj is ushort @ushort) return new StructureValue<ushort>(StructureType.UShort, @ushort);
			if (obj is long @long) return new StructureValue<long>(StructureType.Long, @long);
			if (obj is ulong @ulong) return new StructureValue<ulong>(StructureType.ULong, @ulong);
		}

		if (obj is string @string) return new StructureValue<string>(StructureType.String, @string);

		if (obj is byte[] binary) return new StructureValue<byte[]>(StructureType.Binary, binary);

		// Array
		if (obj is ICollection collection)
		{
			var structArray = new StructureArray();
			var contentType = type.IsArray ? type.GetElementType() : GetElementType(type);

			foreach (var item in collection)
			{
				structArray.Add(CreateStructureFromObject(item, contentType));
			}

			return structArray;
		}

		// Object
		var memlObject = new StructureObject();

		if (type != impliedType)
		{
			memlObject[$"!{type.Assembly.GetName().Name}"] = type.FullName ?? throw new Exception();
		}

		IEnumerable<ObjectMember> variables = memberFinder(type).Select(m => memberConverter(m)).Where(v => v is not null)!;
		foreach (var getter in variables)
		{
			var value = getter.getValue(obj);
			var structValue = CreateStructureFromObject(value, getter.type);
			memlObject[getter.name] = structValue;
		}

		return memlObject;
	}

	static Type? GetElementType(Type type)
	{
		if (type.IsArray)
		{
			return type.GetElementType();
		}

		var etype = typeof(IList<>);

		foreach (var bt in type.GetInterfaces())
		{
			if (bt.IsGenericType && bt.GetGenericTypeDefinition() == etype)
			{
				return bt.GetGenericArguments()[0];
			}
		}

		return null;
	}

	public T CreateObjectFromStructure<T>(StructureValue value, params object?[] args)
	{
		return (T)CreateObjectFromStructure(value, typeof(T), args)!;
	}
	public object? CreateObjectFromStructure(StructureValue value, Type? impliedType = null, params object?[] args)
	{
		// Try to use the explict converter
		if (impliedType is not null)
		{
			if (_converters.TryGetValue(impliedType, out var converter))
			{
				return converter.toObject.Invoke(value);
			}
		}

		// No explict converter exists? Then do it automatically...

		if (value.type == StructureType.Object)
		{
			// Create an object

			// Find the !Assembly: 'Namespace.Type' in the object, it should be the first key value
			var firstPair = value.pairs.First();
			if (firstPair.Key[0] == '!')
			{
				var asmName = firstPair.Key.Remove(0, 1);
				var fullTypeName = firstPair.Value.@string;

				// Convert the string to a type
				var foundType = typeFinder(asmName, fullTypeName) ??
					throw new Exception($"Connot find type '{fullTypeName}' from '{asmName}'");

				if (foundType is not null)
					impliedType = foundType;
			}

			// If no type is found then give up
			if (impliedType is null)
				throw new Exception("Meml object doesn't define its type");

			// Create an instance of the object using the constructor and arguments
			var obj = Activator.CreateInstance(impliedType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance, null, args, null);

			// Loop over all the pairs in the structure
			foreach (var item in value.pairs)
			{
				// Skip over 'meta' pairs
				if (item.Key[0] == '!') continue;

				// Find the member with the name in the type
				var members = impliedType.GetMember(item.Key, suggestedBindingFlags);

				// If there are no members with that name then give up, or if there are too many then freakout
				if (members.Length != 1)
				{
					onUnusedValue(item.Key);
					continue;
				}

				// Abstract away field vs property nonseance
				var variable = memberConverter(members[0]) ?? throw new Exception();

				// Convert the value in the pair
				var memberValue = CreateObjectFromStructure(item.Value, variable.type);

				// Set the member to the value
				variable.setValue(obj, memberValue);
			}

			return obj;
		}

		if (value.type == StructureType.Array)
		{
			// Create an array

			// The array must have a implied type to know content type
			if (impliedType is null) throw new NotSupportedException();

			// Get the content type
			var elementType = GetElementType(impliedType);

			if (impliedType.IsArray)
			{
				// If it is an array then thats easy,
				//  convert the elements to an object the return that.

				var arr = Array.CreateInstance(elementType ?? throw new Exception(), value.count);
				for (int i = 0; i < value.count; i++)
				{
					arr.SetValue(CreateObjectFromStructure(value[i], elementType), i);
				}

				return arr;
			}


			// Or else then it is a list

			// Create the list
			var list = (IList)Activator.CreateInstance(impliedType, true)!;

			// Add the converted elements to the list
			foreach (var item in value.values)
			{
				list.Add(CreateObjectFromStructure(item, elementType, args));
			}

			return list;
		}

		// Return the primitive
		return value.rawValue;
	}

	// TODO  Implement
	public void PopulateObjectFromStructure(ref object obj, in StructureValue value, params object?[] args)
	{
		var type = obj.GetType();

		foreach (var item in value.pairs)
		{
			if (item.Key[0] == '!') continue;

			var members = type.GetMember(item.Key, suggestedBindingFlags);
			if (members.Length != 1)
			{
				onUnusedValue(item.Key);
				continue;
			}

			var variable = memberConverter(members[0]) ?? throw new Exception();

			var memberValue = CreateObjectFromStructure(item.Value, variable.type, args);
			variable.setValue(obj, memberValue);
		}
	}
}
