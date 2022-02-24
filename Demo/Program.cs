using System;
using System.Linq;
using System.Reflection;
using Demo;
using MGE;
using MGE.GLFW;
using MGE.OpenGL;

// System Module (this is Mandatory)
App.modules.Register<GLFW_System>();

// Graphics Module (not Mandatory but required for drawing anything)
App.modules.Register<GL_Graphics>();

// Register the custom Game Module to run your code
App.modules.Register<Game>();

// Start the Application with a single 1280x720 Window
App.Start("My Application", 1280, 720);

System.Console.WriteLine(Convert.ToBase64String(new byte[] { 0, 1, 2, 3, 4, 5 }));
