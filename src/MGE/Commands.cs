using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGE
{
	sealed class Commands
	{
		static Dictionary<string, object> notes = new Dictionary<string, object>();

		public static void help()
		{
			Terminal.Write("--- COMMANDS ---");
			foreach (var command in Terminal.commandMethods)
			{
				var sb = new StringBuilder(command.Key.Item1);
				sb.Append(' ');
				foreach (var param in command.Value.GetParameters())
				{
					sb.Append('[');
					sb.Append(param.Name);
					sb.Append(' ');
					sb.Append(param.ParameterType.Name.Replace("Int32", "int").Replace("Single", "num").Replace("String", "str").Replace("Object", "any"));
					sb.Append("] ");
				}
				Terminal.Write(sb.ToString());
			}
		}
		public static void ping() => Terminal.Write("pong!");

		public static void note(string name)
		{
			if (notes.TryGetValue(name, out var note))
				Terminal.Write(name + ": " + note);
			else
				Terminal.Write($"{name} was not found!", Color.red);
		}
		public static void note(string name, object note)
		{
			if (!notes.TryAdd(name, note))
				notes[name] = note;
			Terminal.Write($"{name} set to {note}");
		}

		public static void write(string text) => Terminal.Write(text);
		public static void clear() => Terminal.Clear();

		public static void time() => Terminal.Write($"Time: {Time.time}");
		public static void time_raw() => Terminal.Write($"Raw Time: {Time.unscaledTime}");
		public static void tick() => Terminal.Write($"Tick: {Time.tick}");
		public static void time_scale() => Terminal.Write($"Time scale: {Time.timeScale}");
		public static void time_scale(float time_scale)
		{
			Time.timeScale = time_scale;
			Terminal.Write($"Time scale set to {Time.timeScale}");
		}
		public static void time_scale_reset()
		{
			Time.timeScale = 1;
			Terminal.Write($"Time scale reset to {Time.timeScale}");
		}

		public static void tps() => Terminal.Write($"TPS: {Engine.config.tps} ({Engine.config.tickTime * 1000}ms)");
		public static void tps(int tps)
		{
			Engine.config.tps = tps;
			Terminal.Write($"TPS set to {Engine.config.tps} ({Engine.config.tickTime * 1000}ms)");
		}

		public static void stats()
		{
			Stats.enabled.Toggle();
			Terminal.Write("Toggled stats " + (Stats.enabled ? "on" : "off"));
		}

		public static void debug()
		{
			Hierarchy.DEBUG.Toggle();
			Terminal.Write("Toggled debug drawing " + (Hierarchy.DEBUG ? "on" : "off"));
		}

		public static void fullscreen()
		{
			Terminal.Write("Entering " + (Window.fullscreen ? "Fullscreen..." : "Windowed..."));
			Window.ToggleFullscreen();
		}
		public static void window_size(int width, int height)
		{
			GFX.gfxManager.PreferredBackBufferWidth = width;
			GFX.gfxManager.PreferredBackBufferHeight = height;
			GFX.gfxManager.ApplyChanges();
			Terminal.Write($"Resized Window to {width}x{height}");
		}
		public static void window_pos(int x, int y)
		{
			Window.window.Position = new Vector2Int(x, y);
			Terminal.Write($"Moving Window to ({x}, {y})");
		}

		public static void set_pack(string path, int order)
		{
			var moved = Assets.enabledAssetPacks.Contains(path);

			Assets.enabledAssetPacks.Remove(path);
			Assets.enabledAssetPacks.Insert(order, path);

			var index = 0;
			foreach (var assetPack in Assets.enabledAssetPacks)
			{
				if (index == order)
				{
					Terminal.Write($"{index}: {assetPack} " + (moved ? "<- Moved" : "<- Added"));
				}
				else
					Terminal.Write($"{index}: {assetPack}");
				index++;
			}
		}
		public static void unset_pack(string path)
		{
			if (Assets.enabledAssetPacks.Count < 2)
				Terminal.Write("(empty)");
			else
			{
				var position = Assets.enabledAssetPacks.IndexOf(path);

				var index = 0;
				foreach (var assetPack in Assets.enabledAssetPacks)
				{
					if (index == position)
						Terminal.Write($"{index}: {assetPack} <- Removed");
					else
						Terminal.Write($"{index}: {assetPack}");
					index++;
				}
			}

			Assets.enabledAssetPacks.Remove(path);
		}
		public static void unset_pack(int position)
		{
			if (Assets.enabledAssetPacks.Count < 1)
				Terminal.Write("(empty)");
			else
			{
				var index = 0;
				foreach (var assetPack in Assets.enabledAssetPacks)
				{
					if (index == position)
					{
						Terminal.Write($"{index}: {assetPack} <- Removed");
						position = -1;
					}
					else
					{
						Terminal.Write($"{index}: {assetPack}");
						index++;
					}
				}
			}

			Assets.enabledAssetPacks.RemoveAt(position);
		}
		public static void packs()
		{
			if (Assets.enabledAssetPacks.Count < 1)
				Terminal.Write("(empty)");
			else
			{
				var index = 0;
				foreach (var assetPack in Assets.enabledAssetPacks)
				{
					Terminal.Write($"{index}: {assetPack}");
					index++;
				}
			}
		}
		public static void reload_assets()
		{
			Assets.ReloadAssets();
			Terminal.Write("Reloaded Assets");
		}

		public static void gfx_vsync()
		{
			GFX.vsync = !GFX.vsync;
			Terminal.Write($"Toggled V-Sync {(GFX.vsync ? "on" : "off")}");
		}
		public static void gfx_cap_fps()
		{
			GFX.capFps = !GFX.capFps;
			Terminal.Write($"Toggled Cap FPS {(GFX.vsync ? "on" : "off")}");
		}
		public static void gfx_max_fps(int fps)
		{
			GFX.maxFps = fps;
			Terminal.Write($"Set Max FPS to {GFX.maxFps}");
		}
		public static void apply_gfx()
		{
			GFX.gfxManager.ApplyChanges();
			Terminal.Write("Applied GFX Changes");
		}

		public static void quit() => quit(0);
		public static void quit(int exit_code)
		{
			System.Environment.ExitCode = exit_code;
			Engine.game.Exit();
		}

		public static void scene()
		{
			PrintNode(Hierarchy.root, string.Empty, true);
		}

		private static void PrintNode(Node node, string indent, bool isLast)
		{
			string _cross = (char)195 + "-";
			string _corner = (char)192 + "-";
			string _vertical = (char)179 + " ";
			string _space = "  ";

			var sb = new StringBuilder(indent);

			if (isLast)
			{
				sb.Append(_corner);
				indent += _space;
			}
			else
			{
				sb.Append(_cross);
				indent += _vertical;
			}

			sb.Append(node.name);
			Terminal.Write(sb.ToString());

			var i = 0;
			var childCount = node.childCount;
			foreach (var child in node)
			{
				PrintNode(child, indent, i == (childCount - 1));
				i++;
			}
		}

		public static void gc()
		{
			System.GC.Collect();
			Terminal.Write("Cleaned up garbage");
		}

		public static void fail(string message) => System.Environment.FailFast(message);
	}
}
