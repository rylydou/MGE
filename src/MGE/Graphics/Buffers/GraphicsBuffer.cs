using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics.Buffers
{
	public class GraphicsBuffer<T> : GraphicsObject where T : struct
	{
		/// <summary>
		/// A value indicating whether the buffer has been initialized and thus has access to allocated memory.
		/// </summary>
		public bool initialized { get; private set; }

		/// <summary>
		/// The size in bytes of one element within the buffer.
		/// </summary>
		public int elementSize { get; protected set; }

		/// <summary>
		/// The number of elements for which buffer memory was allocated.
		/// </summary>
		public int elementCount { get; private set; }

		/// <summary>
		/// The index to the element which will be written to on the next usage of SubData().
		/// </summary>
		public int currentElementIndex { get; set; }

		/// <summary>
		/// The number of elements with data explicitly written.
		/// </summary>
		public int activeElementCount { get; set; }

		/// <summary>
		/// Retrieves data back from vram.
		/// Mainly for debugging purposes.
		/// </summary>
		public T[] content
		{
			get
			{
				var items = new T[elementCount];
				GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
				GL.GetBufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(elementSize * elementCount), items);
				return items;
			}
		}

		/// <summary>
		/// Creates a new, uninitialized buffer object using an explicitly given element size in bytes.
		/// </summary>
		public GraphicsBuffer(int elementSize) : base(GL.GenBuffer())
		{
			initialized = false;
			this.elementSize = elementSize;
		}

		/// <summary>
		/// Creates a new, uninitialized buffer object using the element size determined by Marshal.SizeOf().
		/// </summary>
		public GraphicsBuffer() : this(Marshal.SizeOf(typeof(T)))
		{
		}

		protected override void Dispose(bool manual)
		{
			if (!manual) return;
			GL.DeleteBuffer(handle);
		}

		/// <summary>
		/// Allocates buffer memory and uploads given data to it.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		/// <param name="data">The data to be transfered into the buffer.</param>
		/// <param name="usageHint">The usage hint of the buffer object.</param>
		public void Init(BufferTarget bufferTarget, T[] data, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
		{
			Init(bufferTarget, data.Length, data, usageHint);
			activeElementCount = data.Length;
			currentElementIndex = 0;
		}

		/// <summary>
		/// Allocates buffer memory without initializing it.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		/// <param name="elementCount">The number of elements to allocate memory for.</param>
		/// <param name="usageHint">The usage hint of the buffer object.</param>
		public void Init(BufferTarget bufferTarget, int elementCount, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
		{
			Init(bufferTarget, elementCount, null, usageHint);
			activeElementCount = 0;
			currentElementIndex = 0;
		}

		/// <summary>
		/// Allocates buffer memory and initializes it to the given data.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		/// <param name="elementCount">The number of elements to allocate memory for.</param>
		/// <param name="data">The data to upload into the buffer.</param>
		/// <param name="usageHint">The usage hint of the buffer object.</param>
		protected void Init(BufferTarget bufferTarget, int elementCount, T[]? data, BufferUsageHint usageHint)
		{
			initialized = true;
			this.elementCount = elementCount;
			var fullSize = elementCount * elementSize;
			GL.BindBuffer(bufferTarget, handle);
			GL.BufferData(bufferTarget, (IntPtr)fullSize, data, usageHint);
			CheckBufferSize(bufferTarget, fullSize);
		}

		/// <summary>
		/// Overwrites part of the buffer with the given data and automatically indexes forward through the available memory.
		/// Skips back to the beginning automatically once the end was reached.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		/// <param name="data">The data to be transfered into the buffer.</param>
		public void SubData(BufferTarget bufferTarget, T[] data)
		{
			if (data.Length > elementCount) throw new ArgumentException(
					string.Format("GraphicsBuffer not large enough to hold data. GraphicsBuffer size: {0}. Elements to write: {1}.", elementCount, data.Length));
			// check if data does not fit at the end of the buffer
			var rest = elementCount - currentElementIndex;
			if (rest >= data.Length)
			{
				// add the elements of data to the buffer at the current index
				SubData(bufferTarget, data, currentElementIndex);
				// skip forward through the buffer
				currentElementIndex += data.Length;
				// remember the total number of elements with data
				if (activeElementCount < currentElementIndex) activeElementCount = currentElementIndex;
				// skip back if the end was reached
				// in this case it can only be reached exactly because otherwise it would be handled by the else-case
				if (currentElementIndex >= elementCount) currentElementIndex = 0;
			}
			else
			{
				// first fill the end of the buffer
				SubData(bufferTarget, data, currentElementIndex, rest);
				// proceed to add the remaining elements at the beginning
				rest = data.Length - rest;
				SubData(bufferTarget, data, 0, rest);
				currentElementIndex = rest;
				// remember that the full buffer was already written to
				activeElementCount = elementCount;
			}
		}

		/// <summary>
		/// Overwrites part of the buffer with the given data at the given offset.
		/// Writes all data available in data.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		/// <param name="data">The data to be transfered into the buffer.</param>
		/// <param name="offset">The index to the first element of the buffer to be overwritten.</param>
		public void SubData(BufferTarget bufferTarget, T[] data, int offset)
		{
			SubData(bufferTarget, data, offset, data.Length);
		}

		/// <summary>
		/// Overwrites part of the buffer with the given data at the given offset.
		/// Writes <paramref name="count" /> elements of data.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		/// <param name="data">The data to be transfered into the buffer.</param>
		/// <param name="offset">The index to the first element of the buffer to be overwritten.</param>
		/// <param name="count">The number of elements from data to write.</param>
		public void SubData(BufferTarget bufferTarget, T[] data, int offset, int count)
		{
			if (count > elementCount - offset) throw new ArgumentException(
					string.Format("GraphicsBuffer not large enough to hold data. GraphicsBuffer size: {0}. Offset: {1}. Elements to write: {2}.", elementCount, offset, count));
			if (count > data.Length) throw new ArgumentException(
					string.Format("Not enough data to write to buffer. Data length: {0}. Elements to write: {1}.", data.Length, count));
			GL.BindBuffer(bufferTarget, handle);
			GL.BufferSubData(bufferTarget, (IntPtr)(elementSize * offset), (IntPtr)(elementSize * count), data);
		}

		/// <summary>
		/// Clear the buffer to default values.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		public void Clear(BufferTarget bufferTarget)
		{
			SubData(bufferTarget, new T[elementCount], 0, elementCount);
		}

		/// <summary>
		/// "Orphan" the buffer by calling glBufferData() with the exact same size and usage hint,
		/// but with a NULL pointer as the new data. This will let OpenGL allocate a new buffer
		/// under the same handle and continue using it without synchronization, even if the old
		/// buffer may still be in use by commands remaining in the queue.
		/// </summary>
		/// <param name="bufferTarget">The BufferTarget to use when binding the buffer.</param>
		/// <param name="usageHint">The usage hint of the buffer object.</param>
		public void Orphan(BufferTarget bufferTarget, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
		{
			if (!initialized) throw new InvalidOperationException("Can not orphan uninitialized buffer.");
			Init(bufferTarget, elementCount, usageHint);
			// GL 4.3
			//GL.InvalidateBufferData(handle);
		}

		/// <summary>
		/// Copies elements from the source buffer to this buffer.
		/// Copied on server-side only, no synchronization or transfer of data to host required.
		/// </summary>
		/// <param name="source">The source buffer to copy data from.</param>
		/// <param name="readOffset">Element offset into the source buffer.</param>
		/// <param name="writeOffset">Element offset into this buffer</param>
		/// <param name="count">The Number of elements to copy.</param>
		public void CopyFrom(GraphicsBuffer<T> source, int readOffset, int writeOffset, int count)
		{
			GL.BindBuffer(BufferTarget.CopyReadBuffer, source.handle);
			GL.BindBuffer(BufferTarget.CopyWriteBuffer, handle);
			GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer,
					(IntPtr)(elementSize * readOffset), (IntPtr)(elementSize * writeOffset), (IntPtr)(elementSize * count));
		}

		/// <summary>
		/// Copies elements from the source buffer to this buffer until the end of either buffer is reached.
		/// </summary>
		/// <param name="source">The source buffer to copy elements from.</param>
		public void CopyFrom(GraphicsBuffer<T> source)
		{
			CopyFrom(source, 0, 0, Math.Min(elementCount, source.elementCount));
		}

		/// <summary>
		/// Checks if uploaded size matches the expected size.
		/// </summary>
		protected void CheckBufferSize(BufferTarget bufferTarget, int size)
		{
			int uploadedSize;
			GL.GetBufferParameter(bufferTarget, BufferParameterName.BufferSize, out uploadedSize);
			if (uploadedSize != size) throw new ApplicationException(string.Format("Problem uploading data to buffer object. Tried to upload {0} bytes, but uploaded {1}.", size, uploadedSize));
		}
	}
}
