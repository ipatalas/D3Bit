﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using D3Bit;
using Point = System.Drawing.Point;
using ThreadState = System.Threading.ThreadState;
using System.Globalization;
using D3BitGUI.Utils;

namespace D3BitGUI
{
	public partial class CardForm : Form
	{
		#region [ Fields & Properties ]
		public string TooltipPath { get; private set; }

		private int MAX_PROGRESS_STEPS = 6;

		private Bitmap _bitmap;
		private Bitmap _tooltipBitmap;
		private Thread _thread;
		private bool _needToClose = false;

		private Dictionary<string, string> _info;
		private Dictionary<string, string> _affixes; 
		#endregion

		public CardForm()
		{
			InitializeComponent();
			Location = Properties.Settings.Default.ItemCardStartPosition;
			Cursor = Cursors.SizeAll;
			progressBar.Maximum = MAX_PROGRESS_STEPS;

			string bgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bg.jpg");
			browser.ObjectForScripting = new ScriptInterface(this);
			browser.DocumentText = "<body style=\"background:url(" + bgPath + ")\"></body>";

			_info = new Dictionary<string, string>();
			_info.Add("name", "");
			_info.Add("quality", "");
			_info.Add("type", "");
			_info.Add("meta", "");
			_info.Add("dps", "");
			_info.Add("stats", "");
		}

		public CardForm(Bitmap snapshot)
			: this()
		{
			_bitmap = snapshot;
			_thread = new Thread(Process);
			_thread.Start();
		}

		void Process()
		{
			var sw = Stopwatch.StartNew();
			_tooltipBitmap = Screenshot.GetTooltip_ImageSearch(_bitmap);
			sw.Stop();

			GUI.Debug("Tooltip extracted in {0}ms", sw.ElapsedMilliseconds);

			if (_tooltipBitmap == null)
			{
				this.UIThread(Abort);
				return;
			}
			try
			{
				TooltipPath = string.Format("tmp/{0}.png", DateTime.Now.Ticks);
				_tooltipBitmap.Save(TooltipPath, ImageFormat.Png);

				Tooltip tooltip = new Tooltip(_tooltipBitmap);
				_info["name"] = tooltip.ParseItemName();
				IncreaseProgress();
				string quality = "Unknown";
				_info["type"] = tooltip.ParseItemType(out quality);
				_info["quality"] = quality;
				IncreaseProgress();
				_info["dps"] = tooltip.ParseDPS().ToString(CultureInfo.InvariantCulture);
				IncreaseProgress();
				_info["meta"] = tooltip.ParseMeta();
				IncreaseProgress();
				string socketBonuses = "";
				_affixes = tooltip.ParseAffixes(out socketBonuses);
				if (socketBonuses != "")
					_info["meta"] += _info["meta"] == "" ? socketBonuses : "," + socketBonuses;
				_info["stats"] = String.Join(", ", _affixes.Select(kv => (kv.Value + " " + kv.Key).Trim()));
				IncreaseProgress();
				
				tooltip.Processed.Save("s.png", ImageFormat.Png);
				this.UIThread(() => progressBar.Visible = false);

				Func<string, string> u = System.Uri.EscapeDataString;
				string url = String.Format("http://d3bit.com/c/?image={0}&battletag={1}&build={2}&secret={3}&{4}&test=1",
										   u(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TooltipPath)),
										   u(Properties.Settings.Default.Battletag), Properties.Settings.Default.D3UpDefaultBuildNumber,
										   u(Properties.Settings.Default.Secret.Trim()), Util.FormGetString(_info));
				browser.Url = new Uri(url);
				//GUI.Log(url);

				GUI.SoundFeedback(true);
				this.UIThread(BringToFront);
			}
			catch (Exception ex)
			{
				GUI.Log(ex.Message);
				GUI.Log(ex.StackTrace);
				this.UIThread(Abort);
				return;
			}
		}

		void IncreaseProgress()
		{
			this.UIThread(() => progressBar.Value++);
		}

		void Abort()
		{
			GUI.SoundFeedback(false);
			GUI.Log("Error Scanning...");
			Close();
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= WinAPI.CS_DROPSHADOW;
				return cp;
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape) this.UIThread(() => _needToClose = true);
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void CardForm_Paint(object sender, PaintEventArgs e)
		{
			using (var hb = new HatchBrush(HatchStyle.Percent50, this.TransparencyKey))
			{
				e.Graphics.FillRectangle(hb, this.DisplayRectangle);
			}
		}

		private void CardForm_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				WinAPI.ReleaseCapture();
				WinAPI.SendMessage(Handle, WinAPI.WM_NCLBUTTONDOWN, WinAPI.HT_CAPTION, 0);
			}
		}

		private void CardForm_Load(object sender, EventArgs e)
		{
			System.IntPtr ptr = WinAPI.CreateRoundRectRgn(0, 0, this.Width, this.Height, 14, 14);
			this.Region = System.Drawing.Region.FromHrgn(ptr);
			WinAPI.DeleteObject(ptr);
		}

		private void CardForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_thread != null && _thread.ThreadState == ThreadState.Running)
				_thread.Abort();
		}

		private void updater_Tick(object sender, EventArgs e)
		{
			//if (_progressStep < MAX_PROGRESS_STEPS)
			//{
			//    progressBar.Value = _progressStep;
			//}
			if (_needToClose)
				Close();
		}

		private void CardForm_LocationChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.ItemCardStartPosition = Location;
			Properties.Settings.Default.Save();
		}
	}
}
