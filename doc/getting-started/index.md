# Getting started

Create a new console app using the .NET CLI.

```shell
$ dotnet new console
```

Add the Mangrove Engine package and extra modules to the project.

```shell
$ dotnet add package MGE
$ dotnet add package MGE.GLFW
$ dotnet add package MGE.OpenGL
```

Setup the Engine with the modules you want. In this case we are using the GLFW system module with the OpenGL graphics module.

```cs
//	Import the base engine
using MGE;
//	Import the extra modules
using MGE.GLFW;
using MGE.OpenGL;

//	Register engine modules for the system and graphics.
//		GLFW system module
//			This is used for creating windows and getting user input.
App.modules.Register<GLFW_System>();

//	OpenGL graphics module
//		This is used for drawing graphics to the screen.
App.modules.Register<GL_Graphics>();

//	Start the Application with a 1280x720 Window
App.Start("My Application", 1280, 720);
```
