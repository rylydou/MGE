using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using MGE;
using SDL2;

namespace MGE.SDL2
{
	internal class SDL_Window : Window.Platform
	{
		public readonly IntPtr SDLWindowPtr;
		public readonly uint SDLWindowID;

		internal readonly SDL_System _system;
		internal readonly SDL_GLContext? _glContext;

		bool _isBordered = true;
		bool _isFullscreen = false;
		bool _isResizable = false;
		bool _isVisible = false;
		bool _isVSyncEnabled = true;
		bool _isClosed = false;

		protected override IntPtr pointer
		{
			get
			{
				var info = new SDL.SDL_SysWMinfo();
				SDL.SDL_VERSION(out info.version);
				SDL.SDL_GetWindowWMInfo(SDLWindowPtr, ref info);

				switch (info.subsystem)
				{
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
						return info.info.win.window;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_X11:
						return info.info.x11.window;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_DIRECTFB:
						return info.info.dfb.window;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_COCOA:
						return info.info.cocoa.window;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_UIKIT:
						return info.info.uikit.window;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WAYLAND:
						return info.info.wl.surface;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_MIR:
						return info.info.mir.surface;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINRT:
						return info.info.winrt.window;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_ANDROID:
						return info.info.android.window;
					case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_UNKNOWN:
						break;
				}

				throw new NotImplementedException();
			}
		}

		protected override Vector2Int position
		{
			get
			{
				SDL.SDL_GetWindowPosition(SDLWindowPtr, out int x, out int y);
				return new Vector2Int(x, y);
			}
			set
			{
				SDL.SDL_SetWindowPosition(SDLWindowPtr, value.x, value.y);
			}
		}

		protected override Vector2Int size
		{
			get
			{
				SDL.SDL_GetWindowSize(SDLWindowPtr, out int w, out int h);
				return new Vector2Int(w, h);
			}
			set
			{
				SDL.SDL_SetWindowSize(SDLWindowPtr, value.x, value.y);
			}
		}

		protected override Vector2Int renderSize
		{
			get
			{
				int w, h;

				if (App.graphics is IGraphicsOpenGL)
					SDL.SDL_GL_GetDrawableSize(SDLWindowPtr, out w, out h);
				else if (App.graphics is IGraphicsVulkan)
					SDL.SDL_Vulkan_GetDrawableSize(SDLWindowPtr, out w, out h);
				else
					SDL.SDL_GetWindowSize(SDLWindowPtr, out w, out h);

				return new Vector2Int(w, h);
			}
		}

		protected override Vector2 contentScale
		{
			get
			{
				float hidpiRes = 72f;
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					hidpiRes = 96;

				var index = SDL.SDL_GetWindowDisplayIndex(SDLWindowPtr);
				SDL.SDL_GetDisplayDPI(index, out float ddpi, out _, out _);
				return Vector2.one * (ddpi / hidpiRes);
			}
		}

		protected override bool opened => !_isClosed;

		protected override string title
		{
			get => SDL.SDL_GetWindowTitle(SDLWindowPtr);
			set => SDL.SDL_SetWindowTitle(SDLWindowPtr, value);
		}

		protected override bool bordered
		{
			get => _isBordered;
			set
			{
				if (_isBordered != value)
				{
					_isBordered = value;
					SDL.SDL_SetWindowBordered(SDLWindowPtr, _isBordered ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
				}
			}
		}

		protected override bool resizable
		{
			get => _isResizable;
			set
			{
				if (_isResizable != value)
				{
					_isResizable = value;
					SDL.SDL_SetWindowResizable(SDLWindowPtr, _isResizable ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
				}
			}
		}

		protected override bool fullscreen
		{
			get => _isFullscreen;
			set
			{
				if (_isFullscreen != value)
				{
					_isFullscreen = value;
					if (_isFullscreen)
						SDL.SDL_SetWindowFullscreen(SDLWindowPtr, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);
					else
						SDL.SDL_SetWindowFullscreen(SDLWindowPtr, (uint)0);
				}
			}
		}

		protected override bool visible
		{
			get => _isVisible;
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					if (_isVisible)
						SDL.SDL_ShowWindow(SDLWindowPtr);
					else
						SDL.SDL_HideWindow(SDLWindowPtr);
				}
			}
		}

		protected override bool vsync
		{
			get => _isVSyncEnabled;
			set => _isVSyncEnabled = value;
		}

		protected override bool focused
		{
			get => (SDL.SDL_GetWindowFlags(SDLWindowPtr) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) > 0;
		}

		protected override Vector2 mouse
		{
			get
			{
				SDL.SDL_GetWindowPosition(SDLWindowPtr, out int winX, out int winY);
				SDL.SDL_GetGlobalMouseState(out int x, out int y);
				return new Vector2(x - winX, y - winY);
			}
		}

		protected override Vector2 screenMouse
		{
			get
			{
				SDL.SDL_GetGlobalMouseState(out int x, out int y);
				return new Vector2(x, y);
			}
		}

		protected override bool mouseOver
		{
			get => (SDL.SDL_GetWindowFlags(SDLWindowPtr) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS) > 0;
		}

