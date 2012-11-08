using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace D3Bit
{
    public static class Screenshot
    {
		private const string UpperCornerName = "ucorner.png";
		private const string BottomCornerName = "bcorner.png";

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

		[DllImport("user32.dll")]
		static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;

            public RECT(RECT Rectangle)
                : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
            {
            }
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                _Left = Left;
                _Top = Top;
                _Right = Right;
                _Bottom = Bottom;
            }

            public int X
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Y
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Left
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Top
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Right
            {
                get { return _Right; }
                set { _Right = value; }
            }
            public int Bottom
            {
                get { return _Bottom; }
                set { _Bottom = value; }
            }
            public int Height
            {
                get { return _Bottom - _Top; }
                set { _Bottom = value + _Top; }
            }
            public int Width
            {
                get { return _Right - _Left; }
                set { _Right = value + _Left; }
            }
            public Point Location
            {
                get { return new Point(Left, Top); }
                set
                {
                    _Left = value.X;
                    _Top = value.Y;
                }
            }
            public Size Size
            {
                get { return new Size(Width, Height); }
                set
                {
                    _Right = value.Width + _Left;
                    _Bottom = value.Height + _Top;
                }
            }

            public static implicit operator Rectangle(RECT Rectangle)
            {
                return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
            }
            public static implicit operator RECT(Rectangle Rectangle)
            {
                return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }
            public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }
            public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }

            public override string ToString()
            {
                return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public bool Equals(RECT Rectangle)
            {
                return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
            }

            public override bool Equals(object Object)
            {
                if (Object is RECT)
                {
                    return Equals((RECT)Object);
                }
                else if (Object is Rectangle)
                {
                    return Equals(new RECT((Rectangle)Object));
                }

                return false;
            }
        }
				
        public static Bitmap GetSnapShot(Process d3Proc)
        {
            if (d3Proc != null)
            {
                RECT rc;
                // need to use Client area of the window, otherwise it won't work in Windowed mode
				GetClientRect(d3Proc.MainWindowHandle, out rc);

				Point location = rc.Location;
				ClientToScreen(d3Proc.MainWindowHandle, ref location);

                Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format24bppRgb);

				using (var g = Graphics.FromImage(bmp))
				{
					g.CopyFromScreen(location.X, location.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
				}

                return bmp;
            }
            return null;
        }

        public static Bitmap GetToolTip(Bitmap bitmap)
        {
            var lines = ImageUtil.FindHorizontalLines(bitmap, 260, 650, new int[] { 0, 10, 0, 10, 0, 10 });
            lines = lines.OrderBy(l => l.P1.X).ToList();

            var groups =
                lines.GroupBy(l => l.P1.X).Where(
                    l =>
                    l.Last().P1.Y - l.First().P1.Y > 200 &&
                    l.Count() > 4 &&
                    l.Count() == l.Where(i => Math.Abs(i.XLength - l.First().XLength) < i.XLength*0.1).Count()).OrderByDescending(
                        l => l.First().XLength).ThenByDescending(l => l.Count());
            int x = groups.Count();
            if (groups.Count() > 0)
            {
                lines = groups.ElementAt(0).ToList();
                //Count line clusters
                int clusterCount = 0;
                int lastY = lines.ElementAt(0).P1.Y;
                foreach (var line in lines)
                {
                    if (line.P1.Y - lastY > 5)
                        clusterCount++;
                    lastY = line.P1.Y;
                }

                var min = new Point(bitmap.Width, bitmap.Height);
                var max = new Point(0, 0);
                foreach (var line in groups.ElementAt(0))
                {
                    if (line.P1.X <= min.X && line.P1.Y <= min.Y)
                        min = line.P1;
                    else if (line.P2.X >= max.X && line.P2.Y >= max.Y)
                        max = line.P2;
                }
                Bound bound = new Bound(min, max);
                if (clusterCount==2)
                    bound = new Bound(new Point(min.X, min.Y - (int)Math.Round((42/410.0)*(max.X-min.X))), max);
                return bitmap.Clone(bound.ToRectangle(), bitmap.PixelFormat);
            }
            return null;
        }

		#region [ New tooltip search method ]
		public static Bitmap GetTooltip_LinesV2(Bitmap source, bool limitSearchArea = true)
		{
			Func<double, int> h = percent => (int)Math.Round(percent / 100.0 * source.Width);
			Func<double, int> v = percent => (int)Math.Round(percent / 100.0 * source.Height);
			Func<Color, bool> borderFunc = c => c.R < 10 && c.G < 10 && c.B < 10; // outermost tooltip 1px "black" border

			var cur = Cursor.Position;
			var projectedTooltipWidth = v(39); // 39% of screen resolution, that's pretty much how it scales

			Rectangle searchArea = new Rectangle(Point.Empty, source.Size);
			if (limitSearchArea)
			{
				// limit the area being searched by a rectangle around the mouse cursor to avoid getting incorrect tooltip (when two are visible)
				searchArea = Rectangle.FromLTRB(
					(int)Math.Max(0, cur.X - projectedTooltipWidth * 1.2), // a little more to the left than the projected tooltip width (sometimes the tooltip is not shown directly next to the cursor)
					0,
					Math.Min(cur.X + projectedTooltipWidth * 3 / 4, source.Width),
					(int)(source.Height * 0.8)
				);
			}

			var searchAreaSize = searchArea.Width * searchArea.Height;

			using (var locked = source.Lock())
			{
				var lines = GetTooltipBlackLines(locked, searchArea, projectedTooltipWidth, v);
				if (lines == null || lines.Count == 0)
				{
					return null; // tooltip not found
				}

				var first = lines[0];
				var last = lines.Last();

				// searching for left border
				var range = Enumerable.Range(first.P1.X - v(1), v(0.5)).Reverse();
				var left = FindVerticalBorder(locked, range, first.P1.Y, v, borderFunc);

				range = Enumerable.Range(first.P2.X + v(0.5), v(0.5));
				var right = FindVerticalBorder(locked, range, first.P2.Y, v, borderFunc, false);

				if (left == null || right == null)
				{
					return null; // tooltip not found - why here? :/
				}

				Trace.WriteLine(string.Format("Searched {0:0.00%} of pixels", locked.PixelsRead * 1f / (source.Width * source.Height)));

				var borderWidth = first.P1.X - left.P1.X; // distance between first pixel of first black line and the outer black 1px border found just above
				var ttRect = Rectangle.FromLTRB(left.P1.X, left.P1.Y, right.P1.X + 1, last.P1.Y + borderWidth);

				//g.DrawRectangle(Pens.Lime, ttRect);
				return source.Clone(ttRect, source.PixelFormat);
			}
		}

		private static Line FindVerticalBorder(LockBitmap locked, IEnumerable<int> range, int y, Func<double, int> v, Func<Color, bool> borderFunc, bool isLeft = true)
		{
			foreach (var x in range)
			{
				var pixel = locked.GetPixel(x, y);
				if (borderFunc(pixel))
				{
					var upStart = Math.Max(y - v(6), 0);
					var upRange = Enumerable.Range(upStart, y - upStart).Reverse();
					var downRange = Enumerable.Range(y + 1, locked.Height - y - 1);

					var rng = isLeft ? Enumerable.Range(x + 1, 5) : Enumerable.Range(x - 5, 5).Reverse();
					var up = CountPixelsVertical(locked, borderFunc, upRange, x, rng);
					var down = CountPixelsVertical(locked, borderFunc, downRange, x);

					return new Line(new Point(x, y - up), new Point(x, y + down));
				}
			}

			return null;
		}

		private static List<Line> GetTooltipBlackLines(LockBitmap locked, Rectangle searchArea, int projectedTooltipWidth, Func<double, int> v)
		{
			var lines = new List<Line>();

			Func<Color, bool> blackFunc = c => c.R < 10 && c.G < 10 && c.B < 10;
			var blackLinesThreshold = (int)Math.Floor(0.6 / 100 * locked.Height); // 0,6% of Height

			// It doesn't make sense to start search at the beginning of searchArea as it always covers the upper-left corner, 
			// so we can skip this first vertical scanline to save some time (wouldn't find black lines there anyway).
			// In most cases tooltip will be found in first iteration as long as it was on the left side of the mouse cursor, 
			// otherwise it should take no more than 2 iterations of the outer loop
			for (int x = searchArea.Left + projectedTooltipWidth * 3 / 4; x < searchArea.Right; x += projectedTooltipWidth * 3 / 4)
			{
				int? firstLineX = null;

				//g.DrawLine(Pens.Red, new Point(x, searchArea.Top), new Point(x, searchArea.Bottom));

				for (int y = searchArea.Top; y < searchArea.Bottom; y++)
				{
					var pixel = locked.GetPixel(x, y);

					if (blackFunc(pixel))
					{
						var leftRange = Enumerable.Range(searchArea.Left, x - searchArea.Left).Reverse(); // search until the searchArea.Left as it covers the upper-left corner
						var rightRange = Enumerable.Range(x + 1, (int)(projectedTooltipWidth * 1.2));

						int left = CountPixelsHorizontal(locked, blackFunc, leftRange, y); // count black pixels to the left from current pos
						int right = CountPixelsHorizontal(locked, blackFunc, rightRange, y); // count black pixels to the right from current pos

						var line_width = left + right + 1;
						
						if (line_width > v(39))
						{
							// make bigger steps when longer lines are found, they "waste" a lot of pixels and give no results (step is almost like tooltip border width)
							y += v(0.5);
							continue;
						}
						if (line_width > v(37) && line_width < v(39)) // potential tooltip width, can't calculate it to one pixel's accuracy, need an estimate
						{
							//g.DrawLine(Pens.Lime, x - left, y, x + right, y);

							if (!firstLineX.HasValue && lines.Count > 0 && lines.Last().P1.X != x - left) // group only lines with the same beginning (x-pos)
							{
								lines.Clear();
							}

							if (!firstLineX.HasValue || lines[0].P1.X == x - left)
							{
								lines.Add(new Line(new Point(x - left, y), new Point(x + right, y)));
							}

							if (!firstLineX.HasValue && lines.Count > blackLinesThreshold) // already found the beginning of the tooltip
							{
								firstLineX = lines[0].P1.X;

								// already found a potential tooltip, so need to adjust X a bit, so that it's more efficiently used (as less pixels checked as possible)
								// leaving it as it is would cause the algorithm to find a lot of black areas inside the tooltip (and a lot of pixels read for no reason),
								// but if we move X just next to the left border then the other inner border of the tooltip will stop search very soon for each line, reducing the number of checked pixels massively
								// Note: it's safe to modify a loop iterator as the code won't iterate more over the outer loop anyway (see below: #1)
								x = firstLineX.Value;

								searchArea.Height = locked.Height - searchArea.Top; // extend searchArea to the bottom (almost.. see comment below)
							}
						}
					}
					else if (firstLineX.HasValue) // no need to search until the very bottom of the screenshot, last "black" pixel is for sure last "black" line in the tooltip
					{
						break;
					}
				}

				if (firstLineX.HasValue) // #1
				{
					return lines;
				}
			}

			return null;
		}

		/// <summary>
		/// Counts pixels of given color. Search in X coordinate given and Y range given.
		/// </summary>
		/// <param name="colorFunc">function that checks if the color should be counted or not</param>
		/// <param name="additionalRange">X range to be checked as additional stop condition, used to find upper tooltip boundary</param>
		/// <returns>number of pixels of given color found</returns> 
		private static int CountPixelsVertical(LockBitmap locked, Func<Color, bool> colorFunc, IEnumerable<int> range, int x, IEnumerable<int> additionalRange = null)
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
		/// <param name="colorFunc">function that checks if the color should be counted or not</param>
		/// <returns>number of pixels of given color found</returns> 
		private static int CountPixelsHorizontal(LockBitmap locked, Func<Color, bool> colorFunc, IEnumerable<int> xRange, int y)
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

		#endregion		
    }
}
