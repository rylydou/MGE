using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

namespace MGE
{
	/// <summary>
	/// Parses the Contents of an Aseprite file
	///
	/// Aseprite File Spec: https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md
	///
	/// TODO  This is not a true or full implementation, and is missing several features (ex. blendmodes)
	///
	/// </summary>
	public class Aseprite
	{
		public enum Modes
		{
			Indexed = 1,
			Grayscale = 2,
			RGBA = 4
		}

		enum Chunks
		{
			OldPaletteA = 0x0004,
			OldPaletteB = 0x0011,
			Layer = 0x2004,
			Cel = 0x2005,
			CelExtra = 0x2006,
			Mask = 0x2016,
			Path = 0x2017,
			FrameTags = 0x2018,
			Palette = 0x2019,
			UserData = 0x2020,
			Slice = 0x2022
		}

		public Modes mode { get; set; }
		public int width { get; set; }
		public int height { get; set; }
		int _frameCount;

		public readonly List<Layer> layers = new List<Layer>();
		public readonly List<Frame> frames = new List<Frame>();
		public readonly List<Tag> tags = new List<Tag>();
		public readonly List<Slice> slices = new List<Slice>();

		public Aseprite(File file)
		{
			using var stream = file.OpenRead();
			Parse(stream);
		}

		public Aseprite(Stream stream)
		{
			Parse(stream);
		}

		#region Data Structures

		public class Frame
		{
			public Aseprite sprite;
			public int duration;
			public Bitmap bitmap;
			public Color[] pixels => bitmap.pixels;
			public List<Cel> cels;

			public Frame(Aseprite sprite)
			{
				this.sprite = sprite;
				cels = new List<Cel>();
				bitmap = new Bitmap(sprite.width, sprite.height);
			}
		}

		public class Tag
		{
			public enum LoopDirections
			{
				Forward = 0,
				Reverse = 1,
				PingPong = 2
			}

			public string name = "";
			public LoopDirections loopDirection;
			public int from;
			public int to;
			public Color color;
		}

		public interface IUserData
		{
			string userDataText { get; set; }
			Color userDataColor { get; set; }
		}

		public class Slice : IUserData
		{
			public int frame;
			public string name = "";
			public int originX;
			public int originY;
			public int width;
			public int height;
			public Vector2Int? pivot;
			public RectInt? nineSlice;
			public string userDataText { get; set; } = "";
			public Color userDataColor { get; set; }
		}

		public class Cel : IUserData
		{
			public Layer layer;
			public Color[] pixels;
			public Cel? link;

			public int x;
			public int y;
			public int width;
			public int height;
			public float alpha;

			public string userDataText { get; set; } = "";
			public Color userDataColor { get; set; }

			public Cel(Layer layer, Color[] pixels)
			{
				this.layer = layer;
				this.pixels = pixels;
			}
		}

		public class Layer : IUserData
		{
			[Flags]
			public enum Flags
			{
				Visible = 1,
				Editable = 2,
				LockMovement = 4,
				Background = 8,
				PreferLinkedCels = 16,
				Collapsed = 32,
				Reference = 64
			}

			public enum Types
			{
				Normal = 0,
				Group = 1
			}

			public Flags flag;
			public Types type;
			public string name = "";
			public int childLevel;
			public int blendMode;
			public float alpha;
			public bool visible { get { return flag.HasFlag(Flags.Visible); } }

			public string userDataText { get; set; } = "";
			public Color userDataColor { get; set; }
		}

		#endregion

		#region .ase Parser

