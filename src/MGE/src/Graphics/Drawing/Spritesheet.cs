using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

	public AutoDictionary<string, Slice> slices = new(s => s.name);

	public SpriteSheet(Atlas atlas)
	{
		this.atlas = atlas;
	}

	// TODO Optimize or remove
	public Subtexture GetSprite(int sliceIndex, int frameIndex = 0)
	{
		var frame = atlas[frameIndex.ToString()];
		Debug.Assert(frame is not null, "Frame does not exist");

		return frame.GetClipSubtexture(slices.Values.ElementAt(sliceIndex).rect);
	}

	public Subtexture GetSprite(string sliceName, int frameIndex = 0)
	{
		var frame = atlas[frameIndex.ToString()];
		Debug.Assert(frame is not null, "Frame does not exist");

		return frame.GetClipSubtexture(slices[sliceName].rect);
	}
}
