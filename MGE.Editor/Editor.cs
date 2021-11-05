using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Gtk;

namespace MGE.Editor
{
	internal static class Editor
	{
		public static DirectoryInfo assets = new($"{Environment.CurrentDirectory}/Assets");

#if DEBUG
		public static Project project = new($"{Environment.GetEnvironmentVariable("data-dir")}/MGE Projects/Untitled-Game");
#endif

		public static List<EditorContext> contexts = new() { new() };

		#region Property Name

		static Dictionary<string, string> propNameCache = new();

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
			var nextCharShouldBeCapital = true;

			for (int i = 0; i < text.Length; i++)
			{
				var ch = text[i];

				if (ch == '_')
				{
					nextCharShouldBeCapital = true;

					if (i == 0) continue;

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

		public static void ClearCache()
		{
			propNameCache.Clear();
			propNameCache.TrimExcess();
		}

		#region Util

		static Clipboard? _clipboard;
		public static Clipboard clipboard
		{
			get
			{
				if (_clipboard is null)
					_clipboard = Clipboard.GetDefault(MGEEditorWindow.current.Display);
				return _clipboard;
			}
		}

		public static void CopyText(string text)
		{
			clipboard.Text = text;
		}

		public static void OpenFile(string file)
		{
			try
			{
				Process.Start(new ProcessStartInfo(file) { UseShellExecute = true, WorkingDirectory = Editor.project.root.FullName });
			}
			catch
			{
				if (OperatingSystem.IsLinux())
				{
					System.Diagnostics.Process.Start("xdg-open", file);
				}
				else if (OperatingSystem.IsWindows())
				{
					System.Diagnostics.Process.Start("explorer", file);
				}
			}
		}

		public static void ShowFile(string file)
		{
			if (OperatingSystem.IsLinux())
			{
				System.Diagnostics.Process.Start("xdg-open", file);
			}
			else if (OperatingSystem.IsWindows())
			{
				System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{file}\"");
			}
		}

		#endregion
	}
}
