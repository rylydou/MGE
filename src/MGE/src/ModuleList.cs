using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace MGE;

/// <summary>
/// A List of Modules
/// </summary>
public class ModuleList : IEnumerable<Module>
{
	readonly List<Type> registered = new List<Type>();
	readonly List<Module?> modules = new List<Module?>();
	readonly Dictionary<Type, Module> modulesByType = new Dictionary<Type, Module>();
	bool immediateInit;
	bool immediateStart;

	/// <summary>
	/// Registers a Module
	/// </summary>
	public void Register<T>() where T : Module
	{
		Register(typeof(T));
	}

	static List<Assembly> s_assembliesDllResolversHandled = new();

	/// <summary>
	/// Registers a Module
	/// </summary>
	public void Register(Type type)
	{
		// Register dll resolvers
		var asm = Assembly.GetAssembly(type) ?? throw new MGException("Cannot register module", "Module type is not in an assembly");
		if (!s_assembliesDllResolversHandled.Contains(asm))
		{
			NativeLibrary.SetDllImportResolver(asm, App.DllImportResolver);
			s_assembliesDllResolversHandled.Add(asm);
		}

		if (immediateInit)
		{
			var module = Instantiate(type);

			if (immediateStart)
				StartupModule(module, true);
		}
		else
		{
			registered.Add(type);
		}
	}

	/// <summary>
	/// Registers a Module
	/// </summary>
	Module Instantiate(Type type)
	{
		if (!(Activator.CreateInstance(type) is Module module))
			throw new MGException("Type must inherit from Module");

		// add Module to lookup
		while (type != typeof(Module) && type != typeof(AppModule))
		{
			if (!modulesByType.ContainsKey(type))
				modulesByType[type] = module;

			if (type.BaseType is null)
				break;

			type = type.BaseType;
		}

		// insert in order
		var insert = 0;
		while (insert < modules.Count && (modules[insert]?.priority ?? int.MinValue) <= module.priority)
			insert++;
		modules.Insert(insert, module);

		// registered
		module.isRegistered = true;
		module.mainThreadId = Environment.CurrentManagedThreadId;
		return module;
	}

	/// <summary>
	/// Removes a Module
	/// Note: Removing core modules (such as System) will make everything break
	/// </summary>
	public void Remove(Module module)
	{
		if (!module.isRegistered)
			throw new MGException("Module is not already registered");

		module.Shutdown();
		module.Disposed();

		var index = modules.IndexOf(module);
		modules[index] = null;

		var type = module.GetType();
		while (type != typeof(Module))
		{
			if (modulesByType[type] == module)
				modulesByType.Remove(type);

			if (type.BaseType is null)
				break;

			type = type.BaseType;
		}

		module.isRegistered = false;
	}

	/// <summary>
	/// Tries to get the First Module of the given type
	/// </summary>
	public bool TryGet<T>(out T? module) where T : Module
	{
		if (modulesByType.TryGetValue(typeof(T), out var m))
		{
			module = (T)m;
			return true;
		}

		module = null;
		return false;
	}

	/// <summary>
	/// Tries to get the First Module of the given type
	/// </summary>
	public bool TryGet(Type type, out Module? module)
	{
		if (modulesByType.TryGetValue(type, out var m))
		{
			module = m;
			return true;
		}

		module = null;
		return false;
	}

	/// <summary>
	/// Gets the First Module of the given type, if it exists, or throws an Exception
	/// </summary>
	public T Get<T>() where T : Module
	{
		if (!modulesByType.TryGetValue(typeof(T), out var module))
			throw new MGException($"App is does not have a {typeof(T).Name} Module registered");

		return (T)module;
	}

	/// <summary>
	/// Gets the First Module of the given type, if it exists, or throws an Exception
	/// </summary>
	public Module Get(Type type)
	{
		if (!modulesByType.TryGetValue(type, out var module))
			throw new MGException($"App is does not have a {type.Name} Module registered");

		return module;
	}

