using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace D3Bit
{
	using AForge;
	using AForge.Imaging.Filters;
	using D3Bit.Utils;
	using Extensions;

	public partial class Tooltip : ITooltip
	{
		#region [ Fields & Properties ]
		private Bitmap Original { get; set; }
		public Bitmap Processed { get; private set; }

		private Rectangle dpsBound;
		private Rectangle nameBound;
		private Rectangle typeBound;
		private Rectangle affixesBound;

		private static Regex reDmgNumbers = new Regex(@"\d+(\.\d+)?(-\d+)?");

#if DEBUG
		public List<Bitmap> DebugBitmaps { get; private set; }
#endif
		#endregion

		public Tooltip(Bitmap bitmap)
		{
			Original = bitmap;
			Processed = (Bitmap)Original.Clone();   //Modifiable, used for improvement and drawing

#if DEBUG
			DebugBitmaps = new List<Bitmap>();
#endif
		}

		/// <summary>
		/// calculates horizontal scale based on given percent
		/// </summary>
		private int h(double percent)
		{
			return (int)Math.Round(percent / 100.0 * Original.Width);
		}

		[Conditional("DEBUG")]
		private void AddDebugBitmap(Bitmap bmp)
		{
#if DEBUG
			DebugBitmaps.Add(bmp);
#endif
		}

		public string ParseItemName()
		{
			Func<Color, bool> func = c => c.GetBrightness() > 0.9;
			Func<Color, bool> func2 = c => ImageUtil.GetGrayValue(c) > 180;

			string itemName = "Unknown";

			// get name's position
			var bound = Rectangle.FromLTRB(h(5), h(4), h(95), h(13.5));
			nameBound = ImageUtil.GetTextBounding(Original, bound, func2);
			nameBound.Inflate(4, 6);

			// draw debug info
			ImageUtil.DrawBlockBounding(Processed, bound, Color.Red);
			ImageUtil.DrawBlockBounding(Processed, nameBound, Color.Lime);

			if (nameBound.Width < 20)
			{
				return itemName;
			}

			// TODO: play a bit with threshold or levels, it should remove the background and improve the results
			var nameBitmap = ExtractNameTextBitmapForOCR(nameBound);
			AddDebugBitmap(nameBitmap);			

			itemName = Tesseract.GetTextFromBitmap(nameBitmap).Trim().Replace("Q", "O").Replace("GB", "O").Replace("G3", "O").Replace("EB", "O").Replace("G9", "O").Replace("69", "O").Replace("l", "I");
			itemName = Regex.Replace(itemName, @"([AEIOUM]|^|\b)ITI", "$1M");
			itemName = Tesseract.CorrectSpelling(itemName);

			return itemName;
		}

		public string ParseItemType(out string quality)
		{
			string itemType = "Unknown";
			quality = "Unknown";
			
			Func<Color, bool> colorFunc = c => ImageUtil.GetGrayValue(c) > 130 && !(Math.Abs(c.R - c.G) < 30 && Math.Abs(c.G - c.B) < 30);

			var bound = Rectangle.FromLTRB(h(22), h(14), h(70), h(21));

			// use nameBound to get top bound more accuratrely (otherwise it will break on 2-line item names)
			if (!nameBound.IsEmpty)
			{
				bound.Y = nameBound.Bottom + h(5);
			}

			if (!dpsBound.IsEmpty)
			{
				bound.Height = dpsBound.Top - bound.Top - h(4);
			}

			ImageUtil.DrawBlockBounding(Processed, bound, Color.Red);

			typeBound = ImageUtil.GetTextBounding(Original, bound, colorFunc);
			typeBound.Inflate(4, 4);
			ImageUtil.DrawBlockBounding(Processed, typeBound, Color.Lime);

			var typeBlock = ExtractTypeTextBitmapForOCR(typeBound);
			AddDebugBitmap(typeBlock);

			string text = Tesseract.GetTextFromBitmap(typeBlock).Replace("\n", " ");
			
			// leave only letters and hyphens
			text = Regex.Replace(text, "[^a-z-]", " ", RegexOptions.IgnoreCase).Trim();

			var words = text.Split(new[] { ' ' });
			if (words.Length > 1)
			{
				string qualityString = words[0];
				quality = Data.ItemQualities.OrderByDescending(i => qualityString.DiceCoefficient(i)).FirstOrDefault();
				itemType = string.Join(" ", words.Skip(1));
				itemType = Data.ItemTypes.OrderByDescending(i => itemType.DiceCoefficient(i)).FirstOrDefault();
				return itemType;
			}

			return itemType;
		}

		/// <summary>
		/// Parses meta information regarding weapon damage and attack speed. This method must be called after successful call to ParseDPS()
		/// </summary>
		/// <returns>comma separated list of meta information</returns>
		public string ParseMeta()
		{
			Func<Color, bool> whiteFunc = c => c.B > 150 && Math.Abs(c.R - c.G) < 8 && Math.Abs(c.R - c.B) < 8;

			if (dpsBound.IsEmpty) // if there is no DPS, there is no meta information as well... no need to check for it (Warning: DPS must be parsed before meta)
			{
				return string.Empty;
			}

			var bound = Rectangle.FromLTRB(dpsBound.Left - h(1), dpsBound.Bottom + h(7), dpsBound.Left + h(20), dpsBound.Bottom + h(17));
			ImageUtil.DrawBlockBounding(Processed, bound, Color.Red);			

			var bounds = GetAffixesBounds(bound, whiteFunc, bound.Left, bound.Left + h(5));
			if (bounds.Count != 2)
			{
				return string.Empty;
			}
						
			var bitmaps = GetTextBlocks(bounds, b => ExtractMetaTextBitmapForOCR(b));
			var mergedMeta = MergeBitmaps(bitmaps, 50);
			AddDebugBitmap((Bitmap)mergedMeta.Clone());

			string text = Tesseract.GetTextFromBitmap(mergedMeta, @"nobatch tesseract\d3meta");
			mergedMeta.Dispose();

			return text.Replace(" ", "").Replace("+", "").Replace("\n", ",");
		}

		public double ParseDPS()
		{
			double dps = 0;
			Func<Color, bool> colorFunc = c => c.R == 255 && c.G == 255 && c.B == 255; // ImageUtil.GetGrayValue(c) > 240 && (Math.Abs(c.R - c.G) < 5 && Math.Abs(c.G - c.B) < 5);

			var bound = Rectangle.FromLTRB(h(22), h(22), h(60), h(40));
			dpsBound = ImageUtil.GetTextBounding(Original, bound, colorFunc);
			
			if (dpsBound == bound) // nothing found
			{
				dpsBound = Rectangle.Empty; // used for improved positioning of Type & Quality, so need to clear it if there is no DPS
				return dps;
			}

			// 2nd pass to properly get dps when an item has 2-line caption
			dpsBound.Height += h(2);
			dpsBound = ImageUtil.GetTextBounding(Original, dpsBound, colorFunc);
			dpsBound.Inflate(4, 4);

			ImageUtil.DrawBlockBounding(Processed, bound, Color.Red);
			ImageUtil.DrawBlockBounding(Processed, dpsBound, Color.Lime);

			var dpsBlock = ExtractDpsTextBitmapForOCR(dpsBound);
			AddDebugBitmap(dpsBlock); 

			var text = Tesseract.GetTextFromBitmap(dpsBlock, @"-psm 7 nobatch tesseract\d3digits");
			double.TryParse(text, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dps);

			return dps;
		}

		public Dictionary<string, string> ParseAffixes(out string socketBonuses)
		{
			Func<Color, bool> whiteFunc = c => c.GetBrightness() > 0.6 && c.GetHue() == 0;
			Func<Color, bool> colorFunc = c => c.GetBrightness() > 0.3 && c.GetHue() == 240;

			int xScanStart = h(9), xScanEnd = h(20);

			var affixesY = GetAffixesY(colorFunc, xScanEnd, xScanEnd + h(5));
			affixesBound = Rectangle.FromLTRB(h(5), affixesY, h(93), Original.Height - h(17));

			ImageUtil.DrawVLine(Processed, h(5), Color.Lime);

			var bounds = GetAffixesBounds(affixesBound, c => whiteFunc(c) || colorFunc(c), xScanStart, xScanEnd);

			var bitmaps = GetTextBlocks(bounds, bound => ExtractAffixTextBitmapForOCR(bound, 50));
			var mergedAffixes = MergeBitmaps(bitmaps, 50);
			AddDebugBitmap((Bitmap)mergedAffixes.Clone());

			string text = Tesseract.GetTextFromBitmap(mergedAffixes);
			mergedAffixes.Dispose();
			
			var lines = text.Replace("1 1", "11").Split('\n');
			var socketRange = Enumerable.Range(xScanEnd, 20);
			var affixStrings = GetNormalizedAffixes(out socketBonuses, whiteFunc, bounds, lines, socketRange);

			var affixes = new Dictionary<string, string>();
			int emptySockets = 0;

			foreach (var affix in affixStrings)
			{
				var pair = Data.affixMatches.Select(p => new { p.Key, Coeff = affix.DiceCoefficient(p.Value) }).OrderByDescending(o => o.Coeff).First();
				if (pair.Coeff < 0.20)
				{
					continue;
				}

				if (pair.Key == "Soc")
				{
					emptySockets++;
					continue;
				}

				List<string> values = new List<string>();

				var m = reDmgNumbers.Match(affix);
				if (m.Success)
				{
					if (pair.Key == "Dmg")
					{
						var parts = m.Value.Split('-');
						affixes.Add("MinD", parts[0]);
						affixes.Add("MaxD", parts[1]);
						continue;
					}

					values.Add(m.Value); // elemental dmg
				}

				if (values.Count > 0 && !affixes.ContainsKey(pair.Key))
				{
					affixes.Add(pair.Key, String.Join("*", values));
				}
			}

			if (!string.IsNullOrEmpty(socketBonuses) || emptySockets > 0)
			{
				var sockets = socketBonuses.Count(c => c == ',');
				if (sockets > 0)
				{
					sockets++;
				}
				affixes.Add("Soc", (sockets + emptySockets).ToString());
			}

			return affixes;
		}

		#region [ Affix helpers ]
		/// <summary>
		/// Make some cleanup, join multiline affixes, parse socket bonuses, etc.
		/// </summary>
		private List<string> GetNormalizedAffixes(out string socketBonuses, Func<Color, bool> whiteFunc, List<Rectangle> bounds, string[] lines, IEnumerable<int> socketRange)
		{
			var affixesStrings = new List<string>();
			var sockets = new List<string>();

			using (var locked = Original.Lock())
			{
				for (int i = 0; i < lines.Length; i++)
				{
					var line = lines[i];
					var bound = bounds[i];
					var y = bound.Top + bound.Height / 2;

					if (socketRange.Any(x => whiteFunc(locked.GetPixel(x, y)))) // socket affix found
					{
						affixesStrings.Add("Empty Socket");

						var pair = Data.affixMatches.Select(p => new { p.Key, Coeff = line.DiceCoefficient(p.Value) }).OrderByDescending(o => o.Coeff).First();
						if (pair.Coeff > 0.20)
						{
							var m = reDmgNumbers.Match(line);
							if (m.Success)
							{
								sockets.Add(m.Value + " " + pair.Key);
							}
						}
					}
					else if (bound.Left < h(5)) // multiline affix found
					{
						affixesStrings[affixesStrings.Count - 1] += " " + line;
					}
					else
					{
						affixesStrings.Add(line);
					}
				}
			}

			socketBonuses = string.Join(", ", sockets);

			return affixesStrings;
		}

		private List<Bitmap> GetTextBlocks(List<Rectangle> bounds, Func<Rectangle, Bitmap> extractFunction)
		{
			var bitmaps = new List<Bitmap>();

			foreach (var bound in bounds)
			{
				if (bound.Width < 20)
				{
					continue;
				}

				ImageUtil.DrawBlockBounding(Processed, bound, Color.Pink);

				var affixBitmap = extractFunction(bound);
				bitmaps.Add(affixBitmap);
			}
			return bitmaps;
		}

		/// <summary>
		/// Merges multiple bitmaps vertically into one big bitmap
		/// </summary>
		private Bitmap MergeBitmaps(List<Bitmap> bitmaps, int margin)
		{
			var width = bitmaps.Max(b => b.Width);
			var height = bitmaps.Sum(b => b.Height)
						+ (bitmaps.Count - 1) * margin; // all the space between images

			var mergedAffixes = new Bitmap(width, height);

			using (var g = Graphics.FromImage(mergedAffixes))
			{
				//g.Clear(Color.Black);
				int y = 0;
				foreach (var bmp in bitmaps)
				{
					g.DrawImageUnscaled(bmp, 0, y);
					y += bmp.Height + margin;
				}
			}
			return mergedAffixes;
		}

		private void FixLifeAffix(Dictionary<string, string> affixes)
		{
			var lifeKey = "Life%";
			if (affixes.ContainsKey(lifeKey))
			{
				int n;
				if (int.TryParse(affixes[lifeKey], out n))
				{
					if (n > 20)
					{
						n = int.Parse(n.ToString().Substring(0, 1));
					}
					affixes[lifeKey] = n.ToString();
				}
			}
		}

		private List<Rectangle> GetAffixesBounds(Rectangle affixesBound, Func<Color, bool> colorFunc, int xStart, int xEnd)
		{
			var list = new List<Rectangle>();
			var maxLineHeight = 0;
			int yTextStart = 0, yTextEnd = 0;
			var range = Enumerable.Range(xStart, xEnd - xStart);

			using (var locked = Original.Lock())
			{
				for (int y = affixesBound.Top; y < affixesBound.Bottom; y++)
				{
					var isText = range.Any(x => colorFunc(locked.GetPixel(x, y))); // affix text or socketed gem text in white

					if (isText)
					{
						if (yTextStart == 0)
						{
							yTextStart = y; // mark the beginning
						}
						else
						{
							yTextEnd = y; // mark the end
						}
					}
					else // black space between lines or something different (like red text info about class specific item - as long as it's not an affix it doesn't matter)
					{
						if (yTextEnd != 0) // found a line of text
						{
							var lineHeight = yTextEnd - yTextStart;

							var bound = Rectangle.FromLTRB(affixesBound.Left, yTextStart - lineHeight / 4, affixesBound.Right, yTextEnd + lineHeight / 4);
							bound = ImageUtil.GetTextBounding(locked, bound, colorFunc);
							bound.Inflate(3, 2);
							list.Add(bound);

							maxLineHeight = Math.Max(maxLineHeight, lineHeight);
							yTextStart = yTextEnd = 0;
						}
						else if (list.Count > 0) // if we are too far from last affix, we need to break, because we could get "Stat Changes" text by accident
						{
							var distance = y - list.Last().Bottom;
							if (distance > maxLineHeight * 4)
							{
								break;
							}
						}
					}
				}
			}

			return list;
		}

		private int GetAffixesY(Func<Color, bool> colorFunc, int xStart, int xEnd)
		{
			var _affixStartY = h(30);
			ImageUtil.DrawHLine(Processed, _affixStartY, Color.Red);

			using (var locked = Original.Lock())
			{
				var range = Enumerable.Range(xStart, xEnd - xStart);

				for (int y = _affixStartY; y < _affixStartY + h(23); y++)
				{
					if (range.Any(x => colorFunc(locked.GetPixel(x, y))))
					{
						var X = range.FirstOrDefault(x => colorFunc(locked.GetPixel(x, y)));
						ImageUtil.DrawHLine(Processed, _affixStartY, Color.Lime);
						return y;
					}
				}
			}

			ImageUtil.DrawHLine(Processed, _affixStartY, Color.Lime);

			return _affixStartY;
		} 
		#endregion

		#region [ OCR accuracy helpers ]
		private Bitmap ExtractMetaTextBitmapForOCR(Rectangle bound)
		{
			var textBlock = Original.Clone(bound, Original.PixelFormat);

			// TODO: need to process image with Levels first and then probably Threshold
			textBlock = ImageUtil.ResizeImage(textBlock, (int)(bound.Height * 4f / textBlock.Height * textBlock.Width), bound.Height * 4);

			using (new PerformanceLog("Meta filters:"))
			{
				textBlock = ImageUtil.AdjustImage(textBlock, contrast: 1.5f);
				ClearElementalDamageBackground(textBlock);
			}
			return textBlock;
		}

		private Bitmap ExtractTypeTextBitmapForOCR(Rectangle bound)
		{
			var textBlock = Original.Clone(bound, Original.PixelFormat);

			textBlock = ImageUtil.ResizeImage(textBlock, (int)(bound.Height * 2f / textBlock.Height * textBlock.Width), bound.Height * 2);
			ImageUtil.ApplyThreshold(140, textBlock);
			
			// these makes OCR more accurate
			//textBlock = ImageUtil.AdjustImage(textBlock, contrast: 1.5f);
			//ImageUtil.ApplyContrast(50, textBlock);
			//textBlock = ImageUtil.MakeGrayscale(textBlock);
			
			return textBlock;
		}

		private Bitmap ExtractDpsTextBitmapForOCR(Rectangle bound, int normalizedHeight = 90)
		{
			var textBlock = Original.Clone(bound, Original.PixelFormat);

			textBlock = ImageUtil.AdjustImage(textBlock, contrast: 1.5f);
			ClearElementalDamageBackground(textBlock);

			//ImageUtil.ApplyThreshold(200, textBlock);

			// no need for resize for 1920x1200... maybe it will be necessary for lower res
			//textBlock = ImageUtil.ResizeImage(textBlock, (int)(normalizedHeight * 1f / textBlock.Height * textBlock.Width), normalizedHeight);
			return textBlock;
		}
		
		private Bitmap ExtractNameTextBitmapForOCR(Rectangle bound, int normalizedHeight = 90)
		{
			var textBlock = Original.Clone(bound, Original.PixelFormat);
			textBlock = ImageUtil.ResizeImage(textBlock, (int)(bound.Height * 2f / textBlock.Height * textBlock.Width), bound.Height * 2);

			using (new PerformanceLog("Name filters:"))
			{
				var g = new Grayscale(0.5, 0.5, 0.5);
				textBlock = g.Apply(textBlock);
				var t = new Threshold(75);
				t.ApplyInPlace(textBlock);
			}

			return textBlock;
		}

		private Bitmap ExtractAffixTextBitmapForOCR(Rectangle bound, int normalizedHeight = 90)
		{
			var textBlock = Original.Clone(bound, Original.PixelFormat);
			textBlock = ImageUtil.ResizeImage(textBlock, (int)(normalizedHeight * 1f / textBlock.Height * textBlock.Width), normalizedHeight);
			textBlock = ImageUtil.AdjustImage(textBlock, contrast: 2);

			// these makes OCR more accurate
			textBlock = ImageUtil.MakeGrayscale(textBlock);
			return textBlock;
		}

		/// <summary>
		/// Removes background from elemental damage
		/// </summary>
		private static void ClearElementalDamageBackground(Bitmap textBlock)
		{
			// remove any non-neutral colors (especially reddish background from fire damage) - this is extremely fast, 0-1ms!
			ColorFiltering filter = new ColorFiltering();
			filter.Red = new IntRange(100, 255);
			filter.Green = new IntRange(100, 255);
			filter.Blue = new IntRange(100, 255);
			filter.ApplyInPlace(textBlock);
		}
		#endregion
	}
}
