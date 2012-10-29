using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace D3Bit
{
	public class LockBitmap : IDisposable
	{
		Bitmap source = null;
		BitmapData bitmapData = null;

		int cCount;

		public byte[] Pixels { get; set; }
		public int Depth { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int PixelsRead { get; private set; }

		public LockBitmap(Bitmap source)
		{
			this.source = source;
			LockBits();
		}

		/// <summary>
		/// Lock bitmap data
		/// </summary>
		public void LockBits()
		{
			try
			{
				// Get width and height of bitmap
				Width = source.Width;
				Height = source.Height;
				
				// Create rectangle to lock
				Rectangle rect = new Rectangle(0, 0, Width, Height);

				// get source bitmap pixel format size
				Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);
				cCount = Depth / 8;

				// Check if bpp (Bits Per Pixel) is 8, 24, or 32
				if (Depth != 8 && Depth != 24 && Depth != 32)
				{
					throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
				}

				// Lock bitmap and return bitmap data
				bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);

				// create byte array to copy pixel values				
				Pixels = new byte[bitmapData.Stride * Height];
				
				// Copy data from pointer to array
				Marshal.Copy(bitmapData.Scan0, Pixels, 0, Pixels.Length);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Unlock bitmap data
		/// </summary>
		public void UnlockBits()
		{
			try
			{
				// Copy data from byte array to pointer
				Marshal.Copy(Pixels, 0, bitmapData.Scan0, Pixels.Length);

				// Unlock bitmap data
				source.UnlockBits(bitmapData);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Get the color of the specified pixel
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Color GetPixel(int x, int y)
		{
			// Get start index of the specified pixel
			int i = (y * bitmapData.Stride) + (x * cCount);

			if (i > Pixels.Length - cCount)
				throw new IndexOutOfRangeException();

			PixelsRead++;

			if (Depth == 24) // For 24 bpp get Red, Green and Blue
			{
				return Color.FromArgb(Pixels[i + 2], Pixels[i + 1], Pixels[i]);
			}
			else if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
			{
				byte b = Pixels[i];
				byte g = Pixels[i + 1];
				byte r = Pixels[i + 2];
				byte a = Pixels[i + 3]; // a
				return Color.FromArgb(a, r, g, b);
			}			
			// For 8 bpp get color value (Red, Green and Blue values are the same)
			else if (Depth == 8)
			{
				byte c = Pixels[i];
				return Color.FromArgb(c, c, c);
			}

			return Color.Empty;
		}

		/// <summary>
		/// Set the color of the specified pixel
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="color"></param>
		public void SetPixel(int x, int y, Color color)
		{
			// Get color components count
			int cCount = Depth / 8;

			// Get start index of the specified pixel
			int i = ((y * Width) + x) * cCount;

			if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
			{
				Pixels[i] = color.B;
				Pixels[i + 1] = color.G;
				Pixels[i + 2] = color.R;
				Pixels[i + 3] = color.A;
			}
			if (Depth == 24) // For 24 bpp set Red, Green and Blue
			{
				Pixels[i] = color.B;
				Pixels[i + 1] = color.G;
				Pixels[i + 2] = color.R;
			}
			if (Depth == 8)
			// For 8 bpp set color value (Red, Green and Blue values are the same)
			{
				Pixels[i] = color.B;
			}
		}

		public void Dispose()
		{
			UnlockBits();
		}
	}

	public static class BitmapExtensions
	{
		public static LockBitmap Lock(this Bitmap bmp)
		{
			return new LockBitmap(bmp);
		}
	}
}
