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

namespace Tests
{
	public partial class frmMain : Form
	{
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

		private void HookManager_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == System.Windows.Forms.Keys.F7)
			{
				HandleScreenshot();
			}
		}

		private void HandleScreenshot()
		{
			long time1, time2;

			var bmp = GetScreenshot();// new Bitmap(@"..\..\..\last_screen.png");

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
			lvi.SubItems.AddRange(new string[] { name2, time1.ToString(), time2.ToString()});
			lvConsole.Items.Add(lvi);

			lvi.EnsureVisible();
		}

		private Bitmap GetScreenshot()
		{
			var procs = Process.GetProcessesByName("Diablo III");
			if (procs.Length > 0)
			{
				return Screenshot.GetSnapShot(procs[0]);
			}

			return null;
		}
	}
}
