using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MGE;

public class ObjectVarible
{
	public delegate object? GetValue(object? obj);
	public delegate void SetValue(object? obj, object? value);

	public string name;
	public Type type;
	public GetValue getValue;
	public SetValue setValue;

	public ObjectVarible(string name, Type type, GetValue getValue, SetValue setValue)
	{
		this.name = name;
		this.type = type;
		this.getValue = getValue;
		this.setValue = setValue;
	}
}

public class MemlSerializer
{
	public delegate void OnUnusedValue(string name);
	public OnUnusedValue onUnusedValue = (name) => Trace.TraceWarning($"Unused value: {name}");

	public delegate IEnumerable<MemberInfo> GetMembers(Type type);
	public GetMembers getMembers = DefualtGetMembers;
	public static IEnumerable<MemberInfo> DefualtGetMembers(Type type)
	{
		return type.GetMembers(
			BindingFlags.Instance |
			BindingFlags.NonPublic |
			BindingFlags.Public |
			BindingFlags.GetField |
			BindingFlags.SetField |
			BindingFlags.GetProperty |
			BindingFlags.SetProperty
		);
	}

	public delegate IEnumerable<ObjectVarible> VaribleFinder(IEnumerable<MemberInfo> members);
	public VaribleFinder varibleFinder = DefualtGetVaribles;
	public static IEnumerable<ObjectVarible> DefualtGetVaribles(IEnumerable<MemberInfo> members)
	{
		foreach (var memberInfo in members)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				if (fieldInfo.IsInitOnly | fieldInfo.IsLiteral) continue;

				yield return new(memberInfo.Name, fieldInfo.FieldType, (obj) => fieldInfo.GetValue(obj), (obj, value) => fieldInfo.SetValue(obj, value));
			}
			else if (memberInfo is PropertyInfo propInfo)
			{
				if (propInfo.GetMethod is null) continue;

				yield return new(memberInfo.Name, propInfo.PropertyType, (obj) => propInfo.GetValue(obj), (obj, value) => propInfo.SetValue(obj, value));
			}
			else continue;
		}
		yield break;
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

	public MemlValue MemlFromObject(object? obj, Type? impliedType = null)
	{
		if (obj is null) return new MemlNull();

		var type = obj.GetType();

		if (type.IsPrimitive)
		{
			if (obj is bool @bool) return new MemlValue<bool>(MemlType.Bool, @bool);

			if (obj is float @float) return new MemlValue<float>(MemlType.Float, @float);
			if (obj is double @double) return new MemlValue<double>(MemlType.Double, @double);
			if (obj is int @int) return new MemlValue<int>(MemlType.Int, @int);
			if (obj is uint @uint) return new MemlValue<uint>(MemlType.UInt, @uint);
			if (obj is byte @byte) return new MemlValue<byte>(MemlType.Byte, @byte);
			if (obj is char @char) return new MemlValue<char>(MemlType.Char, @char);
			if (obj is short @short) return new MemlValue<short>(MemlType.Short, @short);
			if (obj is ushort @ushort) return new MemlValue<ushort>(MemlType.UShort, @ushort);
			if (obj is long @long) return new MemlValue<long>(MemlType.Long, @long);
			if (obj is ulong @ulong) return new MemlValue<ulong>(MemlType.ULong, @ulong);
		}

		if (obj is string @string) return new MemlValue<string>(MemlType.String, @string);

		if (obj is byte[] binary) return new MemlValue<byte[]>(MemlType.Binary, binary);

		// Array
		if (obj is ICollection collection)
		{
			var memlArray = new MemlArray();
			var contentType = type.IsArray ? type.GetElementType() : GetCollectionElementType(type);

			foreach (var item in collection)
			{
				memlArray.Add(MemlFromObject(item, contentType));
			}

			return memlArray;
		}

		// Object
		var memlObject = new MemlObject();

		if (type != impliedType)
		{
			memlObject[$"!{type.Assembly.GetName().Name}"] = type.FullName ?? throw new Exception();
		}

		foreach (var getter in varibleFinder(getMembers(type)))
		{
			var value = getter.getValue(obj);
			var memlValue = MemlFromObject(value, getter.type);
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

	public T ObjectFromMeml<T>(MemlValue value)
	{
		return (T)ObjectFromMeml(value, typeof(T))!;
	}
	public object? ObjectFromMeml(MemlValue memlValue, Type? impliedType = null)
	{
		if (memlValue.type == MemlType.Object)
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
			var varibles = varibleFinder(getMembers(type));
			foreach (var item in memlValue.pairs)
			{
				if (item.Key[0] == '!') continue;

				var varible = varibles.FirstOrDefault(v => v.name == item.Key);

				if (varible is null)
				{
					onUnusedValue(item.Key);
					continue;
				}

				var value = ObjectFromMeml(item.Value, varible.type);
				varible.setValue(obj, value);
			}

			return obj;
		}

		if (memlValue.type == MemlType.Array)
		{
			if (impliedType is null) throw new NotSupportedException();

			var collectionType = GetCollectionElementType(impliedType);

			if (impliedType.IsArray)
			{
				return memlValue.values.Select(item => ObjectFromMeml(item, collectionType)).ToArray();
			}

			var list = (IList)Activator.CreateInstance(impliedType)!;

			foreach (var item in memlValue.values)
			{
				list.Add(ObjectFromMeml(item));
			}

			return list;

			// var array = ;
		}

		return memlValue.underlyingValue;
	}
}
