using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

namespace MGE;

public abstract class System : AppModule
{
	/// <summary>
	/// Underlying System implementation API Name
	/// </summary>
	public string apiName { get; protected set; } = "Unknown";

	/// <summary>
	/// Underlying System implementation API Version
	/// </summary>
	public Version apiVersion { get; protected set; } = new Version(0, 0, 0);

	/// <summary>
	/// Whether the System can support Multiple Windows
	/// </summary>
	public abstract bool supportsMultipleWindows { get; }

	/// <summary>
	/// A list of all opened Windows
	/// </summary>
	public readonly ReadOnlyCollection<Window> windows;

	/// <summary>
	/// A list of active Monitors
	/// </summary>
	public readonly ReadOnlyCollection<Monitor> monitors;

	/// <summary>
	/// System Input
	public abstract Input input { get; }

	/// <summary>
	/// The User Directory & a safe location to store save data or preferences
	/// </summary>
	public virtual string UserDirectory(string applicationName) => DefaultUserDirectory(applicationName);

	/// <summary>
	/// Called when a Window is Created
	/// </summary>
	public Action<Window>? onWindowCreated;

	/// <summary>
	/// Internal list of all Windows owned by the System.
	/// </summary>
	internal readonly List<Window> _windows = new List<Window>();

	/// <summary>
	/// Internal list of all Monitors owned by the System. The Platform implementation should maintain this.
	/// </summary>
	protected readonly List<Monitor> _monitors = new List<Monitor>();

	protected System() : base(100)
	{
		windows = new ReadOnlyCollection<Window>(_windows);
		monitors = new ReadOnlyCollection<Monitor>(_monitors);
	}

	protected internal abstract Window.Platform CreateWindow(string title, int width, int height, WindowFlags flags = WindowFlags.None);

	protected internal override void Startup()
	{
		Log.System($"{apiName} {apiVersion}");
	}

	/// <summary>
	/// Gets the Default UserDirectory
	/// </summary>
	internal static string DefaultUserDirectory(string applicationName)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			var result = Environment.GetEnvironmentVariable("HOME");
			if (!string.IsNullOrEmpty(result))
				return Path.Combine(result, "Library", "Application Support", applicationName);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
		{
			var result = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
			if (!string.IsNullOrEmpty(result))
			{
				return Path.Combine(result, applicationName);
			}
			else
			{
				result = Environment.GetEnvironmentVariable("HOME");
				if (!string.IsNullOrEmpty(result))
					return Path.Combine(result, ".local", "share", applicationName);
			}
		}

		return AppDomain.CurrentDomain.BaseDirectory!;
	}
}
