using System;
using System.IO;
using System.Reflection;
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

				var window = new MGEEditor();
				window.ShowAll();

				Application.Run();
			}
			catch (Exception e)
			{
				using (var log = new StreamWriter($"{Environment.CurrentDirectory}/crash-editor.log"))
				{
					log.WriteLine(DateTime.Now.ToString(@"yyyy-MM-dd HH\:mm"));
					log.WriteLine(Assembly.GetAssembly(typeof(Program))!.FullName);
					log.WriteLine();
					log.WriteLine(e.ToString());
				}
				throw;
			}
		}
	}
}