		void Parse(Stream stream)
		{
			var reader = new BinaryReader(stream);

			// wrote these to match the documentation names so it's easier (for me, anyway) to parse
			byte BYTE() => reader.ReadByte();
			ushort WORD() => reader.ReadUInt16();
			short SHORT() => reader.ReadInt16();
			uint DWORD() => reader.ReadUInt32();
			long LONG() => reader.ReadInt32();
			string STRING() => Encoding.UTF8.GetString(BYTES(WORD()));
			byte[] BYTES(int number) => reader.ReadBytes(number);
			void SEEK(int number) => reader.BaseStream.Position += number;

			// Header
			{
				// file size
				DWORD();

				// Magic number (0xA5E0)
				var magic = WORD();
				if (magic != 0xA5E0)
					throw new Exception("File is not in .ase format");

				// Frames / Width / Height / Color Mode
				_frameCount = WORD();
				width = WORD();
				height = WORD();
				mode = (Modes)(WORD() / 8);

				// Other Info, Ignored
				DWORD();       // Flags
				WORD();        // Speed (deprecated)
				DWORD();       // Set be 0
				DWORD();       // Set be 0
				BYTE();        // Palette entry
				SEEK(3);       // Ignore these bytes
				WORD();        // Number of colors (0 means 256 for old sprites)
				BYTE();        // Pixel width
				BYTE();        // Pixel height
				SEEK(92);      // For Future
			}

			// temporary variables
			var temp = new byte[width * height * (int)mode];
			var palette = new Color[256];
			IUserData? last = null;

			// Frames
			for (int i = 0; i < _frameCount; i++)
			{
				var frame = new Frame(this);
				frames.Add(frame);

				long frameStart, frameEnd;
				int chunkCount;

				// frame header
				{
					frameStart = reader.BaseStream.Position;
					frameEnd = frameStart + DWORD();
					WORD();                  // Magic number (always 0xF1FA)
					chunkCount = WORD();     // Number of "chunks" in this frame
					frame.duration = WORD(); // Frame duration (in milliseconds)
					SEEK(6);                 // For future (set to zero)
				}

				// chunks
				for (int j = 0; j < chunkCount; j++)
				{
					long chunkStart, chunkEnd;
					Chunks chunkType;

					// chunk header
					{
						chunkStart = reader.BaseStream.Position;
						chunkEnd = chunkStart + DWORD();
						chunkType = (Chunks)WORD();
					}

					// LAYER CHUNK
					if (chunkType == Chunks.Layer)
					{
						// create layer
						var layer = new Layer();

						// get layer data
						layer.flag = (Layer.Flags)WORD();
						layer.type = (Layer.Types)WORD();
						layer.childLevel = WORD();
						WORD(); // width (unused)
						WORD(); // height (unused)
						layer.blendMode = WORD();
						layer.alpha = (BYTE() / 255f);
						SEEK(3); // for future
						layer.name = STRING();

						last = layer;
						layers.Add(layer);
					}
					// CEL CHUNK
					else if (chunkType == Chunks.Cel)
					{
						var layer = layers[WORD()];
						var x = SHORT();
						var y = SHORT();
						var alpha = BYTE() / 255f;
						var celType = WORD();
						var width = 0;
						var height = 0;
						Color[]? pixels = null;
						Cel? link = null;

						SEEK(7);

						// RAW or DEFLATE
						if (celType == 0 || celType == 2)
						{
							width = WORD();
							height = WORD();

							var count = width * height * (int)mode;
							if (count > temp.Length)
								temp = new byte[count];

							// RAW
							if (celType == 0)
							{
								reader.Read(temp, 0, width * height * (int)mode);
							}
							// DEFLATE
							else
							{
								SEEK(2);

								using var deflate = new DeflateStream(reader.BaseStream, CompressionMode.Decompress, true);
								deflate.Read(temp, 0, count);
							}

							// get pixel data
							pixels = new Color[width * height];
							BytesToPixels(temp, pixels, mode, palette);

						}
						// REFERENCE
						else if (celType == 1)
						{
							var linkFrame = frames[WORD()];
							var linkCel = linkFrame.cels[frame.cels.Count];

							width = linkCel.width;
							height = linkCel.height;
							pixels = linkCel.pixels;
							link = linkCel;
						}
						else
						{
							throw new NotImplementedException();
						}

						var cel = new Cel(layer, pixels)
						{
							x = x,
							y = y,
							width = width,
							height = height,
							alpha = alpha,
							link = link
						};

						// draw to frame if visible
						if (cel.layer.visible)
							CelToFrame(frame, cel);

						last = cel;
						frame.cels.Add(cel);
					}
					// PALETTE CHUNK
					else if (chunkType == Chunks.Palette)
					{
						var size = DWORD();
						var start = DWORD();
						var end = DWORD();
						SEEK(8); // for future

						for (int p = 0; p < (end - start) + 1; p++)
						{
							var hasName = WORD();
							palette[start + p] = new Color(BYTE(), BYTE(), BYTE(), BYTE()).Premultiply();

							if (Calc.IsBitSet(hasName, 0))
								STRING();
						}
					}
					// USERDATA
					else if (chunkType == Chunks.UserData)
					{
						if (last is not null)
						{
							var flags = (int)DWORD();

							// has text
							if (Calc.IsBitSet(flags, 0))
								last.userDataText = STRING();

							// has color
							if (Calc.IsBitSet(flags, 1))
								last.userDataColor = new Color(BYTE(), BYTE(), BYTE(), BYTE()).Premultiply();
						}
					}
					// TAG
					else if (chunkType == Chunks.FrameTags)
					{
						var count = WORD();
						SEEK(8);

						for (int t = 0; t < count; t++)
						{
							var tag = new Tag();
							tag.from = WORD();
							tag.to = WORD();
							tag.loopDirection = (Tag.LoopDirections)BYTE();
							SEEK(8);
							tag.color = new Color(BYTE(), BYTE(), BYTE(), (byte)255).Premultiply();
							SEEK(1);
							tag.name = STRING();
							tags.Add(tag);
						}
					}
					// SLICE
					else if (chunkType == Chunks.Slice)
					{
						var count = DWORD();
						var flags = (int)DWORD();
						DWORD(); // reserved
						var name = STRING();

						for (int s = 0; s < count; s++)
						{
							var slice = new Slice
							{
								name = name,
								frame = (int)DWORD(),
								originX = (int)LONG(),
								originY = (int)LONG(),
								width = (int)DWORD(),
								height = (int)DWORD()
							};

							// 9 slice (ignored atm)
							if (Calc.IsBitSet(flags, 0))
							{
								slice.nineSlice = new RectInt(
										(int)LONG(),
										(int)LONG(),
										(int)DWORD(),
										(int)DWORD());
							}

							// pivot point
							if (Calc.IsBitSet(flags, 1))
								slice.pivot = new((int)DWORD(), (int)DWORD());

							last = slice;
							slices.Add(slice);
						}
					}

					reader.BaseStream.Position = chunkEnd;
				}

				reader.BaseStream.Position = frameEnd;
			}
		}

