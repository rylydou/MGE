using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MEML;

public class ObjectConverter : DataConverter
{
	public override Type type => throw new NotImplementedException();

	Dictionary<Type, DataConverter> _converters = new();
	public void RegisterConverter(DataConverter converter)
	{
		_converters.Add(converter.type, converter);
	}

	public override object? ReadStructure(StructureValue value, StructureConverter converter)
	{
		if (value.type == StructureType.Object)
		{
			Type type;

			var firstPair = value.pairs.First();
			if (firstPair.Key[0] == '!')
			{
				var asmName = firstPair.Key.Remove(0, 1);
				var fullTypeName = firstPair.Value.String;

				type = converter.typeFinder(asmName, fullTypeName) ?? throw new Exception($"Connot find type '{fullTypeName}' from '{asmName}'");
			}
			else
			{
				type = impliedType ?? throw new Exception("Meml object doesn't define its type");
			}

			var obj = Activator.CreateInstance(type);
			foreach (var item in value.pairs)
			{
				if (item.Key[0] == '!') continue;

				var members = type.GetMember(item.Key, StructureConverter.suggestedBindingFlags);
				if (members.Length != 1)
				{
					converter.onUnusedValue(item.Key);
					continue;
				}

				var variable = converter.variableConverter(members[0]) ?? throw new Exception();

				var objValue = ReadStructure(item.Value, converter, variable.type);
				variable.setValue(obj, objValue);
			}

			return obj;
		}

		if (value.type == StructureType.Array)
		{
			if (impliedType is null) throw new NotSupportedException();

			var collectionType = GetCollectionElementType(impliedType);

			if (impliedType.IsArray)
			{
				return value.values.Select(item => ReadStructure(item, converter, collectionType)).ToArray();
			}

			var list = (IList)Activator.CreateInstance(impliedType)!;

			foreach (var item in value.values)
			{
				list.Add(CreateObjectFromStructure(item));
			}

			return list;
		}

		return value.underlyingValue;

		Type? GetCollectionElementType(Type type)
		{
			var etype = typeof(IList<>);

			foreach (var bt in type.GetInterfaces())
			{
				if (bt.IsGenericType && bt.GetGenericTypeDefinition() == etype)
				{
					return bt.GetGenericArguments()[0];
				}
			}

			return null;
		}
	}

	public override StructureValue WriteObj(object? obj, StructureConverter converter)
	{
	}
}

public class ArrayConverter : DataConverter
{
	public override Type type => throw new NotImplementedException();

	public override object? ReadStructure(StructureValue value, StructureConverter converter)
	{
		throw new NotImplementedException();
	}

	public override StructureValue WriteObj(object? obj, StructureConverter converter)
	{
		throw new NotImplementedException();
	}
}

public class ByteConverter : DataConverter<byte>
{
	public override byte Read(StructureValue value, StructureConverter converter) => value.Byte;
	public override StructureValue Write(byte obj, StructureConverter converter) => (StructureValue)obj;
}

public class SByteConverter : DataConverter<sbyte>
{
	public override sbyte Read(StructureValue value, StructureConverter converter) => value.SByte;
	public override StructureValue Write(sbyte obj, StructureConverter converter) => (StructureValue)obj;
}

public class CharConverter : DataConverter<char>
{
	public override char Read(StructureValue value, StructureConverter converter) => value.Char;
	public override StructureValue Write(char obj, StructureConverter converter) => (StructureValue)obj;
}

public class ShortConverter : DataConverter<short>
{
	public override short Read(StructureValue value, StructureConverter converter) => value.Short;
	public override StructureValue Write(short obj, StructureConverter converter) => (StructureValue)obj;
}

public class UShortConverter : DataConverter<ushort>
{
	public override ushort Read(StructureValue value, StructureConverter converter) => value.UShort;
	public override StructureValue Write(ushort obj, StructureConverter converter) => (StructureValue)obj;
}

public class IntConverter : DataConverter<int>
{
	public override int Read(StructureValue value, StructureConverter converter) => value.Int;
	public override StructureValue Write(int obj, StructureConverter converter) => (StructureValue)obj;
}

public class UIntConverter : DataConverter<uint>
{
	public override uint Read(StructureValue value, StructureConverter converter) => value.UInt;
	public override StructureValue Write(uint obj, StructureConverter converter) => (StructureValue)obj;
}

public class LongConverter : DataConverter<long>
{
	public override long Read(StructureValue value, StructureConverter converter) => value.Long;
	public override StructureValue Write(long obj, StructureConverter converter) => (StructureValue)obj;
}

public class ULongConverter : DataConverter<ulong>
{
	public override ulong Read(StructureValue value, StructureConverter converter) => value.ULong;
	public override StructureValue Write(ulong obj, StructureConverter converter) => (StructureValue)obj;
}

public class FloatConverter : DataConverter<float>
{
	public override float Read(StructureValue value, StructureConverter converter) => value.Float;
	public override StructureValue Write(float obj, StructureConverter converter) => (StructureValue)obj;
}

public class DoubleConverter : DataConverter<double>
{
	public override double Read(StructureValue value, StructureConverter converter) => value.Double;
	public override StructureValue Write(double obj, StructureConverter converter) => (StructureValue)obj;
}

public class DecimalConverter : DataConverter<decimal>
{
	public override decimal Read(StructureValue value, StructureConverter converter) => value.Decimal;
	public override StructureValue Write(decimal obj, StructureConverter converter) => (StructureValue)obj;
}

public class BytesConverter : DataConverter<byte[]>
{
	public override byte[] Read(StructureValue value, StructureConverter converter) => value.Bytes;
	public override StructureValue Write(byte[] obj, StructureConverter converter) => (StructureValue)obj;
}
