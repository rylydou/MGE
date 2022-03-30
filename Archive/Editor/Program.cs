using MGE;
using MGE.Editor;
using MGE.GLFW;
// using MGE.OpenAL;
using MGE.OpenGL;

App.modules.Register<GLFW_System>();
App.modules.Register<GL_Graphics>();
// App.modules.Register<AL_Audio>();

App.modules.Register<Content>();

App.modules.Register<EditorModule>();

App.Start("MGE Editor", 1280, 720);
