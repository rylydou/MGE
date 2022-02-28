using System;
using System.Runtime.InteropServices;

namespace MGE.GLFW
{
	internal class GLFW_Window : Window.Platform
	{
		readonly GLFW_System _system;
		internal readonly IntPtr _pointer;

		readonly GLFW.WindowCloseFunc _windowCloseCallbackRef;
		readonly GLFW.WindowSizeFunc _windowSizeCallbackRef;
		readonly GLFW.WindowFocusFunc _windowFocusCallbackRef;
		readonly GLFW.CursorEnterFunc _windowCursorEnterCallbackRef;

		string _title;
		bool _isVisible;
		bool _isFocused;
		bool _isMouseOver;
		bool _isDisposed;
		bool _isFullscreen;
		RectInt _storedBounds;

		protected override Vector2Int position
		{
			get
			{
				GLFW.GetWindowPos(_pointer, out int x, out int y);
				return new Vector2Int(x, y);
			}

			set
			{
				GLFW.SetWindowPos(_pointer, value.x, value.y);
			}
		}

		protected override Vector2Int size
		{
			get
			{
				GLFW.GetWindowSize(_pointer, out int w, out int h);
				return new Vector2Int(w, h);
			}

			set
			{
				GLFW.SetWindowSize(_pointer, value.x, value.y);
			}
		}

		protected override Vector2Int renderSize
		{
			get
			{
				GLFW.GetFramebufferSize(_pointer, out int width, out int height);
				return new Vector2Int(width, height);
			}
		}

		protected override Vector2 mouse
		{
			get
			{
				GLFW.GetCursorPos(_pointer, out var xpos, out var ypos);
				return new Vector2((float)xpos, (float)ypos);
			}
		}

		protected override Vector2 screenMouse
		{
			get
			{
				GLFW.GetCursorPos(_pointer, out var curX, out var curY);
				GLFW.GetWindowPos(_pointer, out var winX, out var winY);
				return new Vector2((float)curX + winX, (float)curY + winY);
			}
		}

		protected override Vector2 contentScale
		{
			get
			{
				GLFW.GetWindowContentScale(_pointer, out float x, out float y);
				return new Vector2(x, y);
			}
		}

		protected override bool opened => !_isDisposed;

		protected override bool focused => _isFocused;

		protected override bool mouseOver => _isMouseOver;

		protected override string title
		{
			get => _title;
			set => GLFW.SetWindowTitle(_pointer, _title = value);
		}

		protected override bool vsync { get; set; } = true;

		protected override bool bordered
		{
			get => GLFW.GetWindowAttrib(_pointer, GLFW.WindowAttributes.Decorated);
			set => GLFW.SetWindowAttrib(_pointer, GLFW.WindowAttributes.Decorated, value);
		}

		protected override bool resizable
		{
			get => GLFW.GetWindowAttrib(_pointer, GLFW.WindowAttributes.Resizable);
			set => GLFW.SetWindowAttrib(_pointer, GLFW.WindowAttributes.Resizable, value);
		}

		protected override bool fullscreen
		{
			get
			{
				return _isFullscreen;
			}
			set
			{
				if (_isFullscreen != value)
				{
					_isFullscreen = value;

					if (_isFullscreen)
					{
						// force window to be shown before calling fullscreen
						// this way it also becomes focused
						GLFW.ShowWindow(_pointer);

						var bounds = new RectInt(position, size);
						var monitor = GLFW.GetPrimaryMonitor();

						// find the monitor we overlap with most
						unsafe
						{
							var monitors = GLFW.GetMonitors(out int count);

							if (count > 1)
							{
								var currMonBounds = new RectInt();
								GLFW.GetMonitorWorkarea(monitor, out currMonBounds.x, out currMonBounds.y, out currMonBounds.width, out currMonBounds.height);

								for (int i = 0; i < count; i++)
								{
									var nextMonBounds = new RectInt();
									GLFW.GetMonitorWorkarea(monitors[i], out nextMonBounds.x, out nextMonBounds.y, out nextMonBounds.width, out nextMonBounds.height);

									if (RectInt.Intersect(bounds, nextMonBounds).area > RectInt.Intersect(bounds, currMonBounds).area)
									{
										monitor = monitors[i];
										currMonBounds = nextMonBounds;
									}
								}
							}
						}

						if (monitor != IntPtr.Zero)
						{
							_storedBounds = new RectInt(position, size);

							GLFW.GetMonitorWorkarea(monitor, out var x, out var y, out var w, out var h);
							GLFW.SetWindowMonitor(_pointer, monitor, 0, 0, w, h, (int)GLFW_Enum.GLFW_DONT_CARE);
						}
						else
							_isFullscreen = false;
					}
					else
					{
						GLFW.SetWindowMonitor(_pointer, IntPtr.Zero, _storedBounds.x, _storedBounds.y, _storedBounds.width, _storedBounds.height, (int)GLFW_Enum.GLFW_DONT_CARE);
					}
				}
			}
		}

