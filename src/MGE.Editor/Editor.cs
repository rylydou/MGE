using System;
using System.Collections.Generic;
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

			RegisterPropDrawer(new Vector2PropDrawer());
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
		static object? _selectedObject = TestNode.root;
		public static Action onSelectionChanged = () => { };

		#region Property Name

		static Dictionary<string, string> propNameCache = new Dictionary<string, string>();

		public static string GetPropertyName(string propName)
		{
			if (propNameCache.TryGetValue(propName, out var name)) return name;

			name = CodeToPrettyName(propName);

			propNameCache.Add(propName, name);
			return name;
		}

		static string CodeToPrettyName(string text)
		{
			var sb = new StringBuilder();

			var lastCharWasCapital = false;
			var inAcronym = false;
			var nextCharShouldBeCapital = false;

			for (int i = 0; i < text.Length; i++)
			{
				var ch = text[i];

				if (ch == '_')
				{
					if (i == 0) continue;

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

			return sb.ToString();
		}

		#endregion

		#region Property Drawer

		static Dictionary<Type, PropDrawer> typeToPropDrawer = new Dictionary<Type, PropDrawer>();

		static PropDrawer enumPropDrawer = new EnumPropDrawer();
		static PropDrawer fallbackPropDrawer = new FallbackPropDrawer();

		public static bool RegisterPropDrawer(PropDrawer propDrawer)
		{
			var type = propDrawer.type;

			if (typeToPropDrawer.TryAdd(type, propDrawer)) return true;
			typeToPropDrawer[type] = propDrawer;
			return false;
		}

		public static PropDrawer GetPropDrawer(Type type)
		{
			if (type.IsEnum) return enumPropDrawer;
			return typeToPropDrawer.GetValueOrDefault(type, fallbackPropDrawer);
		}

		#endregion

		public static void ClearCache()
		{
			propNameCache.Clear();
			propNameCache.TrimExcess();
		}
	}
}
