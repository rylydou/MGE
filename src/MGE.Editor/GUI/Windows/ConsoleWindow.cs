using System;
using System.IO;
using System.Text;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	public class ConsoleWindow : EditorWindow
	{
		class TextBufferWriter : TextWriter
		{
			public override Encoding Encoding => Encoding.Default;

			public readonly TextBuffer buffer;

			public TextBufferWriter(TextBuffer buffer)
			{
				this.buffer = buffer;
			}

			public override void Write(char[]? buffer)
			{
				if (buffer is null) return;

				var iter = this.buffer.EndIter;
				this.buffer.Insert(ref iter, new string(buffer));
			}
		}

		public ConsoleWindow() : base("Console")
		{
		}

		protected override void Update()
		{
			var logTextView = new TextView() { Editable = false, };

			var tw = new TextBufferWriter(logTextView.Buffer);

			// Console.SetOut(tw);

			EditorGUI.Header("General");

			EditorGUI.Add(logTextView);

		}
	}
}
