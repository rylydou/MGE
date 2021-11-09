using System.Runtime.InteropServices;

namespace MGE.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Vertex
{
	public const int SIZE = 8;
	public const int FULL_SIZE = SIZE * sizeof(float);

	public Vector2 position;
	public Vector2 textureCoordinate;
	public Color color;

	public Vertex(Vector2 position, Vector2 textureCoordinate, Color color)
	{
		this.position = position;
		this.textureCoordinate = textureCoordinate;
		this.color = color;
	}
}
