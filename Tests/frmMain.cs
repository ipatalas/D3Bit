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
		}

		void HandleItem()
		{
			tabControl1.SelectTab(tabItem);
			panelDebugPictures.Controls.Clear();			

			var bmp = GetDiabloScreenshot();

			var result = Screenshot.GetTooltip_ImageSearch(bmp);
			if (result == null)
			{
				tbItemSpecs.Text = "Tooltip not found";
				return;
			}
			result.Save("last.png", ImageFormat.Png);

			string quality, socketBonuses;

			var sw = Stopwatch.StartNew();
			var tt = new D3Bit.Tooltip(result);
			var name = tt.ParseItemName();
			var dps = tt.ParseDPS();
			var type = tt.ParseItemType(out quality);
			var meta = tt.ParseMeta();
			var affixes = tt.ParseAffixes(out socketBonuses);
			sw.Stop();

			pbItem.Image = tt.Processed;

			var sb = new StringBuilder();
			sb.AppendLine("Name: {0}", name);
			sb.AppendLine("Type: {0} {1}", quality, type);
			sb.AppendLine("DPS: {0}", dps);
			sb.AppendLine("Meta: {0}", meta);
			sb.AppendLine("Affixes:");
			sb.Append(affixes.Aggregate(new StringBuilder(), (o, v) => o.AppendLine("\t{0} = {1}", v.Key, v.Value)).ToString());
			sb.AppendLine("Socket bonuses: {0}", socketBonuses);
			sb.AppendLine();
			sb.AppendLine("{0}ms", sw.ElapsedMilliseconds);
			
			tbItemSpecs.Text = sb.ToString();

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

		#region [ Tooltip search tests ]
		private void HandleTooltipArea()
		{
			tabControl1.SelectTab(tabTooltipSearch);

			var bmp = GetDiabloScreenshot();

			var pos = Cursor.Position;
			var projectedTooltipWidth = (int)(bmp.Height * 0.39); // 39% of screen resolution

			var rect = Rectangle.FromLTRB((int)Math.Max(0, pos.X - projectedTooltipWidth * 1.2), 0, Math.Min(pos.X + projectedTooltipWidth * 3 / 4, bmp.Width), (int)(bmp.Height * 0.7));
			using (var g = Graphics.FromImage(bmp))
			{
				g.DrawRectangle(Pens.Red, rect);
				GetTooltip_ImageSearch(bmp, rect, g);
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

		#region [ Items tests ]
		private void HandleScreenshot()
		{
			tabControl1.SelectTab(tabItems);

			long time1, time2;

			var bmp = GetDiabloScreenshot();// new Bitmap(@"..\..\..\last_screen.png");

			var sw = Stopwatch.StartNew();
			var result = Screenshot.GetTooltip_ImageSearch(bmp);
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

			sw.Restart();
			result = Screenshot.GetToolTip(bmp);
			sw.Stop();
			time2 = sw.ElapsedMilliseconds;

			if (result == null)
			{
				AddError("Cannot recognize tooltip with legacy method");
			}
#if EXTRACT_TOOLTIPS
			else
			{
				//SaveTooltip(result, bmp.Size, "lines");
			}
#endif

			AddItem("Unknown - " + Cursor.Position.ToString(), "Unknown", time1, time2);
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
	}
}
