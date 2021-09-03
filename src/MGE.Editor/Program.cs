using System;
using Gtk;

namespace MGE.Editor
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application.Init();

			var app = new Application("com.MGE.Editor", GLib.ApplicationFlags.None);
			app.Register(GLib.Cancellable.Current);

			var win = new MainWindow();

			app.AddWindow(win);

			win.Show();
			Application.Run();
		}
	}
}
