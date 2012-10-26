using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3Bit.Extensions
{
	public static class StringExtensions
	{
		public static double DiceCoefficient(this string input, string compareTo)
		{
			return Utils.DiceCoefficient.Calculate(input, compareTo);
		}
	}
}
