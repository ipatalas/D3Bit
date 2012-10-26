using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace D3Bit.Tests
{
	[TestFixture]
	public class LockBitmapTests
	{
		[Test]
		public void test_if_the_pixels_match_with_the_original()
		{
			var path = @"..\..\..\!Data\Tooltips";

			foreach (var file in Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly))
			{
				var bmp = Bitmap.FromFile(file) as Bitmap;
				var clone = (Bitmap)bmp.Clone();

				using (var locked = bmp.Lock())
				{
					for (int x = 0; x < bmp.Width; x++)
					{
						for (int y = 0; y < bmp.Height; y++)
						{
							Assert.AreEqual(clone.GetPixel(x, y), locked.GetPixel(x, y), string.Format("Pixels assertion failed at [{0}, {1}] for {2}", x, y, Path.GetFileName(file)));
						}
					}
				}
			}
		}
	}
}
