using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3Bit;
using System.Drawing.Imaging;

namespace Tests.Extensions
{
	public static class TooltipExtensions
	{
		//public static void Save(this Tooltip_old tt, string path)
		//{
		//    tt.Processed.Save(path, ImageFormat.Bmp);
		//}

		public static void Save(this Tooltip tt, string path)
		{
			tt.Processed.Save(path, ImageFormat.Bmp);
		}
	}
}
