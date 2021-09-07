using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace MGE
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColorTexture
	{
		public Vector3 position;
		public Color color;
		public Vector2 textureCoordinate;

		public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
		{
			this.position = position;
			this.color = color;
			this.textureCoordinate = textureCoordinate;
		}
	}
}
