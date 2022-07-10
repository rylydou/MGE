using Game;
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

// Register the custom Main Module to run your code
App.modules.Register<MainLoop>();

// Profiler Module
App.modules.Register<Profiler>();

// Start the Application with a single 1280x720 Window
App.Start("Unnamed Party Game", 1280, 720, WindowFlags.ScaleToMonitor);