	/// <summary>
	/// Checks if a Module of the given type exists
	/// </summary>
	public bool Has<T>() where T : Module
	{
		return modulesByType.ContainsKey(typeof(T));
	}

	/// <summary>
	/// Checks if a Module of the given type exists
	/// </summary>
	public bool Has(Type type)
	{
		return modulesByType.ContainsKey(type);
	}

	internal void ApplicationStarted()
	{
		// create Application Modules
		for (int i = 0; i < registered.Count; i++)
		{
			if (typeof(AppModule).IsAssignableFrom(registered[i]))
			{
				Instantiate(registered[i]);
				registered.RemoveAt(i);
				i--;
			}
		}

		for (int i = 0; i < modules.Count; i++)
		{
			if (modules[i] is AppModule module)
				module.ApplicationStarted();
		}
	}

	internal void FirstWindowCreated()
	{
		for (int i = 0; i < modules.Count; i++)
		{
			if (modules[i] is AppModule module)
				module.FirstWindowCreated();
		}
	}

	internal void Startup()
	{
		// this method is a little strange because it makes sure all App Modules have
		// had their Startup methods called BEFORE instantiating normal Modules
		// Thus it has to iterate over modules and call Startup twice

		// run startup on on App Modules
		for (int i = 0; i < modules.Count; i++)
			StartupModule(modules[i], false);

		// instantiate remaining modules that are registered
		for (int i = 0; i < registered.Count; i++)
			Instantiate(registered[i]);

		// further modules will be instantiated immediately
		immediateInit = true;

		// call started on all modules
		for (int i = 0; i < modules.Count; i++)
			StartupModule(modules[i], true);

		// further modules will have Startup called immediately
		immediateStart = true;
	}

	static void StartupModule(Module? module, bool callAppMethods)
	{
		if (module is not null && !module.isStarted)
		{
			module.isStarted = true;

			if (module is AppModule appModule && callAppMethods)
			{
				appModule.ApplicationStarted();
				appModule.FirstWindowCreated();
			}

			module.Startup();
		}
	}

	internal void Shutdown()
	{
		for (int i = modules.Count - 1; i >= 0; i--)
			modules[i]?.Shutdown();

		for (int i = modules.Count - 1; i >= 0; i--)
			modules[i]?.Disposed();

		registered.Clear();
		modules.Clear();
		modulesByType.Clear();
	}

	internal void FrameStart()
	{
		// remove null module entries
		int toRemove;
		while ((toRemove = modules.IndexOf(null)) >= 0)
			modules.RemoveAt(toRemove);

		for (int i = 0; i < modules.Count; i++)
			modules[i]?.FrameStart();
	}

	internal void Tick(float delta)
	{
		for (int i = 0; i < modules.Count; i++)
			modules[i]?.Tick(delta);
	}

	internal void Update(float delta)
	{
		for (int i = 0; i < modules.Count; i++)
			modules[i]?.Update(delta);
	}

	internal void FrameEnd()
	{
		for (int i = 0; i < modules.Count; i++)
			modules[i]?.FrameEnd();
	}

	internal void BeforeRender()
	{
		for (int i = 0; i < modules.Count; i++)
			modules[i]?.BeforeRender();
	}

	internal void AfterRender()
	{
		for (int i = 0; i < modules.Count; i++)
			modules[i]?.AfterRender();
	}

	internal void BeforeRenderWindow(Window window)
	{
		for (int i = 0; i < modules.Count; i++)
			modules[i]?.BeforeRenderWindow(window);
	}

	internal void AfterRenderWindow(Window window)
	{
		for (int i = 0; i < modules.Count; i++)
			modules[i]?.AfterRenderWindow(window);
	}

	public IEnumerator<Module> GetEnumerator()
	{
		for (int i = 0; i < modules.Count; i++)
		{
			var module = modules[i];
			if (module is not null)
				yield return module;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int i = 0; i < modules.Count; i++)
		{
			var module = modules[i];
			if (module is not null)
				yield return module;
		}
	}
}
