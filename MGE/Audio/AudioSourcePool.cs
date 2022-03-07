using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace MGE;

internal class AudioSourcePool : IDisposable
{
	private readonly ConcurrentStack<AudioSource> availableSources;
	private readonly HashSet<AudioSource> playingSources;

	public int Count => availableSources.Count + playingSources.Count;

	public AudioSourcePool()
	{
		availableSources = new ConcurrentStack<AudioSource>();
		playingSources = new HashSet<AudioSource>();
	}

	public AudioSource Reserve(Audio audio, Stream stream)
	{
		if (!availableSources.TryPop(out var source))
		{
			source = new AudioSource(audio, stream)
			{
				IsPooled = true
			};
		}

		playingSources.Add(source);
		return source;
	}

	public void Free(AudioSource source)
	{
		playingSources.Remove(source);
		// if (!playingSources.Remove(source))
		// {
		// Log.Error("Audio source is not pooled");
		// }
	}

	public void Update()
	{
		foreach (var source in playingSources)
		{
			if (source.GetState() == AudioState.Stopped)
			{
				playingSources.Remove(source);
				availableSources.Push(source);
				// if (playingSources.TryRemove(source))
				// {
				// 	availableSources.Push(source);
				// }
				// else
				// {
				// 	Log.Info("Failed to remove source; possible race condition");
				// }
			}
		}
	}

	public void Dispose()
	{
		foreach (var source in playingSources)
		{
			source.Dispose();
		}

		foreach (var source in availableSources)
		{
			source.Dispose();
		}
	}
}
