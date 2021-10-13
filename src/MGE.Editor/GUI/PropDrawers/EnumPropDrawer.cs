using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MGE.Editor.GUI.PropDrawers
{
	public class EnumPropDrawer : PropDrawer
	{
		public override Type type => throw new NotImplementedException();

		public override void DrawProp(object? value, Action<object?> setValue)
		{
			if (value is null) throw new Exception();

			var type = value.GetType();
			var options = Enum.GetNames(type);

			if (type.GetCustomAttribute<FlagsAttribute>() is not null)
			{
				// Flags

				// var values = GetFlags((Enum)value);

				// foreach (var option in options)
				// {
				// 	EditorGUI.StartProperty(option);

				// 	EditorGUI.Checkbox(values.Any());
				// }

				throw new NotImplementedException();
			}
			else
			{
				// Single

				EditorGUI.Combobox(options, Enum.GetName(type, value)!).onItemChanged += item => setValue.Invoke(Enum.Parse(type, item));
			}
		}

		static IEnumerable<Enum> GetFlags(Enum e) => Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
	}
}
