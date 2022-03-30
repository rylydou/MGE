using System;
using System.Collections.Generic;
using System.IO;

namespace MGE
{
	/// <summary>
	/// The Packer takes source image data and packs them into large texture pages that can then be used for Atlases
	/// This is useful for sprite fonts, sprite sheets, etc.
	/// </summary>
	public class Packer
	{
		/// <summary>
		/// A single packed Entry
		/// </summary>
		public class Entry
		{
			/// <summary>
			/// The Name of the Entry
			/// </summary>
			public readonly string name;

			/// <summary>
			/// The corresponding image page of the Entry
			/// </summary>
			public readonly int page;

			/// <summary>
			/// The Source Rectangle
			/// </summary>
			public readonly RectInt source;

			/// <summary>
			/// The Frame Rectangle. This is the size of the image before it was packed
			/// </summary>
			public readonly RectInt frame;

			public Entry(string name, int page, RectInt source, RectInt frame)
			{
				this.name = name;
				this.page = page;
				this.source = source;
				this.frame = frame;
			}
		}

		/// <summary>
		/// Stores the Packed result of the Packer
		/// </summary>
		public class Output
		{
			public readonly List<Bitmap> pages = new List<Bitmap>();
			public readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
		}

		/// <summary>
		/// The Packed Output
		/// This is null if the Packer has not yet been packed
		/// </summary>
		public Output packed { get; set; } = new Output();

		/// <summary>
		/// Whether the Packer has unpacked source data
		/// </summary>
		public bool hasUnpackedData { get; set; }

		/// <summary>
		/// Whether to trim transparency from the source images
		/// </summary>
		public bool trim = true;

		/// <summary>
		/// Maximum Texture Size. If the packed data is too large it will be split into multiple pages
		/// </summary>
		public int maxSize = 8192;

		/// <summary>
		/// Image Padding
		/// </summary>
		public int padding = 1;

		/// <summary>
		/// Power of Two
		/// </summary>
		public bool powerOfTwo = false;

		/// <summary>
		/// This will check each image to see if it's a duplicate of an already packed image.
		/// It will still add the entry, but not the duplicate image data.
		/// </summary>
		public bool combineDuplicates = false;

		/// <summary>
		/// The total number of source images
		/// </summary>
		public int sourceImageCount => _sources.Count;

		class Source
		{
			public string name;
			public RectInt packed;
			public RectInt frame;
			public Color[]? buffer;
			public Source? duplicateOf;
			public bool empty => packed.width <= 0 || packed.height <= 0;

			public Source(string name)
			{
				this.name = name;
			}
		}

		readonly List<Source> _sources = new List<Source>();
		readonly Dictionary<int, Source> _duplicateLookup = new Dictionary<int, Source>();

		public void AddBitmap(string name, Bitmap bitmap)
		{
			if (bitmap is not null)
				AddPixels(name, bitmap.width, bitmap.height, new ReadOnlySpan<Color>(bitmap.pixels));
		}

		public void AddFile(string name, File file)
		{
			using var stream = file.OpenRead();
			AddBitmap(name, new Bitmap(stream, file.extension));
		}

		public void AddPixels(string name, int width, int height, ReadOnlySpan<Color> pixels)
		{
			hasUnpackedData = true;

			var source = new Source(name);
			int top = 0, left = 0, right = width, bottom = height;

			// trim
			if (trim)
			{
				// TOP:
				for (int y = 0; y < height; y++)
					for (int x = 0, s = y * width; x < width; x++, s++)
						if (pixels[s].a > 0)
						{
							top = y;
							goto LEFT;
						}
					LEFT:
				for (int x = 0; x < width; x++)
					for (int y = top, s = x + y * width; y < height; y++, s += width)
						if (pixels[s].a > 0)
						{
							left = x;
							goto RIGHT;
						}
					RIGHT:
				for (int x = width - 1; x >= left; x--)
					for (int y = top, s = x + y * width; y < height; y++, s += width)
						if (pixels[s].a > 0)
						{
							right = x + 1;
							goto BOTTOM;
						}
					BOTTOM:
				for (int y = height - 1; y >= top; y--)
					for (int x = left, s = x + y * width; x < right; x++, s++)
						if (pixels[s].a > 0)
						{
							bottom = y + 1;
							goto END;
						}
					END:;
			}

			// determine sizes
			// there's a chance this image was empty in which case we have no width / height
			if (left <= right && top <= bottom)
			{
				var isDuplicate = false;

				if (combineDuplicates)
				{
					var hash = 0;
					for (int x = left; x < right; x++)
						for (int y = top; y < bottom; y++)
							hash = ((hash << 5) + hash) + (int)pixels[x + y * width].abgr;

					if (_duplicateLookup.TryGetValue(hash, out var duplicate))
					{
						source.duplicateOf = duplicate;
						isDuplicate = true;
					}
					else
					{
						_duplicateLookup.Add(hash, source);
					}
				}

				source.packed = new RectInt(0, 0, right - left, bottom - top);
				source.frame = new RectInt(-left, -top, width, height);

				if (!isDuplicate)
				{
					source.buffer = new Color[source.packed.width * source.packed.height];

					// copy our trimmed pixel data to the main buffer
					for (int i = 0; i < source.packed.height; i++)
					{
						var run = source.packed.width;
						var from = pixels.Slice(left + (top + i) * width, run);
						var to = new Span<Color>(source.buffer, i * run, run);

						from.CopyTo(to);
					}
				}
			}
			else
			{
				source.packed = new RectInt();
				source.frame = new RectInt(0, 0, width, height);
			}

			_sources.Add(source);
		}

		struct PackingNode
		{
			public bool used;
			public RectInt rect;
			public unsafe PackingNode* right;
			public unsafe PackingNode* down;
		};

