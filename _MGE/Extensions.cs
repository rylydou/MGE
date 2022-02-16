using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MGE;

static class Extensions
{
	public static IEnumerable ForEach(this IEnumerable source, Action<object> action)
	{
		foreach (var item in source) action.Invoke(item);
		return source;
	}

	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach (var item in source) action.Invoke(item);
		return source;
	}

	public static int IndexOf<T>(this IEnumerable<T> source, T item)
	{
		var index = 0;

		if (item is null)
		{
			foreach (var entry in source)
			{
				if (entry is null) return index;
				index++;
			}
			return -1;
		}

		foreach (var entry in source)
		{
			if (entry is null) continue;
			if (entry.Equals(item)) return index;
			index++;
		}
		return -1;
	}

	public static IEnumerable<TType> Where<T, TType>(this IEnumerable<T> source) where TType : T
	{
		var typeofTType = typeof(TType);
		return source.Where(item => item is null ? false : typeofTType.Equals(item)).Select(item => (TType)item!);
	}

	public static bool First<T, TType>(this IEnumerable<T> source, [MaybeNullWhen(false)] out TType result) where TType : T
	{
		var typeofTType = typeof(TType);
		var item = source.FirstOrDefault(item => item is null ? false : typeofTType.Equals(item));

		result = default(TType);
		if (item is null) return false;

		result = (TType)item;
		return true;
	}

	public static void Invoke(this EventHandler ev) => ev(null, EventArgs.empty);
	public static void Invoke(this EventHandler ev, object sender) => ev(sender, EventArgs.empty);

	public static void Invoke<T>(this EventHandler<GenericEventArgs<T>> ev, T data) => ev(null, new GenericEventArgs<T>(data));
	public static void Invoke<T>(this EventHandler<GenericEventArgs<T>> ev, object sender, T data) => ev(sender, new GenericEventArgs<T>(data));
}
