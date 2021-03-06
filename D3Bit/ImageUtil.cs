﻿using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using D3Bit.Extensions;
using System.Diagnostics;

namespace D3Bit
{
	public static class ImageUtil
	{		
		#region [ Text Bounds ]
		public static Rectangle GetTextBounding(Bitmap bitmap, Rectangle outerBound, Func<Color, bool> colorFunc)
		{
			using (var locked = bitmap.Lock())
			{
				return GetTextBounding(locked, outerBound, colorFunc);
			}
		}

		public static Rectangle GetTextBounding(LockBitmap bitmap, Rectangle outerBound, Func<Color, bool> colorFunc)
		{
			int sampleSize = 3;

			int l = outerBound.Left,
				r = outerBound.Right,
				t = outerBound.Top,
				b = outerBound.Bottom;

			var left = GetHorizontalTextBounds(bitmap, t, b, Enumerable.Range(l, outerBound.Width * 3 / 4).Sample(sampleSize), colorFunc);
			if (left.HasValue)
			{
				l = left.Value;
			}

			var right = GetHorizontalTextBounds(bitmap, t, b, Enumerable.Range(outerBound.Right - outerBound.Width * 9 / 10, outerBound.Width * 9 / 10).Reverse().Sample(sampleSize), colorFunc, true);
			if (right.HasValue)
			{
				r = right.Value;
			}

			var top = GetVerticalTextBounds(bitmap, t, b, l, r, colorFunc);
			if (top.HasValue)
			{
				t = top.Value;
			}

			var bottom = GetVerticalTextBounds(bitmap, t, b, l, r, colorFunc, true);
			if (bottom.HasValue)
			{
				b = bottom.Value;
			}

			return Rectangle.FromLTRB(l, t, r, b);
		}

		private static int? GetHorizontalTextBounds(LockBitmap bmp, int top, int bottom, IEnumerable<int> widths, Func<Color, bool> colorFunc, bool rightToLeft = false)
		{
			var w = widths.ToArray();
			var count = widths.Count();
			var range = Enumerable.Range(top, bottom - top);

			for (int i = 0; i < count; i++)
			{
				var r = range.Where(y => colorFunc(bmp.GetPixel(w[i], y)));
				if (r.Count() > 0)
				{
					if (i > 0) // need to search previous "sectors" line by line to get exact results
					{
						var newStart = w[i - 1];
						if (i > 2)
						{
							newStart = w[i - 3]; // ...or even 3 (sometimes a letter 'I' might get skipped by the scanlines)
						}
						else if (i > 1)
						{
							newStart = w[i - 2]; // search 2 previous sectors if possible
						}

						if (rightToLeft)
						{
							widths = Enumerable.Range(w[i], newStart - w[i]).Reverse();
						}
						else
						{
							widths = Enumerable.Range(newStart, w[i] - newStart);
						}

						foreach (var x in widths)
						{
							r = range.Where(y => colorFunc(bmp.GetPixel(x, y)));
							if (r.Count() > 0)
							{
								return x;
							}
						}
					}

					return w[i]; // that's first found vertical scanline
				}
			}

			return null;
		}

		private static int? GetVerticalTextBounds(LockBitmap bmp, int top, int bottom, int left, int right, Func<Color, bool> colorFunc, bool bottomToTop = false)
		{
			var range = Enumerable.Range(left, right - left + 1);
			var vRange = Enumerable.Range(top, bottom - top + 1);

			if (bottomToTop)
			{
				vRange = vRange.Reverse();
			}

			foreach (var y in vRange)
			{
				var r = range.Where(x => colorFunc(bmp.GetPixel(x, y)));
				if (r.Count() > 0)
				{
					return y;
				}
			}

			return null;
		}
		#endregion

		#region [ Drawing helpers ]
		//[Conditional("DEBUG_BITMAPS")]
		public static void DrawBlockBounding(Bitmap bitmap, Rectangle bound, Color color)
		{
			using (Graphics g = Graphics.FromImage(bitmap))
			using (Pen pen = new Pen(color))
			{
				g.DrawRectangle(pen, bound);
			}
		}

