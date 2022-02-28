using System;
using System.Linq;

namespace MGE;

[Flags]
public enum WindowFlags
{
	/// <summary>
	/// No Flags
	/// </summary>
	None = 0,

	/// <summary>
	/// Hides the Window when it is created
	/// </summary>
	Hidden = 1,

	/// <summary>
	/// Gives the Window a Transparent background
	/// </summary>
	Transparent = 2,

	/// <summary>
	/// Whether the Window should automatically scale to the Monitor
	/// Ex. if a 1280x720 window is created, but the Monitor DPI is 2, this will
	/// create a window at 2560x1440
	/// </summary>
	ScaleToMonitor = 4,

	/// <summary>
	/// Whether the Window BackBuffer should use Multi Sampling. The exact value
	/// of multisampling depends on the platform
	/// </summary>
	MultiSampling = 8,

	/// <summary>
	/// Whether the Window should start fullscreen
	/// </summary>
	Fullscreen = 16,

	/// <summary>
	/// Whether the Window should start maximized
	/// </summary>
	Maximized = 32,
}

public class Window : RenderTarget
{
	public abstract class Platform
	{
		protected internal abstract IntPtr pointer { get; }
		protected internal abstract Vector2Int position { get; set; }
		protected internal abstract Vector2Int size { get; set; }
		protected internal abstract Vector2Int renderSize { get; }
		protected internal abstract Vector2 contentScale { get; }
		protected internal abstract bool opened { get; }

		protected internal abstract string title { get; set; }
		protected internal abstract bool bordered { get; set; }
		protected internal abstract bool resizable { get; set; }
		protected internal abstract bool fullscreen { get; set; }
		protected internal abstract bool visible { get; set; }
		protected internal abstract bool vsync { get; set; }

		protected internal abstract bool focused { get; }
		protected internal abstract Vector2 mouse { get; }
		protected internal abstract Vector2 screenMouse { get; }
		protected internal abstract bool mouseOver { get; }

		protected internal abstract Monitor monitor { get; }

		protected internal abstract void Focus();
		protected internal abstract void Present();
		protected internal abstract void Close();

		protected internal Action? onFocus;
		protected internal Action? onResize;
		protected internal Action? onClose;
		protected internal Action? onCloseRequested;

		protected internal abstract void SetMinSize(Vector2Int? minSize);
		protected internal abstract void SetMaxSize(Vector2Int? maxSize);
		protected internal abstract void SetAspectRatio(Vector2Int? aspectRatio);
	}

	/// <summary>
	/// A reference to the internal platform implementation of the Window
	/// </summary>
	public readonly Platform implementation;

	/// <summary>
	/// A pointer to the underlying OS Window
	/// </summary>
	public IntPtr nativePointer => implementation.pointer;

	/// <summary>
	/// Position of the Window, in Screen coordinates. Setting the Position will toggle off Fullscreen.
	/// </summary>
	public Vector2Int position
	{
		get => implementation.position;
		set
		{
			if (implementation.fullscreen)
				implementation.fullscreen = false;

			implementation.position = value;
		}
	}

	/// <summary>
	/// The size of the Window, in Screen coordinates. Setting the Size will toggle off Fullscreen.
	/// </summary>
	public Vector2Int size
	{
		get => implementation.size;
		set
		{
			if (implementation.fullscreen)
				implementation.fullscreen = false;

			implementation.size = value;
		}
	}

	/// <summary>
	/// The X position of the Window, in Screen coordinates. Setting the Position will toggle off Fullscreen.
	/// </summary>
	public int x
	{
		get => position.x;
		set => position = new Vector2Int(value, y);
	}

	/// <summary>
	/// The X position of the Window, in Screen coordinates. Setting the Position will toggle off Fullscreen.
	/// </summary>
	public int y
	{
		get => position.y;
		set => position = new Vector2Int(x, value);
	}

	/// <summary>
	/// The Width of the Window, in Screen coordinates. Setting the Width will toggle off Fullscreen.
	/// </summary>
	public int width
	{
		get => size.x;
		set => size = new Vector2Int(value, size.y);
	}

	/// <summary>
	/// The Height of the Window, in Screen coordinates. Setting the Height will toggle off Fullscreen.
	/// </summary>
	public int height
	{
		get => size.y;
		set => size = new Vector2Int(size.x, value);
	}

	/// <summary>
	/// The Window bounds, in Screen coordinates. Setting the Bounds will toggle off Fullscreen.
	/// </summary>
	public RectInt bounds
	{
		get
		{
			var position = this.position;
			var size = this.size;

			return new RectInt(position.x, position.y, size.x, size.y);
		}
		set
		{
			size = value.size;
			position = value.position;
		}
	}

	/// <summary>
	/// The Render Size of the Window, in Pixels
	/// </summary>
	public Vector2Int renderSize => implementation.renderSize;

	/// <summary>
	/// The Render Width of the Window, in Pixels
	/// </summary>
	public override int renderWidth => implementation.renderSize.x;

	/// <summary>
	/// The Render Height of the Window, in Pixels
	/// </summary>
	public override int renderHeight => implementation.renderSize.y;

	/// <summary>
	/// The drawable bounds of the Window, in Pixels
	/// </summary>
	public RectInt renderBounds => new RectInt(0, 0, implementation.renderSize.x, implementation.renderSize.y);

