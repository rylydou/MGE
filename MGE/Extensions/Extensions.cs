using System.Collections.Generic;

namespace MGE
{
	static class Extensions
	{
		public static byte[] ColorToByteRGBA(this Color[] colors)
		{
			var pixles = new List<byte>(colors.Length * 4);

			foreach (var color in colors)
			{
				pixles.Add(color.intR);
				pixles.Add(color.intG);
				pixles.Add(color.intB);
				pixles.Add(color.intA);
			}

			return pixles.ToArray();
		}

		public static byte[] ColorToByteRGBA(this ICollection<Color> colors)
		{
			var pixles = new List<byte>(colors.Count * 4);

			foreach (var color in colors)
			{
				pixles.Add(color.intR);
				pixles.Add(color.intG);
				pixles.Add(color.intB);
				pixles.Add(color.intA);
			}

			return pixles.ToArray();
		}

		public static byte[] ColorToByteARGB(this Color[] colors)
		{
			var pixles = new List<byte>(colors.Length * 4);

			foreach (var color in colors)
			{
				pixles.Add(color.intA);
				pixles.Add(color.intR);
				pixles.Add(color.intG);
				pixles.Add(color.intB);
			}

			return pixles.ToArray();
		}

		public static byte[] ColorToByteARGB(this ICollection<Color> colors)
		{
			var pixles = new List<byte>(colors.Count * 4);

			foreach (var color in colors)
			{
				pixles.Add(color.intA);
				pixles.Add(color.intR);
				pixles.Add(color.intG);
				pixles.Add(color.intB);
			}

			return pixles.ToArray();
		}

		public static bool Toggle(this ref bool b) => b = !b;

		// public static string ToString(this float f) => f.ToString("F3");
	}
}