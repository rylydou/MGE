namespace MGE
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var window = new GameWindow())
			{
				window.Run();
			}
		}
	}
}
