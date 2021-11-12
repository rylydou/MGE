using System;
using System.Collections;
using System.Collections.Generic;

namespace MGE;

static class Extensions
{
	public static IEnumerable ForEach(this IEnumerable enumerable, Action<object> action)
	{
		foreach (var item in enumerable) action.Invoke(item);
		return enumerable;
	}

	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
	{
		foreach (var item in enumerable) action.Invoke(item);
		return enumerable;
	}
}
