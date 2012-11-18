using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using D3Bit;
using Gma.UserActivityMonitor;
using System.IO;
using System.Drawing.Imaging;
using D3Bit.Extensions;
using Tests.Properties;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Tests
{
	public partial class frmMain : Form
	{
		private const string UpperCornerName = "ucorner.png";
		private const string BottomCornerName = "bcorner.png";

		private const string TooltipsPath = @"d:\Games\Diablo III\!Utils\D3Bit_Client_1.1.6g\Sources\Tooltips";
		private Dictionary<Size, int> nameCounter;

		public frmMain()
		{
			InitializeComponent();

			nameCounter = new Dictionary<Size, int>();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			HookManager.KeyUp += HookManager_KeyUp;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			Settings.Default.WindowLocation = Location;
			Settings.Default.WindowState = WindowState;
			Settings.Default.Save();
		}

		private void HookManager_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Trace.TraceInformation("KeyUp code: {0}", e.KeyCode);

			if (e.KeyCode == System.Windows.Forms.Keys.F7)
			{
				HandleScreenshot();
			}
			else if (e.KeyCode == Keys.F8)
			{
				HandleTooltipArea();
				//HandleIdent();
			}
			else if (e.KeyCode == Keys.F9)
			{
				HandleItem();
			}
			else if (e.KeyCode == Keys.F10)
			{
				BatchSaveForUnitTests();
			}
		}

		void HandleItem()
		{
			tabControl1.SelectTab(tabItem);
			panelDebugPictures.Controls.Clear();

			Trace.TraceInformation("Hello :)");

			var bmp = GetDiabloScreenshot();
			
			bmp.Save("last_screenshot.png", ImageFormat.Png);

			var result = Screenshot.GetTooltip(bmp);
			if (result == null)
			{
				tbItemSpecs.Text = "Tooltip not found";
				return;
			}
			result.Save("last.png", ImageFormat.Png);

			var sw = Stopwatch.StartNew();
			var tt = new D3Bit.Tooltip(result);
			var r = new Results(tt);
			sw.Stop();

			pbItem.Image = tt.Processed;

			//SaveForUnitTests(r, result, bmp.Size);

			//var sb = new StringBuilder();
			//sb.AppendLine("Name: {0}", r.Name);
			//sb.AppendLine("Type: {0} {1}", r.Quality, r.Type);
			//sb.AppendLine("DPS: {0}", r.DPS);
			//sb.AppendLine("Meta: {0}", r.Meta);
			//sb.AppendLine("Affixes:");
			//sb.Append(r.Affixes.Aggregate(new StringBuilder(), (o, v) => o.AppendLine("\t{0} = {1}", v.Key, v.Value)).ToString());
			//sb.AppendLine("Socket bonuses: {0}", r.SocketBonuses);
			//sb.AppendLine();
			//sb.AppendLine("{0}ms", sw.ElapsedMilliseconds);
			
			tbItemSpecs.Text = JsonConvert.SerializeObject(r, Formatting.Indented);

#if DEBUG
			foreach (var item in tt.DebugBitmaps)
			{
				var pb = new PictureBox();
				pb.SizeMode = PictureBoxSizeMode.AutoSize;
				pb.Image = item;
				pb.ContextMenuStrip = cmPictures;

				panelDebugPictures.Controls.Add(pb);
			}
#endif
		}

		[Conditional("EXTRACT_TOOLTIPS")]
		private void BatchSaveForUnitTests()
		{
			var path = @"d:\Programs\Benchmarks\Fraps\Screenshots";

			var pb = new ProgressBar();
			pb.Maximum = 18;
			pb.Dock = DockStyle.Bottom;
			Controls.Add(pb);
			pb.Show();

			foreach (var file in Directory.GetFiles(path, "*.png"))
			{
				var bmp = Bitmap.FromFile(file) as Bitmap;
				var result = Screenshot.GetTooltip(bmp, false);
				if (result == null)
				{
					Debugger.Break();
					return;
				}
				
				var tt = new D3Bit.Tooltip(result);
				var r = new Results(tt);
				
				pbItem.Image = tt.Processed;

				SaveForUnitTests(r, result, bmp.Size);
				pb.Increment(1);
				Trace.TraceInformation("Saved " + r.Name);
				bmp.Dispose();
			}
			pb.Hide();
			MessageBox.Show("Done!");
		}

		[Conditional("EXTRACT_TOOLTIPS")]
		private void SaveForUnitTests(Results r, Bitmap result, System.Drawing.Size size)
		{
			var path = Path.Combine("Tooltips", string.Format("{0}x{1}", size.Width, size.Height));
			Directory.CreateDirectory(path);

			string filepath;
			int i = 0;
			do
			{
				i++;
				filepath = Path.Combine(path, i.ToString("00") + ".png");
			} while (File.Exists(filepath));

			result.Save(filepath, ImageFormat.Png);


			//File.AppendAllText(@"Tooltips\items.json", JsonConvert.SerializeObject(r, Formatting.Indented));
		}

		#region [ Tooltip search tests ]
		private void HandleTooltipArea()
		{
			tabControl1.SelectTab(tabTooltipSearch);

			var bmp = GetDiabloScreenshot();

			var pos = Cursor.Position;
			var projectedTooltipWidth = (int)(bmp.Height * 0.39); // 39% of screen resolution

			var rect = Rectangle.FromLTRB((int)Math.Max(0, pos.X - projectedTooltipWidth * 1.2), 0, Math.Min(pos.X + projectedTooltipWidth, bmp.Width), (int)(bmp.Height * 0.7));

			var clone = (Bitmap)bmp.Clone();
			using (var g = Graphics.FromImage(bmp))
			{
				g.DrawRectangle(Pens.Red, rect);
				var sw = Stopwatch.StartNew();
				//Screenshot.GetToolTip(bmp);
				var tt = GetTooltip_LinesV2(clone, rect, g);
				//GetTooltip_ImageSearch(bmp, rect, g);
				sw.Stop();
				g.DrawString(string.Format("{0} ms", sw.ElapsedMilliseconds), SystemFonts.DefaultFont, Brushes.Lime, 20, 20);
			}

			pictureBox1.Image = bmp;
		}

		private void HandleIdent()
		{
			tabControl1.SelectTab(tabTooltipSearch);

			var bmp = GetDiabloScreenshot();
			
			var path = @"pics\ident.png";
			var findImg = "*TRANSBLACK *45 " + path;

			var result = ImageUtil.ImageSearch(0, 0, bmp.Width, bmp.Height, findImg + UpperCornerName);
			if (result != "0")
			{
				var start = ImageSearchResultToRectangle(result);

				using (var g = Graphics.FromImage(bmp))
				{
					g.DrawRectangle(Pens.Lime, start);
				}
			}

			pictureBox1.Image = bmp;
		}

		public Bitmap GetTooltip_LinesV2(Bitmap source, Rectangle searchArea, Graphics g)
		{			
			Func<double, int> h = percent => (int)Math.Round(percent / 100.0 * source.Width);
			Func<double, int> v = percent => (int)Math.Round(percent / 100.0 * source.Height);

			Func<Color, bool> borderFunc = c => c.R < 10 && c.G < 10 && c.B < 10; // tooltip 1px border

			var searchAreaSize = searchArea.Width * searchArea.Height;
			var projectedTooltipWidth = v(39); // 39% of screen resolution
			
			using (var locked = source.Lock())
			{
				var lines = GetTooltipBlackLines(locked, searchArea, projectedTooltipWidth, v, g);
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

				var borderWidth = first.P1.X - left.P1.X; // distance between first pixel of first black line and the outer black 1px border found just above
				var ttRect = Rectangle.FromLTRB(left.P1.X, left.P1.Y, right.P1.X + 1, last.P1.Y + borderWidth);

				g.DrawRectangle(Pens.Lime, ttRect);

				Trace.WriteLine(string.Format("Searched {0:0.00%} of pixels", locked.PixelsRead * 1f / (source.Width * source.Height)));

				return source.Clone(ttRect, source.PixelFormat);
			}
		}

		private Line FindVerticalBorder(LockBitmap locked, IEnumerable<int> range, int y, Func<double, int> v, Func<Color, bool> borderFunc, bool isLeft = true)
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

		private List<Line> GetTooltipBlackLines(LockBitmap locked, Rectangle searchArea, int projectedTooltipWidth, Func<double, int> v, Graphics g)
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

				g.DrawLine(Pens.Red, x, searchArea.Top, x, searchArea.Bottom);

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
							// make bigger steps when longer lines are found, they waste a lot of pixels and give no results (step is almost like tooltip border width)
							y += v(0.5);
							continue;
						}
						if (line_width > v(37) && line_width < v(39)) // potential tooltip width, can't calculate it to the pixel's accuracy
						{
							if (!firstLineX.HasValue && lines.Count > 0 && lines.Last().P1.X != x - left) // group only lines with the same x-pos
							{								
								lines.Clear();
							}

							if (!firstLineX.HasValue || lines[0].P1.X == x - left)
							{
								lines.Add(new Line(new Point(x - left, y), new Point(x + right, y)));
								g.DrawLine(Pens.Lime, lines.Last().P1, lines.Last().P2);
							}

							if (!firstLineX.HasValue && lines.Count > blackLinesThreshold) // just found the beginning of the tooltip
							{
								firstLineX = lines[0].P1.X;

								// already found a potential tooltip, so need to adjust X a bit, so that it's more efficiently used (as less pixels checked as possible)
								// leaving it as it is would cause the algorithm to find a lot of black areas inside the tooltip (and a lot of pixels read for no reason),
								// but if we move X just next to the left border then the other inner border of the tooltip will stop search very soon for each line, reducing the number of checked pixels massively
								// Note: it's safe to modify a loop iterator as the code won't iterate more over the outer loop anyway (see below: if (firstLineX.HasValue))
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

				if (firstLineX.HasValue)
				{
					return lines;
				}
			}

			return null;
		}

		private int CountPixelsVertical(LockBitmap locked, Func<Color, bool> colorFunc, IEnumerable<int> range, int x, IEnumerable<int> additionalRange = null)
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

		private int CountPixelsHorizontal(LockBitmap locked, Func<Color, bool> colorFunc, IEnumerable<int> range, int y)
		{			
			int count = 0;

			foreach (var x in range)
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

		#region [ ImageSearch tests ]
		public static Bitmap GetTooltip_ImageSearch(Bitmap source, Rectangle searchArea, Graphics g)
		{
			var searchAreaSize = searchArea.Width * searchArea.Height;
			var projectedTooltipWidth = (int)(source.Height * 0.39); // 39% of screen resolution

			var path = string.Format(@"pics\{0}x{1}\", source.Width, source.Height);
			if (!Directory.Exists(path))
			{
				// fallback to legacy mechanism
				throw new Exception(string.Format("Cannot find pics for {0}x{1}", source.Width, source.Height));
			}

			var findImg = "*TRANSBLACK *15 " + path;

			var result = ImageUtil.ImageSearch(searchArea.Left, searchArea.Top, searchArea.Right, searchArea.Bottom, findImg + UpperCornerName);
			if (result == "0")
			{
				Trace.TraceWarning("Upper-left corner not found...");
				return null;
			}

			var start = ImageSearchResultToRectangle(result);

			g.DrawRectangle(Pens.Lime, start);

			searchArea = Rectangle.FromLTRB(start.Left + (int)(projectedTooltipWidth * 0.9), start.Top, start.Left + (int)(projectedTooltipWidth * 1.1), source.Height);
			searchAreaSize += searchArea.Width * searchArea.Height;
			g.DrawRectangle(Pens.Navy, searchArea);

			Trace.WriteLine(string.Format("Searched {0:0%} of pixels", searchAreaSize * 1f / (source.Width * source.Height)));

			result = ImageUtil.ImageSearch(searchArea.Left, searchArea.Top, searchArea.Right, searchArea.Bottom, findImg + BottomCornerName);
			if (result == "0")
			{
				Trace.TraceWarning("Bottom-right corner not found...");
				return null;
			}

			var end = ImageSearchResultToRectangle(result);
			g.DrawRectangle(Pens.Lime, end);

			var bounds = Rectangle.FromLTRB(start.Left, start.Top, end.Right, end.Bottom);

			return source.Clone(bounds, source.PixelFormat);
		}

		static Rectangle ImageSearchResultToRectangle(string result)
		{
			var split = result.Split('|').ToList().ConvertAll(x => int.Parse(x));

			return new Rectangle(split[1], split[2], split[3], split[4]);
		}  
		#endregion
		#endregion

		#region [ Items tests ]
		private void HandleScreenshot()
		{
			tabControl1.SelectTab(tabItems);

			long time1, time2;

			var bmp = GetDiabloScreenshot();// new Bitmap(@"..\..\..\last_screen.png");

			var sw = Stopwatch.StartNew();
			var result = Screenshot.GetTooltip(bmp);
			sw.Stop();
			time1 = sw.ElapsedMilliseconds;

			if (!nameCounter.ContainsKey(bmp.Size))
			{
				nameCounter.Add(bmp.Size, 1);
			}

			if (result == null)
			{
				AddError("Cannot recognize tooltip with ImageSearch");
			}
#if EXTRACT_TOOLTIPS
			else
			{
				SaveTooltip(result, bmp.Size, "imagesearch");
			}
#endif			

			AddItem("Unknown - " + Cursor.Position.ToString(), "Unknown", time1, 0);
			Console.WriteLine("Tooltip extracted in {0}ms", sw.ElapsedMilliseconds);

			nameCounter[bmp.Size]++;
		}

		private void SaveTooltip(Bitmap result, Size size, string suffix)
		{
			do
			{
				var path = Path.Combine(TooltipsPath, string.Format("{0}x{1}_{2}_{3}.bmp", size.Width, size.Height, nameCounter[size], suffix));
				if (File.Exists(path))
				{
					nameCounter[size]++;
					continue;
				}

				result.Save(path, ImageFormat.Bmp);
				break;
			} while (true);
		}

		private void AddError(string message)
		{
			var lvi = new ListViewItem(message);
			lvi.ForeColor = Color.Red;

			lvConsole.Items.Add(lvi);
			lvi.EnsureVisible();
		}

		private void AddItem(string name1, string name2, long time1, long time2)
		{
			var lvi = new ListViewItem(name1);
			lvi.SubItems.AddRange(new string[] { name2, time1.ToString(), time2.ToString() });
			lvConsole.Items.Add(lvi);

			lvi.EnsureVisible();
		} 
		#endregion

		private Bitmap GetDiabloScreenshot()
		{
			var procs = Process.GetProcessesByName("Diablo III");
			if (procs.Length > 0)
			{
				return Screenshot.GetSnapShot(procs[0]);
			}

			return null;
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var pbox = cmPictures.SourceControl as PictureBox;
			if (pbox != null && pbox.Image != null)
			{
				if (sfdImages.ShowDialog() == DialogResult.OK)
				{
					pbox.Image.Save(sfdImages.FileName, ImageFormat.Png);
				}
			}
		}

		private void toClipboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var pbox = cmPictures.SourceControl as PictureBox;
			if (pbox != null && pbox.Image != null)
			{
				Clipboard.SetImage(pbox.Image);
			}
		}
	}
}
