using System;

namespace MGE.Editor.GUI
{
	public abstract class PropDrawer
	{
		public abstract Type type { get; }

		public abstract void DrawProp(object? value, Action<object?> setValue);
	}

	public abstract class PropDrawer<T> : PropDrawer
	{
		public sealed override Type type => typeof(T);

		public sealed override void DrawProp(object? value, Action<object?> setValue) => DrawProp((T)value!, val => setValue.Invoke(val));
		protected abstract void DrawProp(T value, Action<T> setValue);
	}
}
