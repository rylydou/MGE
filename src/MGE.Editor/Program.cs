using System;
using System.IO;
using Gtk;

namespace MGE.Editor
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
				Application.Init();

				var win = new EditorApplicationWindow();
				win.ShowAll();

				Application.Run();
			}
			catch (Exception e)
			{
				File.WriteAllText(Environment.CurrentDirectory + "/crash-editor.log", e.ToString());
			}
		}
	}
}
