﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NHunspell;

namespace D3Bit
{
    public static class Tesseract
    {
        private static Lazy<Hunspell> hunspellEngine = new Lazy<Hunspell>(() => new Hunspell("en_us.aff", "en_us.dic"));
        public static string language_code = "eng";

        public static string GetTextFromBitmap(Bitmap bitmap)
        {
			//return GetTextFromBitmap(bitmap, @"nobatch tesseract\d3letters");
			return GetTextFromBitmap(bitmap, @"tesseract\d3letters");
        }

        public static string GetTextFromBitmap(Bitmap bitmap, string extraParams)
        {
            //StopWatch sw = new StopWatch();
            bitmap.Save(@"tesseract\x.png", ImageFormat.Png);
            //sw.Lap("File Save");
            ProcessStartInfo info = new ProcessStartInfo(@"tesseract\tesseract.exe", string.Format(@"tesseract\x.png tesseract\x -l {0} {1}", language_code, extraParams));
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(info);
            p.WaitForExit();
            //sw.Lap("Tesseract");

			using (var tr = new StreamReader(@"tesseract\x.txt"))
			{
				return tr.ReadToEnd().Trim();
			}
        }

        public static string CorrectSpelling(string text)
        {
            Hunspell hunspell = hunspellEngine.Value;
            string[] words = text.Split(new[] { ' ' });
            string res = "";

            foreach (var word in words)
            {
                var suggestions = hunspell.Suggest(word);
                if (suggestions.Count > 0 && !hunspell.Spell(word))
                    res += suggestions[0] + " ";
                else
                    res += word + " ";
            }

            return res.Trim();
        }

    }
}
