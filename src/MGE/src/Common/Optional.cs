using System;
using System.Diagnostics.CodeAnalysis;

namespace MGE;

public class Optional<T>
{
	public T value
	{
		get
		{
			if (isUnset) throw new MGException("Optional value has not been set");
			return _value!;
		}
		set => Set(value);
	}
	T? _value;

	public bool isSet { get; set; } = false;
	public bool isUnset { get => !isSet; }

	public Optional() { }
	public Optional(T value) => Set(value);

	public void Set(T value)
	{
		this.isSet = true;
		this._value = value;
	}

	public void Unset() => this.isSet = false;

	public T? TryGetValue() => isSet ? _value : default(T?);
	public T TryGetValue(T defaultValue) => isSet ? _value! : defaultValue;
	public T TryGetValue(Func<T> defaultValue) => isSet ? _value! : defaultValue();
	public bool TryGetValue([NotNullWhen(true)] out T value)
	{
		if (isSet)
		{
			value = this._value!;
			return true;
		}
		value = default(T)!;
		return false;
	}
	public bool TryGetValue(Action<T> hasValue)
	{
		if (isSet)
		{
			hasValue(_value!);
			return true;
		}
		return false;
	}

	public bool SetIfUnset(T value)
	{
		if (isUnset)
		{
			Set(value);
			return true;
		}
		return false;
	}
	public bool SetIfUnset(Func<T> getValue)
	{
		if (isUnset)
		{
			Set(getValue());
			return true;
		}
		return false;
	}

	public static explicit operator T(Optional<T> optional) => optional._value!;
	public static implicit operator Optional<T>(T value) => new Optional<T>(value);
}
