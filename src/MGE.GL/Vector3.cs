namespace MGE
{
	public struct Vector3
	{
		public float x;
		public float y;
		public float z;

		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public float sqrMagnitude { get => x * x + y * y + z * z; }
		public float magnitude { get => Math.Sqrt(sqrMagnitude); }
	}
}
