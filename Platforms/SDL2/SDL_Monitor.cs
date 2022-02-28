using System;
using System.Numerics;
using MGE;
using SDL2;

namespace MGE.SDL2
{
	internal class SDL_Monitor : Monitor
	{
		public readonly int index;

		string _name;
		RectInt _bounds;
		Vector2 _contentScale;

		public override string name => _name;
		public override bool isPrimary => index == 0;
		public override RectInt bounds => _bounds;
		public override Vector2 contentScale => _contentScale;

		public SDL_Monitor(int index)
		{
			this.index = index;

			_name = SDL.SDL_GetDisplayName(index);
			FetchProperties();
		}

		public void FetchProperties()
		{
			SDL.SDL_GetDisplayBounds(index, out var rect);
			_bounds = new RectInt(rect.x, rect.y, rect.w, rect.h);

			var hidpiRes = 72f;
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				hidpiRes = 96;
			SDL.SDL_GetDisplayDPI(index, out float ddpi, out _, out _);
			_contentScale = Vector2.one * (ddpi / hidpiRes);
		}
	}
}
