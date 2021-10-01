using System;

namespace MGE.Editor
{
	public class Optional<T>
	{
		public T value
		{
			get
			{
				if (isUnset) throw new Exception("Optional value has not been set");
				return _value!;
			}
			set => Set(value);
		}
		T? _value;

		public bool isSet { get; private set; } = false;
		public bool isUnset { get => !isSet; }

		public Optional() { }
		public Optional(T value) => Set(value);

		public void Set(T value)
		{
			this.isSet = true;
			this._value = value;
		}

		public void Unset() => this.isSet = false;

		public T? GetValueOnDefualt() => isSet ? _value : (T?)default;
		public T GetValueOnDefualt(T defualt) => isSet ? _value! : defualt;
		public T GetValueOnDefualt(Func<T> defualt) => isSet ? _value! : defualt.Invoke();

		public void TrySetValue(Action<T> setValue)
		{
			if (isSet) setValue.Invoke(_value!);
		}

		public static explicit operator T(Optional<T> optional) => optional._value!;
		public static implicit operator Optional<T>(T value) => new Optional<T>(value);
	}
}
