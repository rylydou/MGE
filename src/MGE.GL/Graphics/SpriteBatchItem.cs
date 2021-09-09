namespace MGE.Graphics
{
	internal class SpriteBatchItem
	{
		public Texture? texture;
		public float depth;

		public VertexPositionColorTexture vertexTL;
		public VertexPositionColorTexture vertexTR;
		public VertexPositionColorTexture vertexBL;
		public VertexPositionColorTexture vertexBR;

		public SpriteBatchItem()
		{
			vertexTL = new VertexPositionColorTexture();
			vertexTR = new VertexPositionColorTexture();
			vertexBL = new VertexPositionColorTexture();
			vertexBR = new VertexPositionColorTexture();
		}

		public void Set(float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
		{
			this.depth = depth;

			vertexTL.position.X = x + dx * cos - dy * sin;
			vertexTL.position.Y = y + dx * sin + dy * cos;
			vertexTL.position.Z = depth;
			vertexTL.color = color;
			vertexTL.textureCoordinate.x = texCoordTL.x;
			vertexTL.textureCoordinate.y = texCoordTL.y;

			vertexTR.position.X = x + (dx + w) * cos - dy * sin;
			vertexTR.position.Y = y + (dx + w) * sin + dy * cos;
			vertexTR.position.Z = depth;
			vertexTR.color = color;
			vertexTR.textureCoordinate.x = texCoordBR.x;
			vertexTR.textureCoordinate.y = texCoordTL.y;

			vertexBL.position.X = x + dx * cos - (dy + h) * sin;
			vertexBL.position.Y = y + dx * sin + (dy + h) * cos;
			vertexBL.position.Z = depth;
			vertexBL.color = color;
			vertexBL.textureCoordinate.x = texCoordTL.x;
			vertexBL.textureCoordinate.y = texCoordBR.y;

			vertexBR.position.X = x + (dx + w) * cos - (dy + h) * sin;
			vertexBR.position.Y = y + (dx + w) * sin + (dy + h) * cos;
			vertexBR.position.Z = depth;
			vertexBR.color = color;
			vertexBR.textureCoordinate.x = texCoordBR.y;
			vertexBR.textureCoordinate.y = texCoordBR.y;
		}

		public void Set(float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
		{
			this.depth = depth;

			vertexTL.position.X = x;
			vertexTL.position.Y = y;
			vertexTL.position.Z = depth;
			vertexTL.color = color;
			vertexTL.textureCoordinate.x = texCoordTL.x;
			vertexTL.textureCoordinate.y = texCoordTL.y;

			vertexTR.position.X = x + w;
			vertexTR.position.Y = y;
			vertexTR.position.Z = depth;
			vertexTR.color = color;
			vertexTR.textureCoordinate.x = texCoordBR.x;
			vertexTR.textureCoordinate.y = texCoordTL.y;

			vertexBL.position.X = x;
			vertexBL.position.Y = y + h;
			vertexBL.position.Z = depth;
			vertexBL.color = color;
			vertexBL.textureCoordinate.x = texCoordTL.x;
			vertexBL.textureCoordinate.y = texCoordBR.y;

			vertexBR.position.X = x + w;
			vertexBR.position.Y = y + h;
			vertexBR.position.Z = depth;
			vertexBR.color = color;
			vertexBR.textureCoordinate.x = texCoordBR.x;
			vertexBR.textureCoordinate.y = texCoordBR.y;
		}

		public int CompareTo(SpriteBatchItem other) => depth.CompareTo(other.depth);
	}
}
