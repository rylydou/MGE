using System;

namespace MEML;

public abstract class DataConverter
{
	public abstract Type type { get; }

	public abstract StructureValue Write(object? obj);

	public abstract object? Read(StructureValue value, object? existingValue);
}

public abstract class DataConverter<T> : DataConverter where T : notnull
{
	public sealed override Type type => typeof(T);

	public sealed override StructureValue Write(object? obj)
	{
		if (obj is null) return new StructureValueNull();

		return Write((T)obj);
	}

	public abstract StructureValue Write(T obj);

	public sealed override object? Read(StructureValue value, object? existingValue)
	{
		if (value is StructureValueNull) return null;

		return Read(value, existingValue);
	}

	public abstract T Read(StructureValue value, T? existingValue);
}
