using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace D3Bit.Tests
{
	[DebuggerDisplay("{Name} ({Quality,nq} {Type,nq})")]
	public class TooltipData
	{
		public string Name { get; set; }
		public string Meta { get; set; }
		public string Type { get; set; }
		public string Quality { get; set; }
		public string SocketBonuses { get; set; }
		public double DPS { get; set; }
		public Dictionary<string, string> Affixes { get; set; }

		public long TimeTaken { get; set; }

		public TooltipData()
		{

		}

		public TooltipData(ITooltip tooltip)
		{
			Stopwatch timer = new Stopwatch();
			string sockets = string.Empty, quality = "Unknown";

			timer.Start();
			DPS = tooltip.ParseDPS();
			Name = tooltip.ParseItemName();
			Type = tooltip.ParseItemType(out quality);
			Meta = tooltip.ParseMeta();
			Affixes = tooltip.ParseAffixes(out sockets);

			Quality = quality;
			SocketBonuses = sockets;
			timer.Stop();
			TimeTaken = timer.ElapsedMilliseconds;
		}

		public override string ToString()
		{
			return string.Format("[{0}]", Name);
		}

		//public override int GetHashCode()
		//{
		//    var sb = new StringBuilder();
		//    sb.AppendLine(Name);
		//    sb.AppendLine(Type);
		//    sb.AppendLine(Quality);
		//    sb.AppendLine(DPS.ToString());
		//    sb.AppendLine(Meta);
		//    if (Affixes != null)
		//    {
		//        sb.AppendLine(Affixes.Aggregate(new StringBuilder(), (sb2, kvp) => sb2.AppendLine("{0}={1}", kvp.Key, kvp.Value)).ToString());
		//    }
		//    sb.AppendLine(SocketBonuses);

		//    return sb.ToString().GetHashCode();
		//}
	}
}
