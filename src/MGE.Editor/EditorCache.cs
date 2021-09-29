using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MGE.Editor
{
	internal static class EditorCache
	{
		static Dictionary<string, string> propNameCache = new Dictionary<string, string>();

		public static string GetPropertyName(string propName)
		{
			if (propNameCache.TryGetValue(propName, out var name)) return name;

			var sb = new StringBuilder();

			var lastCharWasCapital = false;
			var inAcronym = false;
			for (int i = 0; i < propName.Length; i++)
			{
				var ch = propName[i];

				if (i == 0)
				{
					lastCharWasCapital = char.IsUpper(ch);
					sb.Append(char.ToUpper(ch));
					continue;
				}

				if (char.IsUpper(ch))
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
	}
}