		#endregion

		#region Blend Modes

		// More or less copied from Aseprite's source code:
		// https://github.com/aseprite/aseprite/blob/master/src/doc/blend_funcs.cpp

		delegate void Blend(ref Color dest, Color src, byte opacity);

		static readonly Blend[] BlendModes = new Blend[]
	 {
            // 0 - NORMAL
            (ref Color dest, Color src, byte opacity) =>
						{
								if (src.a != 0)
								{
										if (dest.a == 0)
										{
												dest = src;
										}
										else
										{
												var sa = MUL_UN8(src.a, opacity);
												var ra = dest.a + sa - MUL_UN8(dest.a, sa);

												dest.r = (byte)(dest.r + (src.r - dest.r) * sa / ra);
												dest.g = (byte)(dest.g + (src.g - dest.g) * sa / ra);
												dest.b = (byte)(dest.b + (src.b - dest.b) * sa / ra);
												dest.a = (byte)ra;
										}

								}
						}
	 };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int MUL_UN8(int a, int b)
		{
			var t = (a * b) + 0x80;
			return (((t >> 8) + t) >> 8);
		}

		#endregion

		#region Utils

		/// <summary>
		/// Converts an array of Bytes to an array of Colors, using the specific Aseprite Mode & Palette
		/// </summary>
		void BytesToPixels(byte[] bytes, Color[] pixels, Modes mode, Color[] palette)
		{
			int len = pixels.Length;
			if (mode == Modes.RGBA)
			{
				for (int p = 0, b = 0; p < len; p++, b += 4)
				{
					pixels[p].r = (byte)(bytes[b + 0] * bytes[b + 3] / 255);
					pixels[p].g = (byte)(bytes[b + 1] * bytes[b + 3] / 255);
					pixels[p].b = (byte)(bytes[b + 2] * bytes[b + 3] / 255);
					pixels[p].a = bytes[b + 3];
				}
			}
			else if (mode == Modes.Grayscale)
			{
				for (int p = 0, b = 0; p < len; p++, b += 2)
				{
					pixels[p].r = pixels[p].g = pixels[p].b = (byte)(bytes[b + 0] * bytes[b + 1] / 255);
					pixels[p].a = bytes[b + 1];
				}
			}
			else if (mode == Modes.Indexed)
			{
				for (int p = 0; p < len; p++)
					pixels[p] = palette[bytes[p]];
			}
		}

		/// <summary>
		/// Applies a Cel's pixels to the Frame, using its Layer's BlendMode & Alpha
		/// </summary>
		void CelToFrame(Frame frame, Cel cel)
		{
			var opacity = (byte)((cel.alpha * cel.layer.alpha) * 255);
			var pxLen = frame.bitmap.pixels.Length;

			var blend = BlendModes[0];
			if (cel.layer.blendMode < BlendModes.Length)
				blend = BlendModes[cel.layer.blendMode];

			for (int sx = Math.Max(0, -cel.x), right = Math.Min(cel.width, frame.sprite.width - cel.x); sx < right; sx++)
			{
				int dx = cel.x + sx;
				int dy = cel.y * frame.sprite.width;

				for (int sy = Math.Max(0, -cel.y), bottom = Math.Min(cel.height, frame.sprite.height - cel.y); sy < bottom; sy++, dy += frame.sprite.width)
				{
					if (dx + dy >= 0 && dx + dy < pxLen)
						blend(ref frame.bitmap.pixels[dx + dy], cel.pixels[sx + sy * cel.width], opacity);
				}
			}
		}

		/// <summary>
		/// Adds all Aseprite Frames to the Atlas, using the naming format (ex. "mySprite/{0}" where {0} becomes the frame index)
		/// </summary>
		public void Pack(string namingFormat, Packer packer)
		{
			if (!namingFormat.Contains("{0}"))
				throw new Exception("naming format must contain {0} for frame index");

			int frameIndex = 0;
			foreach (var frame in frames)
			{
				var name = string.Format(namingFormat, frameIndex);
				packer.AddPixels(name, width, height, frame.bitmap.pixels);

				frameIndex++;
			}
		}

		#endregion
	}
}
