using System;
using System.Collections.Generic;
using System.Reflection;
using MemlDotNet;

namespace MGE.Serialization
{
	public class FeildFinder : IMemlFeildFinder
	{
		const BindingFlags _bindingFlagsGeneral = BindingFlags.Public | BindingFlags.Instance;
		const BindingFlags _bindingFlagsFeild = _bindingFlagsGeneral | BindingFlags.GetField | BindingFlags.SetField;
		const BindingFlags _bindingFlagsProp = _bindingFlagsGeneral | BindingFlags.GetProperty | BindingFlags.SetProperty;

		public MemlVar GetFeild(Type type, string name)
		{
			var feild = type.GetField(name, _bindingFlagsFeild);
			if (feild is not null) return new MemlVar(feild);

			var prop = type.GetProperty(name, _bindingFlagsProp);
			if (prop is not null) return new MemlVar(prop);

			return null;

			// throw new Exception("Feild or prop of");
		}

		public MemlVar[] GetFeilds(Type type)
		{
			var members = type.GetMembers(_bindingFlagsGeneral);
			var feilds = new List<MemlVar>(members.Length);

			foreach (var member in members)
			{
				if (member.GetCustomAttribute<PropAttribute>() is PropAttribute prop)
				{
					if (member.MemberType.HasFlag(MemberTypes.Field))
					{
						feilds.Add(new MemlVar((FieldInfo)member) { order = prop.order });
					}
					else if (member.MemberType.HasFlag(MemberTypes.Property))
					{
						feilds.Add(new MemlVar((PropertyInfo)member));
					}
				}
			}

			return feilds.ToArray();
		}
	}
}