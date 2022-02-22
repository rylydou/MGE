using System;
using System.Collections;
using System.Collections.Generic;
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

	public delegate IEnumerable<ObjectVarible> GetVaribles(IEnumerable<MemberInfo> members);
	public GetVaribles getVaribles = DefualtGetVaribles;
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

	public MemlValue ValueFromObject(object? obj, Type? impliedType = null)
	{
		if (obj is null) return new MemlNull();

		var type = obj.GetType();

		if (type.IsPrimitive)
		{
			if (obj is float @float) return new MemlValue<float>(MemlType.Number, @float);
			if (obj is double @double) return new MemlValue<double>(MemlType.Number, @double);
			if (obj is byte @byte) return new MemlValue<byte>(MemlType.Number, @byte);
			if (obj is char @char) return new MemlValue<char>(MemlType.Number, @char);
			if (obj is short @short) return new MemlValue<short>(MemlType.Number, @short);
			if (obj is ushort @ushort) return new MemlValue<ushort>(MemlType.Number, @ushort);
			if (obj is int @int) return new MemlValue<int>(MemlType.Number, @int);
			if (obj is uint @uint) return new MemlValue<uint>(MemlType.Number, @uint);
			if (obj is long @long) return new MemlValue<long>(MemlType.Number, @long);
			if (obj is ulong @ulong) return new MemlValue<ulong>(MemlType.Number, @ulong);

			if (obj is bool @bool) return new MemlValue<bool>(MemlType.Bool, @bool);
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
				memlArray.Add(ValueFromObject(item, contentType));
			}

			return memlArray;
		}

		// Object
		var memlObject = new MemlObject();

		if (type != impliedType)
		{
			memlObject["!"] = type.FullName;
		}

		foreach (var getter in getVaribles(getMembers(type)))
		{
			var value = getter.getValue(obj);
			var memlValue = ValueFromObject(value, getter.type);
			memlObject[getter.name] = memlValue;
		}

		return memlObject;
	}

	static Type? GetCollectionElementType(Type type)
	{
		var etype = typeof(ICollection<>);

		foreach (var bt in type.GetInterfaces())
		{
			if (bt.IsGenericType && bt.GetGenericTypeDefinition() == etype)
			{
				return bt.GetGenericArguments()[0];
			}
		}

		return null;
	}
}
