using System;

namespace MGE.Graphics
{
	internal class SpriteBatchItem : IComparable<SpriteBatchItem>
	{
		public Texture texture;
		public int priority;

		public Vertex bottomLeft;
		public Vertex bottomRight;
		public Vertex topLeft;
		public Vertex topRight;

		public SpriteBatchItem(Texture texture, Rect destination, RectInt source, Color color, int priority)
		{
			this.texture = texture;

			bottomLeft = new(destination.bottomLeft, texture.GetTextureCoord(source.bottomLeft), color);
			bottomRight = new(destination.bottomRight, texture.GetTextureCoord(source.bottomRight), color);
			topLeft = new(destination.topLeft, texture.GetTextureCoord(source.topLeft), color);
			topRight = new(destination.topRight, texture.GetTextureCoord(source.topRight), color);

			this.priority = priority;
		}

		public int CompareTo(SpriteBatchItem? other) => priority.CompareTo(other!.priority);
	}
}
