namespace MGE
{
	public static class Scene
	{
		public static readonly RootNode root = new();

		public static void Update()
		{
			root.DoUpdate(Time.updateTime);
		}

		public static void Tick()
		{
			root.DoTick(Time.tickTime);
		}

		public static void Draw()
		{
			root.DoDraw();
		}
	}
}
