using Demo;
using MGE;
using MGE.GLFW;
using MGE.OpenAL;
using MGE.OpenGL;

// System Module (this is Mandatory)
App.modules.Register<GLFW_System>();

// Graphics Module (not Mandatory but required for drawing anything)
App.modules.Register<GL_Graphics>();

// Audio Module
App.modules.Register<AL_Audio>();

// Content Module
App.modules.Register<Content>();

// Physics Module
App.modules.Register<Physics>();

// Register the custom Game Module to run your code
App.modules.Register<Game>();

// Start the Application with a single 1280x720 Window
App.Start("MGE Game", 1280, 720, WindowFlags.ScaleToMonitor);
