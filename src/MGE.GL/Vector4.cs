namespace MGE
{
	public struct Vector4
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public Vector4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public float sqrMagnitude { get => x * x + y * y + z * z + w * w; }
		public float magnitude { get => Math.Sqrt(sqrMagnitude); }
	}
}
