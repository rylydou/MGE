using System;

namespace MGE
{
	public abstract class AssetLoader
	{
		public abstract Type loaderFor { get; }

		public abstract object Load(string path);
	}

	public abstract class AssetLoader<T> : AssetLoader where T : class
	{
		public sealed override Type loaderFor => typeof(T);

		public sealed override object Load(string path)
		{
			return LoadAsset(path);
		}

		protected abstract T LoadAsset(string path);
	}
}