using System;
using System.Text;

namespace MGE
{
	public static class Util
	{
		static string[] filesSizeUnits = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

		public static string BytesToString(long byteCount)
		{
			if (byteCount == 0) return "0B";

			var bytes = Math.Abs(byteCount);
			var place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
			var num = bytes / Math.Pow(1024, place);

			return $"{(Math.Sign(byteCount) * num).ToString("F2")}{filesSizeUnits[place]}";
		}

		public static string GetFamilyTree<T>() => GetFamilyTree(typeof(T));
		public static string GetFamilyTree(Type type)
		{
			var builder = new StringBuilder();

			while (type != null)
			{
				builder.Insert(0, '.');
				builder.Insert(0, type.Name);

				type = type.BaseType;
			}

			return builder.ToString(7, builder.Length - 8);
		}
	}
}