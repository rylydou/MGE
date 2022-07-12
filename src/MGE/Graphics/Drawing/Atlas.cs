using System;
using System.Collections.Generic;

namespace MGE
{
	/// <summary>
	/// A Texture Atlas
	/// </summary>
	public class Atlas
	{
		/// <summary>
		/// List of all the Texture Pages of the Atlas
		/// Generally speaking it's ideal to have a single Page per atlas, but that's not always possible.
		/// </summary>
		public readonly List<Texture> pages = new List<Texture>();

		/// <summary>
		/// A Dictionary of all the Subtextures in this Atlas.
		/// </summary>
		public readonly Dictionary<string, Subtexture> subtextures = new Dictionary<string, Subtexture>();

		/// <summary>
		/// An empty Atlas
		/// </summary>
		public Atlas() { }

		/// <summary>
		/// An Atlas created from an Image Packer, optionally premultiplying the textures
		/// </summary>
		public Atlas(Packer packer, bool premultiply = false)
		{
			var output = packer.Pack();
			if (output is not null)
			{
				foreach (var page in output.pages)
				{
					if (premultiply)
						page.Premultiply();

					pages.Add(new Texture(page));
				}

				foreach (var entry in output.entries.Values)
				{
					var texture = pages[entry.page];
					var subtexture = new Subtexture(texture, entry.source, entry.frame);

					subtextures.Add(entry.name, subtexture);
				}
			}
		}

		/// <summary>
		/// Gets or Sets a Subtexture by name
		/// </summary>
		public Subtexture? this[string name]
		{
			get
			{
				if (subtextures.TryGetValue(name, out var subtex))
					return subtex;
				return null;
			}
			set
			{
				if (value is not null)
					subtextures[name] = value;
			}
		}
	}
}
