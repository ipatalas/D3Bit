using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace D3Bit.Utils
{
	public class PerformanceLog : IDisposable
	{
		private Stopwatch stopwatch;
		public string Format { get; set; }

		public PerformanceLog(string format = null)
		{
			Format = format;
			stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			stopwatch.Stop();

			WriteResults(Format);
		}

		private void WriteResults(string Format)
		{
			if (string.IsNullOrEmpty(Format))
			{
				Format = "PerformanceLog: {0}ms";
			}
			else if (!Format.Contains("{0}"))
			{
				Format += " {0}ms";
			}

			Debug.WriteLine(string.Format(Format, stopwatch.ElapsedMilliseconds));
		}
	}
}