		//[Conditional("DEBUG_BITMAPS")]
		public static void DrawHLine(Bitmap bitmap, int y, Color color)
		{
			DrawLine(bitmap, new Point(0, y), new Point(bitmap.Width, y), color);
		}

		//[Conditional("DEBUG_BITMAPS")]
		public static void DrawVLine(Bitmap bitmap, int x, Color color)
		{
			DrawLine(bitmap, new Point(x, 0), new Point(x, bitmap.Height), color);
		}

		//[Conditional("DEBUG_BITMAPS")]
		public static void DrawLine(Bitmap bitmap, Point p1, Point p2, Color color)
		{
			using (Graphics g = Graphics.FromImage(bitmap))
			using (var pen = new Pen(color))
			{
				g.DrawLine(pen, p1, p2);
			}
		} 
		#endregion

		#region [ Image manipulation ]
		public static Bitmap MakeGrayscale(Bitmap original)
		{
			//create a blank bitmap the same size as original
			Bitmap newBitmap = new Bitmap(original.Width, original.Height);

			//get a graphics object from the new image
			using (Graphics g = Graphics.FromImage(newBitmap))
			{
				//create the grayscale ColorMatrix
				ColorMatrix colorMatrix = new ColorMatrix(
				   new float[][]
				  {
					 new float[] {.3f, .3f, .3f, 0, 0},
					 new float[] {.59f, .59f, .59f, 0, 0},
					 new float[] {.11f, .11f, .11f, 0, 0},
					 new float[] {0, 0, 0, 1, 0},
					 new float[] {0, 0, 0, 0, 1}
				  });

				//create some image attributes
				ImageAttributes attributes = new ImageAttributes();

				//set the color matrix attribute
				attributes.SetColorMatrix(colorMatrix);

				//draw the original image on the new image
				//using the grayscale color matrix
				g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
			}
			return newBitmap;
		}		

		public static unsafe void ApplyThreshold(int threshold, Bitmap bmp)
		{			
			var bitmapdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			int PixelSize = 4;

			for (int y = 0; y < bitmapdata.Height; y++)
			{
				byte* destPixels = (byte*)bitmapdata.Scan0 + (y * bitmapdata.Stride);

				for (int x = 0; x < bitmapdata.Width; x++)
				{
					destPixels[x * PixelSize] = destPixels[x * PixelSize] > threshold ? (byte)255 : (byte)0; // B
					destPixels[x * PixelSize + 1] = destPixels[x * PixelSize + 1] > threshold ? (byte)255 : (byte)0; // G
					destPixels[x * PixelSize + 2] = destPixels[x * PixelSize + 2] > threshold ? (byte)255 : (byte)0; // R
					//destPixels[x * PixelSize + 3] = contrast_lookup[destPixels[x * PixelSize + 3]]; //A
				}
			}
			bmp.UnlockBits(bitmapdata);
		}

		public static Bitmap AdjustImage(Bitmap original, float brightness = 1.0f, float contrast = 1.0f, float gamma = 1.0f)
		{
			var adjustedImage = new Bitmap(original.Width, original.Height);
			
			float adjustedBrightness = brightness - 1.0f;
			// create matrix that will brighten and contrast the image
			float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, contrast, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                    new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

			var ia = new ImageAttributes();
			ia.ClearColorMatrix();
			ia.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			ia.SetGamma(gamma, ColorAdjustType.Bitmap);

			using (var g = Graphics.FromImage(adjustedImage))
			{
				g.DrawImage(original, new Rectangle(Point.Empty, adjustedImage.Size), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, ia);
			}

			return adjustedImage;
		}

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			//a holder for the result
			Bitmap result = new Bitmap(width, height);

			//use a graphics object to draw the resized image into the bitmap
			using (Graphics graphics = Graphics.FromImage(result))
			{
				//set the resize quality modes to high quality
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.High;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				//draw the image into the target bitmap
				graphics.DrawImage(image, 0, 0, result.Width, result.Height);
			}

			//return the resulting bitmap
			return result;
		}

