using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3Bit;

namespace Tests
{
	public class TooltipWrapper<T> : ITooltip
		where T : ITooltip
	{
		#region [ Fields & Properties ]
		private T wrappedTooltip;

		// information about the resolution the screenshot with this tooltip was taken on, will be used to generate a better report in future
		public int ScreenWidth { get; private set; }
		public int ScreenHeight { get; private set; }

		public T WrappedTooltip
		{
			get
			{
				return wrappedTooltip;
			}
		} 
		#endregion

		public TooltipWrapper(T tooltip, int screenWidth, int screenHeight)
		{
			wrappedTooltip = tooltip;

			ScreenHeight = screenHeight;
			ScreenWidth = screenWidth;
		}

		#region [ ITooltip methods ]
		public string ParseItemName()
		{
			return wrappedTooltip.ParseItemName();
		}

		public string ParseItemType(out string quality)
		{
			return wrappedTooltip.ParseItemType(out quality);
		}

		public string ParseMeta()
		{
			return wrappedTooltip.ParseMeta();
		}

		public double ParseDPS()
		{
			return wrappedTooltip.ParseDPS();
		}

		public Dictionary<string, string> ParseAffixes(out string socketBonuses)
		{
			return wrappedTooltip.ParseAffixes(out socketBonuses);
		} 
		#endregion
	}
}
