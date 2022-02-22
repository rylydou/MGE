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

var obj = serilizer.ValueFromObject(new TestObject());

// var file = Folder.data.GetFile("in.meml");
// if (file.exists)
// {
// 	obj = MemlValue.FromFile(file);
// }

obj.ToFile(Folder.data.GetFile("out.meml"));
Folder.data.GetFile("out.dat").WriteBytes(obj.ToBytes());

class TestObject
{
	[Save] public string text = "Hello world";
	[Save] public int id = 1337;
	public float number = 69.420f;
	[Save]
	public object[] items = new object[]
	{
		"Hello world",
		2048,
		7.07,
		new Thing("Joe"),
	};
}

class Thing
{
	[Save] public string name;

	public Thing(string name)
	{
		this.name = name;
	}
}
