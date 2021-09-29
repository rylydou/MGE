using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MGE.Editor.GUI;
using MGE.Editor.GUI.PropDrawers;

namespace MGE.Editor
{
	internal static class Editor
	{
		static Editor()
		{
			RegisterPropDrawer(new BoolPropDrawer());

			RegisterPropDrawer(new IntPropDrawer());
			RegisterPropDrawer(new FloatPropDrawer());

			RegisterPropDrawer(new StringPropDrawer());
		}

		public static object? selectedObject
		{
			get => _selectedObject;
			set
			{
				_selectedObject = value;
				onSelectionChanged.Invoke();
			}
		}
		static object? _selectedObject = new TestNode();
		public static Action onSelectionChanged = () => { };

		#region Property Name

		static Dictionary<string, string> propNameCache = new Dictionary<string, string>();

		public static string GetPropertyName(string propName)
		{
			if (propNameCache.TryGetValue(propName, out var name)) return name;

			var sb = new StringBuilder();

			var lastCharWasCapital = false;
			var inAcronym = false;
			var nextCharShouldBeCapital = false;
			for (int i = 0; i < propName.Length; i++)
			{
				var ch = propName[i];

				if (i == 0)
				{
					lastCharWasCapital = char.IsUpper(ch);
					sb.Append(char.ToUpper(ch));
					continue;
				}

				if (ch == '_')
				{
					nextCharShouldBeCapital = true;
					sb.Append(' ');

					continue;
				}

				if (nextCharShouldBeCapital)
				{
					nextCharShouldBeCapital = false;

					sb.Append(char.ToUpper(ch));

					lastCharWasCapital = true;
					inAcronym = false;

					continue;
				}

				if (char.IsUpper(ch) || char.IsDigit(ch))
				{
					if (lastCharWasCapital)
					{
						sb.Append(ch);

						inAcronym = true;
					}
					else
					{
						sb.Append(' ');
						sb.Append(ch);
					}

					lastCharWasCapital = true;
				}
				else
				{
					if (inAcronym)
					{
						sb.Insert(sb.Length - 1, ' ');
					}

					sb.Append(ch);

					lastCharWasCapital = false;
					inAcronym = false;
				}
			}

			name = sb.ToString();
			propNameCache.Add(propName, name);
			return name;
		}

		#endregion

		#region Property Drawer

		static Dictionary<Type, PropDrawer> typeToPropDrawer = new Dictionary<Type, PropDrawer>();

		static PropDrawer fallbackPropDrawer = new FallbackPropDrawer();

		public static bool RegisterPropDrawer(PropDrawer propDrawer)
		{
			var type = propDrawer.type;

			if (typeToPropDrawer.TryAdd(type, propDrawer)) return true;
			typeToPropDrawer[type] = propDrawer;
			return false;
		}

		public static PropDrawer GetPropDrawer(Type type) => typeToPropDrawer.GetValueOrDefault(type, fallbackPropDrawer);

		#endregion

		public static void ClearCache()
		{
			propNameCache.Clear();
			propNameCache.TrimExcess();
		}
	}
}
