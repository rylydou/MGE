using System;

namespace MGE;

public static class Engine
{
	public static readonly string operatingSystemName = OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsMacOS() ? "MacOS" : OperatingSystem.IsLinux() ? "Linux" : "Unknown";

	public static readonly bool isWindows = OperatingSystem.IsWindows();
	public static readonly bool isMacOS = OperatingSystem.IsMacOS();
	public static readonly bool isLinux = OperatingSystem.IsLinux();
}