		protected override bool visible
		{
			get => _isVisible;
			set
			{
				_isVisible = value;
				if (_isVisible)
					GLFW.ShowWindow(_pointer);
				else
					GLFW.HideWindow(_pointer);
			}
		}

		protected override IntPtr pointer
		{
			get
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return GLFW.GetWin32Window(_pointer);
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					return GLFW.GetCocoaWindow(_pointer);
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return GLFW.GetX11Window(_pointer); // TODO Support wayland
				return IntPtr.Zero;
			}
		}

		protected override Monitor monitor
		{
			get
			{
				// // GLFW only returns the monitor if the window is fullscreen
				// if (_isFullscreen)
				// {
				// 	var fullscreenMonitor = GLFW.GetWindowMonitor(_pointer);
				// 	if (fullscreenMonitor != IntPtr.Zero)
				// 	{
				// 		return new GLFW_Monitor(fullscreenMonitor);
				// 	}
				// }

				var windowArea = new RectInt(position, size);

				var bestMonitor = _system.primaryMonitor;
				var bestArea = -1;
				foreach (var monitor in _system.monitors)
				{
					var area = RectInt.Intersect(monitor.bounds, windowArea).area;
					if (area > bestArea)
					{
						bestMonitor = monitor;
						bestArea = area;
					}
				}

				return bestMonitor;
			}
		}

		internal GLFW_Window(GLFW_System system, IntPtr pointer, string title, bool visible)
		{
			this._system = system;
			this._pointer = pointer;
			this._title = title;
			this._isVisible = visible;

			// opengl swap interval
			if (App.graphics is IGraphicsOpenGL)
			{
				system.SetCurrentGLContext(pointer);
				GLFW.SwapInterval(vsync ? 1 : 0);
			}

			GLFW.SetWindowCloseCallback(this._pointer, _windowCloseCallbackRef = OnWindowClose);
			GLFW.SetWindowSizeCallback(this._pointer, _windowSizeCallbackRef = OnWindowResize);
			GLFW.SetWindowFocusCallback(this._pointer, _windowFocusCallbackRef = OnWindowFocus);
			GLFW.SetCursorEnterCallback(this._pointer, _windowCursorEnterCallbackRef = OnCursorEnter);
		}

		void OnWindowClose(IntPtr window)
		{
			GLFW.SetWindowShouldClose(window, true);
			onCloseRequested?.Invoke();
		}

		void OnWindowResize(IntPtr window, int width, int height)
		{
			onResize?.Invoke();
		}

		void OnWindowFocus(IntPtr window, int focused)
		{
			_isFocused = (focused != 0);
			if (_isFocused)
				onFocus?.Invoke();
		}

		void OnCursorEnter(IntPtr window, int entered)
		{
			_isMouseOver = (entered != 0);
		}

		protected override void Focus()
		{
			GLFW.FocusWindow(_pointer);
		}

		protected override void Present()
		{
			// update our Swap Interval while we're here
			if (App.graphics is IGraphicsOpenGL)
			{
				var context = _system.GetCurrentGLContext();
				if (context is null || (context is GLFW_GLContext ctx && ctx.window != _pointer))
					_system.SetCurrentGLContext(_pointer);

				GLFW.SwapInterval(vsync ? 1 : 0);
			}

			// Swap
			GLFW.SwapBuffers(_pointer);
		}

		protected override void Close()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				GLFW.SetWindowShouldClose(_pointer, true);
			}
		}

		internal void InvokeCloseWindowCallback()
		{
			onClose?.Invoke();
		}

		Vector2Int minSize;
		protected override void SetMinSize(Vector2Int? minSize)
		{
			if (minSize.HasValue)
			{
				this.minSize = minSize.Value;
			}
			else
			{
				this.minSize = new((int)GLFW_Enum.GLFW_DONT_CARE);
			}

			UpdateWindowLimits();
		}

		Vector2Int maxSize;
		protected override void SetMaxSize(Vector2Int? maxSize)
		{
			if (maxSize.HasValue)
			{
				this.maxSize = maxSize.Value;
			}
			else
			{
				this.maxSize = new((int)GLFW_Enum.GLFW_DONT_CARE);
			}

			UpdateWindowLimits();
		}

		void UpdateWindowLimits()
		{
			GLFW.SetWindowSizeLimits(_pointer, minSize.x, minSize.y, maxSize.x, maxSize.y);
		}

		protected override void SetAspectRatio(Vector2Int? aspectRatio)
		{
			GLFW.SetWindowAspectRatio(_pointer, size.x, size.y);
		}
	}
}
