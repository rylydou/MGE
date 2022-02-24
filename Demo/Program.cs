using System;
using System.Linq;
using System.Reflection;
using Demo;
using MGE;
using MGE.GLFW;
using MGE.OpenGL;

// // System Module (this is Mandatory)
// App.modules.Register<GLFW_System>();

// // Graphics Module (not Mandatory but required for drawing anything)
// App.modules.Register<GL_Graphics>();

// // Register the custom Game Module to run your code
// App.modules.Register<Game>();

// // Start the Application with a single 1280x720 Window
// App.Start("My Application", 1280, 720);

// System.Console.WriteLine(Convert.ToBase64String(new byte[] { 0, 1, 2, 3, 4, 5 }));

var serilizer = new MemlSerializer()
{
	getMembers = (type) => MemlSerializer.DefualtGetMembers(type).Where(m => m.GetCustomAttribute<SaveAttribute>() is not null),
};

Log.StartStopwatch("File to MEML");
var meml = MemlValue.FromFile(Folder.data.GetFile("in.meml"));
Log.EndStopwatch();

Log.StartStopwatch("MEML to Object");
var obj = serilizer.ObjectFromMeml<TestObject>(meml);
Log.EndStopwatch();

Log.StartStopwatch("Object to MEML");
meml = serilizer.MemlFromObject(obj);
Log.EndStopwatch();

Log.StartStopwatch("MEML to File");
meml.ToFile(Folder.data.GetFile("out.meml"));
Log.EndStopwatch();

class TestObject
{
	[Save] public string text = "";
	[Save] public bool enabled = false;
	[Save] public int id = 0;
	[Save] public float number = 0;
	[Save]
	public byte[] bin = { };
	[Save] public object[] items = { };
}

class Test
{
	[Save] public string name = "";
	[Save] public int id = 0;
	[Save] public float number = 0;
}
