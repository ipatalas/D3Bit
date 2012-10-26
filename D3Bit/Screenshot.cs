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
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

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
                GetWindowRect(d3Proc.MainWindowHandle, out rc);

                Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format24bppRgb);

				using (var g = Graphics.FromImage(bmp))
				{
					g.CopyFromScreen(rc.X, rc.Y, 0, 0, new Size(rc.Width, rc.Height), CopyPixelOperation.SourceCopy);
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

		/// <summary>
		/// Looks for the tooltip based on upper-left and bottom-right graphics
		/// </summary>
		/// TODO: limit the area being search by a rectangle around the mouse cursor to avoid getting incorrect tooltip
		public static Bitmap GetTooltip_ImageSearch(Bitmap source)
		{
			var path = string.Format(@"pics\{0}x{1}\", source.Width, source.Height);
			if (!Directory.Exists(path))
			{
				Trace.WriteLine("GetTooltip_ImageSearch(): Falling back to legacy mechanism");
				// fallback to legacy mechanism
				return GetToolTip(source);
			}

			var cur = Cursor.Position;
			var projectedTooltipWidth = (int)(source.Height * 0.39); // 39% of screen resolution height

			var searchArea = Rectangle.FromLTRB(
				(int)Math.Max(0, cur.X - projectedTooltipWidth * 1.2), // a little more to the left than the projected tooltip width (sometimes the tooltip is not shown directly next to the cursor)
				0, 
				Math.Min(cur.X + projectedTooltipWidth * 3 / 4, source.Width),
				(int)(source.Height * 0.7)
			);

			var findImg = "*TRANSBLACK *15 " + path;

			var result = ImageUtil.ImageSearch(searchArea.Left, searchArea.Top, searchArea.Right, searchArea.Bottom, findImg + UpperCornerName);
			if (result == "0")
			{
				Trace.TraceWarning("Upper-left corner not found...");
				return null;
			}

			var start = ImageSearchResultToRectangle(result);

			searchArea = Rectangle.FromLTRB(
				start.Left + (int)(projectedTooltipWidth * 0.9), 
				start.Top, 
				start.Left + (int)(projectedTooltipWidth * 1.1), 
				source.Height
			);

			result = ImageUtil.ImageSearch(searchArea.Left, searchArea.Top, searchArea.Right, searchArea.Bottom, findImg + BottomCornerName);
			if (result == "0")
			{
				Trace.TraceWarning("Bottom-right corner not found...");
				return null;
			}

			var end = ImageSearchResultToRectangle(result);
			var bounds = Rectangle.FromLTRB(start.Left, start.Top, end.Right, end.Bottom);

			return source.Clone(bounds, source.PixelFormat);
		}

		static Rectangle ImageSearchResultToRectangle(string result)
		{
			var split = result.Split('|').ToList().ConvertAll(x => int.Parse(x));

			return new Rectangle(split[1], split[2], split[3], split[4]);
		}
    }
}
