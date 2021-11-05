using System;
using System.Collections.Generic;

namespace MGE
{
	public static class Assets
	{
		public static List<Folder> enabledAssetPacks = new List<Folder>();

		static Dictionary<Type, AssetLoader> assetTypeToLoader = new Dictionary<Type, AssetLoader>();
		static Dictionary<Type, string> assetTypeToExtention = new Dictionary<Type, string>();
		static Dictionary<string, AssetLoader> assetExtensionToLoader = new Dictionary<string, AssetLoader>();

		static Dictionary<string, object> preloadedAssets = new Dictionary<string, object>();
		static Dictionary<string, string> indexedAssets = new Dictionary<string, string>();

		public static Font font;

		public static void RegisterAssetType(AssetLoader loader, string extension)
		{
			assetTypeToLoader.TryAdd(loader.loaderFor, loader);
			assetTypeToExtention.TryAdd(loader.loaderFor, extension);
			assetExtensionToLoader.TryAdd(extension, loader);
		}

		internal static void ReloadAssets()
		{
			Logger.StartHeader("Reload Assets");

			Logger.LogVar("Enabled Asset Packs", enabledAssetPacks);

			foreach (var asset in preloadedAssets.Values)
				(asset as IDisposable)?.Dispose();

			preloadedAssets.Clear();
			indexedAssets.Clear();

			var foundAssets = new Dictionary<string, string>();

			ScanAssetPack(Folder.assets, foundAssets);

			foreach (var assetPack in enabledAssetPacks)
			{
				ScanAssetPack(assetPack, foundAssets);
			}

			foreach (var asset in foundAssets)
			{
				preloadedAssets.Add(asset.Key, LoadAssetInt(asset.Value));
			}

			font = GetAsset<Font>("Font");
		}

		internal static void UnloadAssets()
		{
			foreach (var asset in preloadedAssets.Values)
			{
				if (asset is IDisposable a)
					a.Dispose();
			}

			preloadedAssets.Clear();
			indexedAssets.Clear();
		}

		static object LoadAssetInt(string path, Type hint = null)
		{
			var filePath = path.Remove(0, path.LastIndexOf('/'));
			var fullExt = filePath.Remove(0, filePath.IndexOf('.'));
			if (!assetExtensionToLoader.TryGetValue(fullExt, out var assetLoader)) return null;

			Logger.Log($"Loading {Folder.assets.GetRelitivePath(path)} with {assetLoader}...");

			return assetLoader.Load(path);
		}

		static T LoadAssetInt<T>(string path) where T : class
		{
			if (!assetTypeToLoader.TryGetValue(typeof(T), out var assetLoader)) return null;

			Logger.Log($"Loading {Folder.assets.GetRelitivePath(path)} with {assetLoader}...");

			return (T)assetLoader.Load(path);
		}

		static void ScanAssetPack(Folder folder, Dictionary<string, string> foundAssets)
		{
			foreach (var file in folder.FolderGetFiles(string.Empty, true))
			{
				if (file.Contains(Engine.config.metaFileExtention)) continue;

				if (file.StartsWith(Engine.config.indexedAssetPrefix))
				{
					if (indexedAssets.ContainsKey(file))
					{
						Logger.Log(" -  " + folder.GetAbsolutePath(file));
						indexedAssets[file] = folder.GetAbsolutePath(file);
					}
					else
					{
						Logger.Log(" +  " + folder.GetAbsolutePath(file));
						indexedAssets.Add(file, folder.GetAbsolutePath(file));
					}
				}
				else
				{
					if (foundAssets.ContainsKey(file))
					{
						Logger.Log("[-] " + folder.GetAbsolutePath(file));
						foundAssets[file] = folder.GetAbsolutePath(file);
					}
					else
					{
						Logger.Log("[+] " + folder.GetAbsolutePath(file));
						foundAssets.Add(file, folder.GetAbsolutePath(file));
					}
				}
			}
		}

		public static T GetAsset<T>(string path) where T : class
		{
			if (!path.Contains('.'))
				path = path + assetTypeToExtention[typeof(T)];

			preloadedAssets.TryGetValue(path, out var asset);

			return asset as T;
		}

		public static T LoadAsset<T>(string path) where T : class
		{
			if (!path.Contains('.'))
				path = path + assetTypeToExtention[typeof(T)];

			indexedAssets.TryGetValue(path, out var assetPath);

			if (string.IsNullOrEmpty(assetPath)) return null;

			return LoadAssetInt<T>(assetPath) as T;
		}
	}
}