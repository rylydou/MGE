using System;

[AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class ObjectDrawerAttribute : Attribute
{
	public readonly Type type;

	public ObjectDrawerAttribute(Type type)
	{
		this.type = type;
	}
}
