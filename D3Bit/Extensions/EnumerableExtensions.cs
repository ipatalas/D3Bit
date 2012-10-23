using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3Bit.Extensions
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> Sample<T>(this IEnumerable<T> source, int interval)
		{
			// null check, out of range check go here
			return source.Where((value, index) => index % interval == 0);
		}
	}
}
