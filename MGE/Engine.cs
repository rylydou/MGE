using System;

namespace MGE;

public static class Engine
{
	public static string operatingSystemName = OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsMacOS() ? "Mac OS X" : OperatingSystem.IsLinux() ? "Linux" : "Unknown";
}
