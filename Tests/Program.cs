#define DEBUG_BITMAPS

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3Bit;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Gma.UserActivityMonitor;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Tests.Extensions;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace Tests
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			ExtractTooltips();
			return;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new frmMain());
		}

		static void ExtractTooltips()
		{
			var dstPath = @"..\..\..\!Data\Tooltips";
			var imagesPath = Path.Combine(dstPath, "images");
				
			var specificTooltip = "";

			var files = Directory.GetFiles(dstPath, "1920x1200*_imagesearch.bmp");//.Take(10).ToArray();
			int i = 1;

			var html = new StringBuilder();			
			html.AppendLine("<!DOCTYPE html>");
			html.AppendLine(@"<html xmlns=""http://www.w3.org/1999/xhtml"">");
			html.AppendLine("<head>");
			html.AppendLine(@"<link rel=""Stylesheet"" type=""text/css"" href=""styles.css"">");
			html.AppendLine("</head>");
			html.AppendLine("<body>").AppendLine(@"<table cellpadding=""0"" cellspacing=""0"">");

			WarmUp(files[0]);

			var results1 = new List<Results>();
			var results2 = new List<Results>();

			foreach (var file in files)
			{
				if (!string.IsNullOrEmpty(specificTooltip) && !file.Contains(specificTooltip))
				{
					continue;
				}

				var filename = Path.GetFileName(file);

				var m = Regex.Match(filename, @"(?<W>\d+)x(?<H>\d+)");
				int width = int.Parse(m.Groups["W"].Value);
				int height = int.Parse(m.Groups["H"].Value);

				Bitmap bitmap = Bitmap.FromFile(file) as Bitmap;

				var path = Path.Combine(imagesPath, Path.GetFileNameWithoutExtension(file) + ".jpg");
				if (!File.Exists(path))
				{
					bitmap.Save(path, ImageFormat.Jpeg);
				}

				var tt2 = new TooltipWrapper<TooltipV2>(new TooltipV2(bitmap), width, height);
				var result2 = new Results(tt2);
				tt2.WrappedTooltip.SaveTemp(Path.Combine(dstPath, "tmp_v2", filename));

				var tt = new TooltipWrapper<TooltipV1>(new TooltipV1(bitmap), width, height);
				var result1 = new Results(tt);
				tt.WrappedTooltip.SaveTemp(Path.Combine(dstPath, "tmp", filename));

				var diffClass = result1.GetHashCode() == result2.GetHashCode() ? "" : "different";

				html.AppendLine(@"<tr class=""{0}"">", diffClass);
				
				html.AppendLine(@"<td class=""tooltip"">");
				html.AppendLine("<div>");
				html.AppendLine("<img src='images/{0}' />", Path.GetFileName(path));
				html.AppendLine("<span>{0}x{1}</span>", width, height);
				html.AppendLine("</div>");
				html.AppendLine("</td>");

				html.AppendLine("<td class='tt1'>");
				html.AppendLine("<img src='tmp/{0}' />", filename);
				html.AppendLine("</td>");

				html.AppendLine("<td class='tt2'>");
				html.AppendLine("<img src='tmp_v2/{0}' />", filename);
				html.AppendLine("</td>");

				html.AppendLine(@"<td class=""original"">");
				html.AppendLine("<pre>");
				RenderResults(html, result1);
				html.AppendLine("</pre>");
				html.AppendLine("</td>");

				html.AppendLine(@"<td class=""improved"">");
				html.AppendLine("<pre>");
				RenderResults(html, result2);
				html.AppendLine("</pre>");
				html.AppendLine("</td>");
				
				html.AppendLine("</tr>");

				results1.Add(result1);
				results2.Add(result2);

				Console.WriteLine("{0}/{1}", i++, files.Count());
			}

			var r1 = results1.Sum(r => r.TimeTaken) / 1000.0;
			var	r2 = results2.Sum(r => r.TimeTaken) / 1000.0;

			html.AppendLine("<tr>{0}<td class='performance'>{1:0.00}s</td><td class='performance'>{2:0.00}s ({3:0%})</td></tr>", 
				string.Join("", Enumerable.Repeat("<td></td>", 3)), 
				r1, r2, (r2 - r1) / r1
			);

			html.AppendLine("</table>");
			html.AppendLine("</body>").AppendLine("</html>");

			File.WriteAllText(Path.Combine(dstPath, "results.html"), html.ToString());
		}

		private static void WarmUp(string p)
		{
			var bitmap = Bitmap.FromFile(p) as Bitmap;
			var tt = new TooltipV1(bitmap);
			var result1 = new Results(tt);
		}

		private static void RenderResults(StringBuilder html, Results r)
		{
			html.AppendLine("<strong>Name</strong>: " + r.Name);
			html.AppendLine("<strong>Type</strong>: " + r.Quality + " " + r.Type);
			html.AppendLine("<strong>DPS</strong>: " + r.DPS);
			html.AppendLine("<strong>Meta</strong>: " + r.Meta).AppendLine();
			//html.AppendLine("<strong>Affixes</strong>: ")
			//    .AppendLine(r.Affixes.Aggregate(new StringBuilder(), (sb, kvp) => sb.AppendLine("<strong>{0}</strong> = {1}", kvp.Key, kvp.Value)).ToString());
			//html.AppendLine("<strong>Socket bonuses</strong>: " + r.SocketBonuses);
			html.AppendLine().AppendLine();

			html.AppendLine(@"<span class=""performance"">{0}ms</span>", r.TimeTaken);
		}
	}

	public class Results
	{
		public string Name { get; private set; }
		public string Meta { get; private set; }
		public string Type { get; private set; }
		public string Quality { get; private set; }
		public string SocketBonuses { get; private set; }
		public double DPS { get; private set; }
		public Dictionary<string, string> Affixes { get; private set; }

		public long TimeTaken { get; private set; }

		public Results(IResultProvider provider)
		{
			Stopwatch timer = new Stopwatch();
			string sockets = string.Empty, quality = "Unknown";


			timer.Start();
			DPS = provider.ParseDPS();
			//Name = provider.ParseItemName();
			//Type = provider.ParseItemType(out quality);
			Meta = provider.ParseMeta();
			//Affixes = provider.ParseAffixes(out sockets);

			Quality = quality;
			SocketBonuses = sockets;
			timer.Stop();
			TimeTaken = timer.ElapsedMilliseconds;			
		}

		public override int  GetHashCode()
		{
 			var sb = new StringBuilder();
			sb.AppendLine(Name);
			sb.AppendLine(Type);
			sb.AppendLine(Quality);
			sb.AppendLine(DPS.ToString());
			sb.AppendLine(Meta);
			if (Affixes != null)
			{
				sb.AppendLine(Affixes.Aggregate(new StringBuilder(), (sb2, kvp) => sb2.AppendLine("{0}={1}", kvp.Key, kvp.Value)).ToString());
			}
			sb.AppendLine(SocketBonuses);

			return sb.ToString().GetHashCode();
		}
	}
}
