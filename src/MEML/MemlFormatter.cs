using System;

namespace MEML;

public abstract class MemlFormatter
{
	public abstract Type type { get; }

	public abstract object? ReadStructure(MemlValue value, MemlConverter converter);

	public abstract MemlValue WriteObj(object? obj, MemlConverter converter);
}

public abstract class MemlFormatter<T> : MemlFormatter where T : notnull
{
	public sealed override Type type => typeof(T);

	public sealed override object? ReadStructure(MemlValue value, MemlConverter converter)
	{
		if (value is MemlValueNull) return null;

		return Read(value, converter);
	}

	public abstract T Read(MemlValue value, MemlConverter converter);

	public sealed override MemlValue WriteObj(object? obj, MemlConverter converter)
	{
		if (obj is null) return new MemlValueNull();

		return Write((T)obj, converter);
	}

	public abstract MemlValue Write(T obj, MemlConverter converter);
}