	/// <summary>
	/// The scale of the Render size compared to the Window size
	/// On Windows and Linux this is always 1.
	/// On MacOS Retina displays this is 2.
	/// </summary>
	public Vector2 renderScale => new Vector2(implementation.renderSize.x / (float)width, implementation.renderSize.y / (float)height);

	/// <summary>
	/// The Content Scale of the Window
	/// On High DPI displays this may be larger than 1
	/// Use this to appropriately scale UI
	/// </summary>
	public Vector2 contentScale => implementation.contentScale;

	/// <summary>
	/// A callback when the Window is redrawn
	/// </summary>
	public Action<Window>? onRender;

	/// <summary>
	/// A callback when the Window is resized by the user
	/// </summary>
	public Action<Window>? onResize;

	/// <summary>
	/// A callback when the Window is focused
	/// </summary>
	public Action<Window>? onFocus;

	/// <summary>
	/// A callback when the Window is about to close
	/// </summary>
	public Action<Window>? onClose;

	/// <summary>
	/// A callback when the Window has been requested to be closed (ex. by pressing the Close menu button).
	/// By default this calls Window.Close()
	/// </summary>
	public Action<Window>? onCloseRequested;

	/// <summary>
	/// Gets or Sets the Title of this Window
	/// </summary>
	public string title
	{
		get => implementation.title;
		set => implementation.title = value;
	}

	/// <summary>
	/// Gets if the Window is currently Open
	/// </summary>
	public bool opened => implementation.opened;

	/// <summary>
	/// Gets or Sets whether the Window has a Border
	/// </summary>
	public bool bordered
	{
		get => implementation.bordered;
		set => implementation.bordered = value;
	}

	/// <summary>
	/// Gets or Sets whether the Window is resizable by the user
	/// </summary>
	public bool resizable
	{
		get => implementation.resizable;
		set => implementation.resizable = value;
	}

	/// <summary>
	/// Gets or Sets whether the Window is in Fullscreen Mode
	/// </summary>
	public bool fullscreen
	{
		get => implementation.fullscreen;
		set => implementation.fullscreen = value;
	}

	/// <summary>
	/// Gets or Sets whether the Window is Visible to the user
	/// </summary>
	public bool visible
	{
		get => implementation.visible;
		set => implementation.visible = value;
	}

	/// <summary>
	/// Gets or Sets whether the Window synchronizes the vertical redraw
	/// </summary>
	public bool vsync
	{
		get => implementation.vsync;
		set => implementation.vsync = value;
	}

	/// <summary>
	/// Whether this is the currently focused Window
	/// </summary>
	public bool focused => implementation.focused;

	/// <summary>
	/// The Mouse position relative to the top-left of the Window, in Screen coordinates
	/// </summary>
	public Vector2 mouse => implementation.mouse;

	/// <summary>
	/// The position of the Mouse in pixels, relative to the top-left of the Window
	/// </summary>
	public Vector2 renderMouse => implementation.mouse * renderScale;

	/// <summary>
	/// The position of the mouse relative to the top-left of the Screen, in Screen coordinates
	/// </summary>
	public Vector2 screenMouse => implementation.screenMouse;

	/// <summary>
	/// Whether the mouse is currently over this Window
	/// </summary>
	public bool mouseOver => implementation.mouseOver;

	/// <summary>
	/// The Monitor the Window is on
	/// </summary>
	public Monitor monitor => implementation.monitor;

	public Window(string title, int width, int height, WindowFlags flags = WindowFlags.ScaleToMonitor) : this(App.system, title, width, height, flags)
	{

	}

	public Window(System system, string title, int width, int height, WindowFlags flags = WindowFlags.ScaleToMonitor)
	{
		system._windows.Add(this);

		// create implementation object
		implementation = system.CreateWindow(title, width, height, flags);
		implementation.onFocus = () => onFocus?.Invoke(this);
		implementation.onResize = () => onResize?.Invoke(this);
		implementation.onClose = () =>
		{
			onClose?.Invoke(this);
			system._windows.Remove(this);
		};
		implementation.onCloseRequested = () => onCloseRequested?.Invoke(this);

		// default close request to... close the window!
		onCloseRequested = (window) => window.Close();
	}

	public void Focus()
	{
		implementation.Focus();
	}

	/// <summary>
	/// Renders the Window. Call Present afterwards to display the rendered contents
	/// </summary>
	internal void Render()
	{
		// The Window Target is only allowed to be rendered to during this call
		// it greatly simplifies the various states for the Graphics Module
		lock (this)
		{
			renderable = true;

			App.modules.BeforeRenderWindow(this);
			onRender?.Invoke(this);
			App.modules.AfterRenderWindow(this);

			renderable = false;
		}
	}

	/// <summary>
	/// Presents the drawn contents of the Window
	/// </summary>
	internal void Present()
	{
		implementation.Present();
	}

	/// <summary>
	/// Closes the Window
	/// </summary>
	public void Close()
	{
		implementation.Close();
	}

	public void Center()
	{
		position = (monitor.bounds.size - size) / 2;
	}

	public void SetMinSize(Vector2Int? minSize) => implementation.SetMinSize(minSize);
	public void SetMaxSize(Vector2Int? maxSize) => implementation.SetMaxSize(maxSize);
	public void SetAspectRatio(Vector2Int? aspectRatio) => implementation.SetAspectRatio(aspectRatio);
}
