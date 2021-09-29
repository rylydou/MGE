using System;
using System.IO;

namespace OpenGLTest
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				using (var window = new GLWindow())
				{
					window.Run();
				}
			}
			catch (Exception e)
			{
				File.WriteAllText(Environment.CurrentDirectory + "/crash-game.log", e.ToString());
			}
		}
	}
}
