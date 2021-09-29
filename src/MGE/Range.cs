namespace MGE
{
	[System.Serializable]
	public struct Range
	{
		public float min;
		public float max;

		public float average { get => min + (max - min) / 2; }
		public float random { get => Random.Float(min, max); }

		public Range(float max)
		{
			this.min = 0;
			this.max = max;
		}

		public Range(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float Clamp(float value) => Math.Clamp(value, min, max);
	}
}