using System.Collections.Generic;

namespace MGE;

public class SpriteSheet
{
	public abstract class UserData
	{
		public Color color;
		public string? userData;
	}

	public class Frame
	{
		public float duration;
	}

	public class Layer : UserData
	{
		public string name = "";

		public float opacity;
		public bool visible = true;
	}

	public class Slice : UserData
	{
		public string name = "";

		public RectInt rect;
		public RectInt? nineSlice;
		public Vector2Int? pivot;
	}

	public class Tag : UserData
	{
		public string name = "";

		public int start;
		public int end;
	}

	public class Cel : UserData
	{
		public Layer layer;
		public Color[] pixels;

		public Cel? link;

		public Vector2Int position;
		public Vector2Int size;

		public float opacity;

		public Cel(Layer layer, Color[] pixels)
		{
			this.layer = layer;
			this.pixels = pixels;
		}
	}

	public enum AnimationDirection
	{
		Forward,
		Reverse,
		PingPong,
	}

	public Atlas atlas;

	public List<Slice> slices = new();

	public SpriteSheet(Atlas atlas)
	{
		this.atlas = atlas;
	}
}
