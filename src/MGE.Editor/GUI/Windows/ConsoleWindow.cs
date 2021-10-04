using System;
using System.IO;
using System.Text;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	public class ConsoleWindow : EditorWindow
	{
		public ConsoleWindow() : base("Console")
		{
		}

		protected override void Update()
		{
			var logTextView = new TextView();

			var writer = new Writer(logTextView.Buffer);

			Console.SetOut(writer);

			EditorGUI.Add(logTextView);
		}
	}

	public class Writer : TextWriter
	{
		public override Encoding Encoding => Encoding.Default;

		public readonly TextBuffer buffer;

		public Writer(TextBuffer buffer)
		{
			this.buffer = buffer;
		}

		public override void Write(char[]? buffer)
		{
			var iter = new TextIter();
			this.buffer.Insert(ref iter, buffer?.ToString());
		}
	}
}
