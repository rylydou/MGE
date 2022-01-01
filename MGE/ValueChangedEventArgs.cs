using System;

namespace MGE;

public class ValueChangedEventArgs<T> : EventArgs
{
	public T OldValue { get; private set; }
	public T NewValue { get; private set; }

	public ValueChangedEventArgs(T oldValue, T newValue)
	{
		OldValue = oldValue;
		NewValue = newValue;
	}
}

public class ValueChangingEventArgs<T> : CancellableEventArgs
{
	public T OldValue { get; private set; }
	public T NewValue { get; set; }

	public ValueChangingEventArgs(T oldValue, T newValue)
	{
		OldValue = oldValue;
		NewValue = newValue;
	}
}
