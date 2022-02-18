using Demo;
using MGE;
using MGE.GLFW;
using MGE.OpenGL;
using MGE.SDL2;

// our System Module (this is Mandatory)
App.modules.Register<GLFW_System>();

// our Graphics Module (not Mandatory but required for drawing anything)
App.modules.Register<GL_Graphics>();

App.modules.Register<Game>();

// start the Application with a single 1280x720 Window
App.Start("My Application", 1280, 720);
