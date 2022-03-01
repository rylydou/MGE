using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MEML;

public class StructureVariable
{
	public delegate object? GetValue(object? obj);
	public delegate void SetValue(object? obj, object? value);

	public string name;
	public Type type;
	public GetValue getValue;
	public SetValue setValue;

	public StructureVariable(string name, Type type, GetValue getValue, SetValue setValue)
	{
		this.name = name;
		this.type = type;
		this.getValue = getValue;
		this.setValue = setValue;
	}
}

public class StructureConverter
{
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

	public delegate StructureVariable? VariableConverter(MemberInfo member);
	public VariableConverter variableConverter = DefualtVariableConverter;
	public static StructureVariable? DefualtVariableConverter(MemberInfo member)
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

	public StructureValue CreateStructureFromObject(object? obj, Type? impliedType = null)
	{
		if (obj is null) return new StructureValueNull();

		var type = obj.GetType();

		if (type.IsPrimitive)
		{
			if (obj is bool @bool) return new MemlValue<bool>(StructureType.Bool, @bool);

			if (obj is float @float) return new MemlValue<float>(StructureType.Float, @float);
			if (obj is double @double) return new MemlValue<double>(StructureType.Double, @double);
			if (obj is int @int) return new MemlValue<int>(StructureType.Int, @int);
			if (obj is uint @uint) return new MemlValue<uint>(StructureType.UInt, @uint);
			if (obj is byte @byte) return new MemlValue<byte>(StructureType.Byte, @byte);
			if (obj is char @char) return new MemlValue<char>(StructureType.Char, @char);
			if (obj is short @short) return new MemlValue<short>(StructureType.Short, @short);
			if (obj is ushort @ushort) return new MemlValue<ushort>(StructureType.UShort, @ushort);
			if (obj is long @long) return new MemlValue<long>(StructureType.Long, @long);
			if (obj is ulong @ulong) return new MemlValue<ulong>(StructureType.ULong, @ulong);
		}

		if (obj is string @string) return new MemlValue<string>(StructureType.String, @string);

		if (obj is byte[] binary) return new MemlValue<byte[]>(StructureType.Binary, binary);

		// Array
		if (obj is ICollection collection)
		{
			var memlArray = new StructureArray();
			var contentType = type.IsArray ? type.GetElementType() : GetCollectionElementType(type);

			foreach (var item in collection)
			{
				memlArray.Add(CreateStructureFromObject(item, contentType));
			}

			return memlArray;
		}

		// Object
		var memlObject = new StructureObject();

		if (type != impliedType)
		{
			memlObject[$"!{type.Assembly.GetName().Name}"] = type.FullName ?? throw new Exception();
		}

		IEnumerable<StructureVariable> variables = memberFinder(type).Select(m => variableConverter(m)).Where(v => v is not null)!;
		foreach (var getter in variables)
		{
			var value = getter.getValue(obj);
			var memlValue = CreateStructureFromObject(value, getter.type);
			memlObject[getter.name] = memlValue;
		}

		return memlObject;
	}

	static Type? GetCollectionElementType(Type type)
	{
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

	public T CreateObjectFromStructure<T>(StructureValue value)
	{
		return (T)CreateObjectFromStructure(value, typeof(T))!;
	}
	public object? CreateObjectFromStructure(StructureValue memlValue, Type? impliedType = null)
	{
		if (memlValue.type == StructureType.Object)
		{
			Type type;

			var firstPair = memlValue.pairs.First();
			if (firstPair.Key[0] == '!')
			{
				var asmName = firstPair.Key.Remove(0, 1);
				var fullTypeName = firstPair.Value.String;

				type = typeFinder(asmName, fullTypeName) ??
					throw new Exception($"Connot find type '{fullTypeName}' from '{asmName}'");
			}
			else
			{
				type = impliedType ??
					throw new Exception("Meml object doesn't define its type");
			}

			var obj = Activator.CreateInstance(type);
			foreach (var item in memlValue.pairs)
			{
				if (item.Key[0] == '!') continue;

				var members = type.GetMember(item.Key, suggestedBindingFlags);
				if (members.Length != 1)
				{
					onUnusedValue(item.Key);
					continue;
				}

				var variable = variableConverter(members[0]) ?? throw new Exception();

				var value = CreateObjectFromStructure(item.Value, variable.type);
				variable.setValue(obj, value);
			}

			return obj;
		}

		if (memlValue.type == StructureType.Array)
		{
			if (impliedType is null) throw new NotSupportedException();

			var collectionType = GetCollectionElementType(impliedType);

			if (impliedType.IsArray)
			{
				return memlValue.values.Select(item => CreateObjectFromStructure(item, collectionType)).ToArray();
			}

			var list = (IList)Activator.CreateInstance(impliedType)!;

			foreach (var item in memlValue.values)
			{
				list.Add(CreateObjectFromStructure(item));
			}

			return list;
		}

		return memlValue.underlyingValue;
	}

	public void PopulateObjectFromStructure(ref object obj, in StructureValue value)
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

			var variable = variableConverter(members[0]) ?? throw new Exception();

			var memberValue = CreateObjectFromStructure(item.Value, variable.type);
			variable.setValue(obj, memberValue);
		}
	}
}
