using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tests
{
	public interface IResultProvider
	{
		string ParseItemName();

		string ParseItemType(out string quality);

		string ParseMeta();

		double ParseDPS();

		Dictionary<string, string> ParseAffixes(out string socketBonuses);
	}
}
