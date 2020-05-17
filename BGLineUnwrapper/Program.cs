namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using LineUnwrapper;

	internal static class Program
	{
		private static IEnumerable<Section> ConvertBG1(string text)
		{
			var newText = new List<Section>();
			text = HarmonizeText(text);
			var sectionTexts = text.Split(new[] { "\n\n\n" + Section.Divider + "\n" }, StringSplitOptions.None);
			if (sectionTexts.Length == 0)
			{
				throw new InvalidOperationException("No sections!");
			}

			foreach (var sectionText in sectionTexts)
			{
				var section = new Section(sectionText);
				newText.Add(section);
			}

			return newText;
		}

		private static string HarmonizeText(string text)
		{
			var lines = new List<string>(text.Split('\n'));
			if (lines[0][0] == '-')
			{
				lines.RemoveAt(0);
			}

			for (var lineNum = 0; lineNum < lines.Count; lineNum++)
			{
				var line = lines[lineNum];
				if (line.Length > 0)
				{
					var charNum = 0;
					while (charNum < line.Length && line[charNum] == '\t')
					{
						charNum++;
					}

					lines[lineNum] = new string(' ', 4 * charNum) + line.Substring(charNum).TrimEnd();
					if (lines[lineNum].Contains("\t"))
					{
						Debug.WriteLine("Unexpected tab on line " + lineNum.ToString() + ": " + lines[lineNum]);
					}
				}
			}

			return string.Join("\n", lines);
		}

		public static void Main(string[] args)
		{
			var filename = args.Length > 0 ? args[0] : @"D:\Users\rmorl\Documents\Games\Baldur's Gate\BG Walkthrough.txt";
			var option = args.Length > 1 ? args[1] : "BG1";
			var text = File.ReadAllText(filename, Encoding.UTF8); // Encoding.GetEncoding(1252)
			IEnumerable<Section> parsedFile = null;
			if (option == "BG1")
			{
				parsedFile = ConvertBG1(text);
			}

			var doWord = true;
			using var writer = File.CreateText(@"D:\Output." + (doWord ? "xml" : "htm"));
			var saver = doWord ? (SaveAs)new SaveAsWordML(writer) : new SaveAsHtml(writer);
			saver.Save(parsedFile);
		}
	}
}