		public static Bitmap Sharpen(Bitmap image)
		{
			Bitmap sharpenImage = (Bitmap)image.Clone();

			int filterWidth = 3;
			int filterHeight = 3;
			int width = image.Width;
			int height = image.Height;

			// Create sharpening filter.
			double[,] filter = new double[filterWidth, filterHeight];
			filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
			filter[1, 1] = 9;

			double factor = 1.0;
			double bias = 0.0;

			Color[,] result = new Color[image.Width, image.Height];

			// Lock image bits for read/write.
			BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			// Declare an array to hold the bytes of the bitmap.
			int bytes = pbits.Stride * height;
			byte[] rgbValues = new byte[bytes];

			// Copy the RGB values into the array.
			Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

			int rgb;
			// Fill the color array with the new sharpened color values.
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					double red = 0.0, green = 0.0, blue = 0.0;

					for (int filterX = 0; filterX < filterWidth; filterX++)
					{
						for (int filterY = 0; filterY < filterHeight; filterY++)
						{
							int imageX = (x - filterWidth / 2 + filterX + width) % width;
							int imageY = (y - filterHeight / 2 + filterY + height) % height;

							rgb = imageY * pbits.Stride + 3 * imageX;

							red += rgbValues[rgb + 2] * filter[filterX, filterY];
							green += rgbValues[rgb + 1] * filter[filterX, filterY];
							blue += rgbValues[rgb + 0] * filter[filterX, filterY];
						}
						int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
						int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
						int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

						result[x, y] = Color.FromArgb(r, g, b);
					}
				}
			}

			// Update the image with the sharpened pixels.
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					rgb = y * pbits.Stride + 3 * x;

					rgbValues[rgb + 2] = result[x, y].R;
					rgbValues[rgb + 1] = result[x, y].G;
					rgbValues[rgb + 0] = result[x, y].B;
				}
			}

			// Copy the RGB values back to the bitmap.
			Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
			// Release image bits.
			sharpenImage.UnlockBits(pbits);

			return sharpenImage;
		}
		#endregion

	    /// <summary>
	    /// Counts pixels of given color. Search in X coordinate given and Y range given.
	    /// </summary>
        /// <param name="locked">bitmap to search in</param>
        /// <param name="colorFunc">function that checks if the color should be counted or not</param>
        /// <param name="range">vertical range in which to search for pixels</param>
        /// <param name="x">X coordinate</param>
	    /// <param name="additionalRange">X range to be checked as additional stop condition, used to find upper tooltip boundary</param>
	    /// <returns>number of pixels of given color found</returns> 
	    public static int CountPixelsVertical(LockBitmap locked, Func<Color, bool> colorFunc, IEnumerable<int> range, int x, IEnumerable<int> additionalRange = null)
	    {
	        int count = 0;

	        foreach (var y in range)
	        {
	            var pixel = locked.GetPixel(x, y);
	            if (colorFunc(pixel))
	            {
	                count++;

	                if (additionalRange != null && additionalRange.All(ax => colorFunc(locked.GetPixel(ax, y))))
	                {
	                    break;
	                }
	            }
	            else
	            {
	                break;
	            }
	        }

	        return count;
	    }

	    /// <summary>
	    /// Counts pixels of given color. Search in Y coordinate given and X range given
	    /// </summary>
	    /// <param name="locked">bitmap to search in</param>
	    /// <param name="colorFunc">function that checks if the color should be counted or not</param>
	    /// <param name="xRange">horizontal range in which to search for pixels</param>
	    /// <param name="y">Y coordinate</param>
	    /// <returns>number of pixels of given color found</returns> 
	    public static int CountPixelsHorizontal(LockBitmap locked, Func<Color, bool> colorFunc, IEnumerable<int> xRange, int y)
	    {
	        int count = 0;

	        foreach (var x in xRange)
	        {
	            var pixel = locked.GetPixel(x, y);
	            if (colorFunc(pixel))
	            {
	                count++;
	            }
	            else
	            {
	                break;
	            }
	        }

	        return count;
	    }
	}

	public class Line
	{
		public Point P1 { get; private set; }
		public Point P2 { get; private set; }
		public int XLength { get; private set; }

		public Line(Point p1, Point p2)
		{
			P1 = p1;
			P2 = p2;
			XLength = Math.Abs(p2.X - p1.X);
		}

		public override string ToString()
		{
			return P1 + " - " + P2;
		}

	}

}