		protected override Monitor monitor { get => new SDL_Monitor(SDL.SDL_GetWindowDisplayIndex(SDLWindowPtr)); }

		public SDL_Window(SDL_System system, string title, int width, int height, WindowFlags flags)
		{
			this._system = system;

			var sdlWindowFlags =
					SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI |
					SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN |
					SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

			if (flags.HasFlag(WindowFlags.Fullscreen))
			{
				sdlWindowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
				_isFullscreen = true;
			}

			if (flags.HasFlag(WindowFlags.Maximized))
				sdlWindowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;

			if (App.graphics is IGraphicsOpenGL)
			{
				sdlWindowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;

				if (system.windows.Count > 0)
					SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_SHARE_WITH_CURRENT_CONTEXT, 1);
			}

			// create the window
			SDLWindowPtr = SDL.SDL_CreateWindow(title, 0x2FFF0000, 0x2FFF0000, width, height, sdlWindowFlags);
			if (SDLWindowPtr == IntPtr.Zero)
				throw new Exception($"Failed to create a new Window: {SDL.SDL_GetError()}");
			SDLWindowID = SDL.SDL_GetWindowID(SDLWindowPtr);

			if (flags.HasFlag(WindowFlags.Transparent))
				SDL.SDL_SetWindowOpacity(SDLWindowPtr, 0f);

			// scale to monitor for HiDPI displays
			if (flags.HasFlag(WindowFlags.ScaleToMonitor))
			{
				float hidpiRes = 72f;
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					hidpiRes = 96;

				var display = SDL.SDL_GetWindowDisplayIndex(SDLWindowPtr);
				SDL.SDL_GetDisplayDPI(display, out var ddpi, out var hdpi, out var vdpi);

				var dpi = (ddpi / hidpiRes);
				if (dpi != 1)
				{
					SDL.SDL_GetDesktopDisplayMode(display, out var mode);
					SDL.SDL_SetWindowPosition(SDLWindowPtr, (int)(mode.w - width * dpi) / 2, (int)(mode.h - height * dpi) / 2);
					SDL.SDL_SetWindowSize(SDLWindowPtr, (int)(width * dpi), (int)(height * dpi));
				}
			}

			// create the OpenGL context
			if (App.graphics is IGraphicsOpenGL)
			{
				_glContext = new SDL_GLContext(system, SDLWindowPtr);
				system.SetCurrentGLContext(_glContext);
			}

			// show window
			_isVisible = false;
			if (!flags.HasFlag(WindowFlags.Hidden))
			{
				_isVisible = true;
				SDL.SDL_ShowWindow(SDLWindowPtr);
			}
		}

		protected override void Focus()
		{
			if ((SDL.SDL_GetWindowFlags(SDLWindowPtr) & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) > 0)
				SDL.SDL_RestoreWindow(SDLWindowPtr);
			SDL.SDL_RaiseWindow(SDLWindowPtr);
		}

		protected override void Present()
		{
			if (App.graphics is IGraphicsOpenGL)
			{
				_system.SetCurrentGLContext(_glContext);
				SDL.SDL_GL_SetSwapInterval(_isVSyncEnabled ? 1 : 0);
				SDL.SDL_GL_SwapWindow(SDLWindowPtr);
			}
		}

		protected override void Close()
		{
			if (!_isClosed)
			{
				_isClosed = true;

				onClose?.Invoke();
				_glContext?.Dispose();
				SDL.SDL_DestroyWindow(SDLWindowPtr);
			}
		}

		public void Resized()
		{
			onResize?.Invoke();
		}

		public void CloseRequested()
		{
			onCloseRequested?.Invoke();
		}

		public void FocusGained()
		{
			onFocus?.Invoke();
		}

		public void Minimized()
		{
			_isVisible = false;
		}

		public void Restored()
		{
			_isVisible = true;
		}

		protected override void SetMinSize(Vector2Int? minSize)
		{
			if (minSize.HasValue)
			{
				SDL.SDL_SetWindowMinimumSize(SDLWindowPtr, minSize.Value.x, minSize.Value.y);
			}
			else
			{
				SDL.SDL_SetWindowMinimumSize(SDLWindowPtr, -1, -1);
			}
		}

		protected override void SetMaxSize(Vector2Int? maxSize)
		{
			if (maxSize.HasValue)
			{
				SDL.SDL_SetWindowMaximumSize(SDLWindowPtr, maxSize.Value.x, maxSize.Value.y);
			}
			else
			{
				SDL.SDL_SetWindowMaximumSize(SDLWindowPtr, -1, -1);
			}
		}

		// TODO SDL doesn't support setting the window aspect ratio yet.
		// Issue: https://github.com/libsdl-org/SDL/issues/1573
		protected override void SetAspectRatio(Vector2Int? aspectRatio)
		{
#if !MGE_IGNORE_UNSUPPORTED
			throw new NotImplementedException("SDL doesn't support setting the window aspect ratio yet.\nIssue: https://github.com/libsdl-org/SDL/issues/1573");
#endif
		}
	}
}
