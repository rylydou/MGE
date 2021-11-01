using System.Diagnostics;
using System.IO;
using System.Text;
using Gtk;

namespace MGE.Editor.GUI.Windows
{
	[EditorWindow("Console")]
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

		class TextBufferTraceListener : TraceListener
		{
			readonly TextBuffer buffer;

			public TextBufferTraceListener(TextBuffer buffer)
			{
				this.buffer = buffer;
			}

			public override void Write(string? message)
			{
				var iter = this.buffer.EndIter;
				this.buffer.Insert(ref iter, message);
			}

			public override void WriteLine(string? message)
			{
				var iter = this.buffer.EndIter;
				// ? Would it be faster to do two writes or a string concat
				this.buffer.Insert(ref iter, message);
				this.buffer.Insert(ref iter, "\n");
			}
		}

		public string[] consoles = new[]
		{
			"Editor",
			"Build",
			"Game",
		};

		public int currentConsole;

		TextView logTextView = new() { Editable = false, };

		public ConsoleWindow() : base(false) { }

		protected override void Draw()
		{

			var consoleWriter = new TextBufferWriter(logTextView.Buffer);
			// Console.SetOut(consoleWriter);
			var traceWriter = new TextBufferTraceListener(logTextView.Buffer);
			Trace.Listeners.Add(traceWriter);

			EditorGUI.StartHorizontal();

			EditorGUI.Label("Console");
			EditorGUI.horizontalExpand = false;
			EditorGUI.Combobox(consoles, currentConsole).onItemIndexChanged += index => currentConsole = index;

			EditorGUI.Flex();

			EditorGUI.horizontalExpand = false;
			EditorGUI.Button("Clear");
			EditorGUI.horizontalExpand = false;
			EditorGUI.Button("Save");

			EditorGUI.End();

			EditorGUI.VerticalOverflow();
			EditorGUI.Add(logTextView);
		}
	}
}
