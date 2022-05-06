using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace MGE;

static class Extentions
{
	public static byte[] ToRGBA(this Color[] pixels)
	{
		var length = pixels.Length;
		var data = new byte[length * 4];

		for (int i = 0; i < length; i++)
		{
			var j = i * 4;
			var col = pixels[i];
			data[j + 0] = col.r;
			data[j + 1] = col.g;
			data[j + 2] = col.b;
			data[j + 3] = col.a;
		}

		return data;
	}

	public static Color[] ToColorsFromRGBA(this byte[] data)
	{
		var dataLength = data.Length / 4;
		var pixels = new Color[dataLength];

		for (int i = 0; i < dataLength; i++)
		{
			var j = i * 4;
			pixels[i].r = data[j + 0];
			pixels[i].g = data[j + 1];
			pixels[i].b = data[j + 2];
			pixels[i].a = data[j + 3];
		}

		return pixels;
	}

	public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
	{
		foreach (var item in list)
		{
			action(item);
		}
	}

	/// <summary>
	/// Adds a value to the dictionary, if it already exists then change that value.
	/// </summary>
	/// <param name="dict"></param>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <returns>true and new item was added to the dictionary, false if a value was changed.</returns>
	public static bool Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) where TKey : notnull
	{
		if (!dict.TryAdd(key, value))
		{
			dict[key] = value;
			return false;
		}
		return true;
	}

	public static bool TryGet(this object obj, string propertyName, out object? value)
	{
		var member = obj.GetType().GetMember(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty)[0];

		if (member is FieldInfo field)
		{
			value = field.GetValue(obj);
			return true;
		}
		else if (member is PropertyInfo prop)
		{
			value = prop.GetValue(obj);
			return true;
		}

		value = null;
		return false;
	}

	public static bool TrySet(this object obj, string propertyName, object value)
	{
		var member = obj.GetType().GetMember(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty).FirstOrDefault();

		if (member is FieldInfo field)
		{
			field.SetValue(obj, value);
			return true;
		}
		else if (member is PropertyInfo prop)
		{
			prop.SetValue(obj, value);
			return true;
		}

		return false;
	}

	public static bool TryCall(this object obj, string methodName, object?[] args, out object? returnValue)
	{
		var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, args.Select(arg => arg?.GetType() ?? typeof(object)).ToArray());

		if (method is null)
		{
			returnValue = null;
			return false;
		}

		returnValue = method.Invoke(obj, args);
		return true;
	}

	public static IEnumerable<Body2D> WhereInLayer(this IEnumerable<Body2D> list, Layer layer)
	{
		foreach (var body in list)
		{
			if (body.layer.CheckLayer(layer))
				yield return body;
		}
	}
}
