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

var serilizer = new MemlSerializer()
{
	getMembers = (type) => MemlSerializer.DefualtGetMembers(type).Where(m => m.GetCustomAttribute<SaveAttribute>() is not null),
};

var obj = serilizer.ObjectFromMeml<TestObject>(MemlValue.FromFile(Folder.data.GetFile("in.meml")));

var meml = serilizer.MemlFromObject(obj);

meml.ToFile(Folder.data.GetFile("out.meml"));

class TestObject
{
	[Save] public string text = "";
	[Save] public int id = 0;
	[Save] public float number = 0;
	[Save] public object[] items = new object[0];
}

class Test
{
	[Save] public string name = "";
	[Save] public int id = 0;
	[Save] public float number = 0;
}