		public unsafe Output Pack()
		{
			// Already been packed
			if (!hasUnpackedData)
				return packed;

			// Reset
			packed = new Output();
			hasUnpackedData = false;

			// Nothing to pack
			if (_sources.Count <= 0)
				return packed;

			// sort the sources by size
			_sources.Sort((a, b) => b.packed.width * b.packed.height - a.packed.width * a.packed.height);

			// make sure the largest isn't too large
			if (_sources[0].packed.width > maxSize || _sources[0].packed.height > maxSize)
				throw new Exception("Source image is larger than max atlas size");

			// TODO: why do we sometimes need more than source images * 3?
			// for safety I've just made it 4 ... but it should really only be 3?

			int nodeCount = _sources.Count * 4;
			Span<PackingNode> buffer = (nodeCount <= 2000 ?
					stackalloc PackingNode[nodeCount] :
					new PackingNode[nodeCount]);

			var padding = Math.Max(0, this.padding);

			// using pointer operations here was faster
			fixed (PackingNode* nodes = buffer)
			{
				int packed = 0, page = 0;
				while (packed < _sources.Count)
				{
					if (_sources[packed].empty)
					{
						packed++;
						continue;
					}

					var from = packed;
					var nodePtr = nodes;
					var rootPtr = ResetNode(nodePtr++, 0, 0, _sources[from].packed.width + padding, _sources[from].packed.height + padding);

					while (packed < _sources.Count)
					{
						if (_sources[packed].empty || _sources[packed].duplicateOf is not null)
						{
							packed++;
							continue;
						}

						int w = _sources[packed].packed.width + padding;
						int h = _sources[packed].packed.height + padding;
						var node = FindNode(rootPtr, w, h);

						// try to expand
						if (node is null)
						{
							bool canGrowDown = (w <= rootPtr->rect.width) && (rootPtr->rect.height + h < maxSize);
							bool canGrowRight = (h <= rootPtr->rect.height) && (rootPtr->rect.width + w < maxSize);
							bool shouldGrowRight = canGrowRight && (rootPtr->rect.height >= (rootPtr->rect.width + w));
							bool shouldGrowDown = canGrowDown && (rootPtr->rect.width >= (rootPtr->rect.height + h));

							if (canGrowDown || canGrowRight)
							{
								// grow right
								if (shouldGrowRight || (!shouldGrowDown && canGrowRight))
								{
									var next = ResetNode(nodePtr++, 0, 0, rootPtr->rect.width + w, rootPtr->rect.height);
									next->used = true;
									next->down = rootPtr;
									next->right = node = ResetNode(nodePtr++, rootPtr->rect.width, 0, w, rootPtr->rect.height);
									rootPtr = next;
								}
								// grow down
								else
								{
									var next = ResetNode(nodePtr++, 0, 0, rootPtr->rect.width, rootPtr->rect.height + h);
									next->used = true;
									next->down = node = ResetNode(nodePtr++, 0, rootPtr->rect.height, rootPtr->rect.width, h);
									next->right = rootPtr;
									rootPtr = next;
								}
							}
						}

						// doesn't fit in this page
						if (node is null)
							break;

						// add
						node->used = true;
						node->down = ResetNode(nodePtr++, node->rect.x, node->rect.y + h, node->rect.width, node->rect.height - h);
						node->right = ResetNode(nodePtr++, node->rect.x + w, node->rect.y, node->rect.width - w, h);

						_sources[packed].packed.x = node->rect.x;
						_sources[packed].packed.y = node->rect.y;

						packed++;
					}

					// get page size
					int pageWidth, pageHeight;
					if (powerOfTwo)
					{
						pageWidth = 2;
						pageHeight = 2;
						while (pageWidth < rootPtr->rect.width)
							pageWidth *= 2;
						while (pageHeight < rootPtr->rect.height)
							pageHeight *= 2;
					}
					else
					{
						pageWidth = rootPtr->rect.width;
						pageHeight = rootPtr->rect.height;
					}

					// create each page
					{
						var bmp = new Bitmap(pageWidth, pageHeight);
						this.packed.pages.Add(bmp);

						// create each entry for this page and copy its image data
						for (int i = from; i < packed; i++)
						{
							var source = _sources[i];

							// do not pack duplicate entries yet
							if (source.duplicateOf is null)
							{
								this.packed.entries[source.name] = new Entry(source.name, page, source.packed, source.frame);

								if (!source.empty)
									bmp.SetPixels(source.packed, source.buffer);
							}
						}
					}

					page++;
				}

			}

			// make sure duplicates have entries
			if (combineDuplicates)
			{
				foreach (var source in _sources)
				{
					if (source.duplicateOf is not null)
					{
						var entry = packed.entries[source.duplicateOf.name];
						packed.entries[source.name] = new Entry(source.name, entry.page, entry.source, entry.frame);
					}
				}
			}

			return packed;

			static unsafe PackingNode* FindNode(PackingNode* root, int w, int h)
			{
				if (root->used)
				{
					var r = FindNode(root->right, w, h);
					return (r is not null ? r : FindNode(root->down, w, h));
				}
				else if (w <= root->rect.width && h <= root->rect.height)
				{
					return root;
				}

				return null;
			}

			static unsafe PackingNode* ResetNode(PackingNode* node, int x, int y, int w, int h)
			{
				node->used = false;
				node->rect = new RectInt(x, y, w, h);
				node->right = null;
				node->down = null;
				return node;
			}
		}

		/// <summary>
		/// Removes all source data and removes the Packed Output
		/// </summary>
		public void Clear()
		{
			_sources.Clear();
			_duplicateLookup.Clear();
			packed = new Output();
			hasUnpackedData = false;
		}
	}
}
