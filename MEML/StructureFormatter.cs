using System;

namespace MEML;

public abstract class StructureFormatter
{
	public abstract Type type { get; }

	public abstract object? ReadStructure(StructureValue value, StructureConverter converter);

	public abstract StructureValue WriteObj(object? obj, StructureConverter converter);
}

public abstract class StructureFormatter<T> : StructureFormatter where T : notnull
{
	public sealed override Type type => typeof(T);

	public sealed override object? ReadStructure(StructureValue value, StructureConverter converter)
	{
		if (value is StructureValueNull) return null;

		return Read(value, converter);
	}

	public abstract T Read(StructureValue value, StructureConverter converter);

	public sealed override StructureValue WriteObj(object? obj, StructureConverter converter)
	{
		if (obj is null) return new StructureValueNull();

		return Write((T)obj, converter);
	}

	public abstract StructureValue Write(T obj, StructureConverter converter);
}
