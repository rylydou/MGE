namespace MGE;

public static class Time
{
	public static float updateTime { get; internal set; }

	public static float tickTime { get; set; }
	// An int is enough for over a year!
	public static int tickNumber { get; internal set; }

	public static float drawTime { get; internal set; }
}
