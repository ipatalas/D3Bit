using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace D3Bit.Tests
{
	[TestFixture]
	public class TooltipTests
	{
		static object[] TestFiles;
		static string path;

		static TooltipTests()
		{
			path = Environment.CurrentDirectory; // for some reason path here is correct, but in the tests it's not...

			var list = new ArrayList();
			var di = new DirectoryInfo("Tooltips");

			var items = JsonConvert.DeserializeObject<TooltipData[]>(File.ReadAllText(@"Tooltips\items.json"));

			foreach (var fi in di.GetFiles("*.png", SearchOption.AllDirectories))
			{
				var split = fi.Directory.Name.Split('x');
				var res = new Size(int.Parse(split[0]), int.Parse(split[1]));

				var idx = int.Parse(Path.GetFileNameWithoutExtension(fi.Name));

				list.Add(new object[] { res, fi.FullName, items[idx - 1] });
			}

			TestFiles = list.ToArray();
		}

		[Test, TestCaseSource("TestFiles")]
		public void test_if_tooltips_properties_are_read_correctly_NO_NAMES_CHECKED(Size resolution, string filename, TooltipData data)
		{
			Environment.CurrentDirectory = path;

			var bmp = Bitmap.FromFile(filename) as Bitmap;

			try
			{
				var tt = new Tooltip(bmp);
				var results = new TooltipData(tt);

				Trace.TraceInformation("Tooltip scanned in {0}ms", results.TimeTaken);

				var fn = string.Format("{0}x{1}_{2}", resolution.Width, resolution.Height, Path.GetFileName(filename));
				tt.Processed.Save(fn, ImageFormat.Png);

				Assert.AreEqual(data.Type, results.Type);
				Assert.AreEqual(data.Quality, results.Quality);
				Assert.AreEqual(data.Meta, results.Meta);
				Assert.AreEqual(data.DPS, results.DPS);

				File.Delete(fn);
			}
			finally
			{
				bmp.Dispose();
			}
		}
	}
}
