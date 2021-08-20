using System;

namespace MGE
{
	public sealed class Stopwatch : System.IDisposable
	{
		public readonly string name;

		public DateTime startTime { get; private set; }
		public DateTime? endTime { get; private set; }

		TimeSpan? _duration;
		public TimeSpan duration
		{
			get
			{
				if (!endTime.HasValue)
					return DateTime.Now - startTime;
				if (!_duration.HasValue)
					_duration = endTime - startTime;
				return _duration.Value;
			}
		}

		public Stopwatch(string name)
		{
			this.name = name;

			Restart();
		}

		public void Restart()
		{
			startTime = DateTime.Now;
			endTime = null;
		}

		public void Stop()
		{
			endTime = DateTime.Now;
		}

		public void LogHeader()
		{
			Logger.Log("Starting...", $"⏱ {name}");
		}

		public void LogDuration()
		{
			Logger.Log(duration.ToString(@"mm\:ss\.ffff"), $"⏱ {name}");
		}

		public void Dispose()
		{
			Stop();
			LogDuration();
		}
	}
}