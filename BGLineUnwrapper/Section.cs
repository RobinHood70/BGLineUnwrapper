namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text.RegularExpressions;

	public class Section
	{
		#region Constants
		public const string Divider = "-------------------------------------------------------------------------------";
		public const string EntryDelimiter = "\n  - ";
		#endregion

		#region Fields
		private static readonly Regex SubsectionFinder = new Regex(@"\n+(?<subsection>[^ \n][^:\n]*?):", RegexOptions.Singleline);
		private static readonly Regex PlainTextFinder = new Regex(@"(?<title>[^\n]*)\n-+\n", RegexOptions.Singleline);
		private static readonly Regex OldLocFinder = new Regex(@"(\s+at)?\s*(\(x +(?<x>\d+),? +y +(?<y>\d+)\)|x +(?<x>\d+),? +y +(?<y>\d+))");
		private static readonly Regex AreaFinder = new Regex(@"\((?<area>([A-Z]{2}\d{4},? ?)+)\)");
		#endregion

		#region Constructors
		public Section(string sectionText)
		{
			sectionText = OldLocFinder.Replace(sectionText, "[${x}.${y}]").Replace(", [", " [");
			var split = sectionText.Split(new[] { Divider }, StringSplitOptions.None);
			if (split.Length != 2)
			{
				throw new InvalidOperationException("Invalid section text!");
			}

			var titleText = split[0].TrimEnd();
			var bodyText = split[1];
			var sectionNum = titleText.IndexOf(". ");
			if (sectionNum == -1)
			{
				this.Title = titleText.Trim();
				this.PlainText = ParsePlainText(bodyText);

				return;
			}

			titleText = titleText.Substring(sectionNum + 2).Trim();
			titleText = titleText.Substring(0, titleText.LastIndexOf('[') - 1).TrimEnd();

			var areaMatch = AreaFinder.Match(titleText);
			if (areaMatch.Success)
			{
				this.Area = areaMatch.Groups["area"].Value;
				this.Title = titleText.Substring(0, areaMatch.Index - 1).TrimEnd();
			}
			else
			{
				this.Title = titleText;
			}

			var matches = SubsectionFinder.Split(bodyText);
			if (matches[0].Length > 0)
			{
				throw new InvalidOperationException("Invalid section format!");
			}

			var titleTrimmed = this.Title.Split(new[] { " (" }, StringSplitOptions.None)[0];
			for (var i = 1; i < matches.Length; i += 2)
			{
				var subsectionTitle = matches[i];
				var subbodyText = matches[i + 1];
				switch (subsectionTitle)
				{
					case "Assassination Attempt":
					case "Assassination Attempts":
					case "Enemy Wizards":
						this.Assassinations = new List<Subsection>(ParseTitledSubsection(subbodyText, titleTrimmed));
						break;
					case "Companions":
						this.ParseCompanions(subbodyText);
						break;
					case "Enemies":
						this.Enemies = ParseEnemies(subbodyText);
						break;
					case "Note":
						this.Notes = new List<Line>(WrappedLines(TrimStart(subbodyText), LineType.Note));
						break;
					case "Other":
						this.Other = new List<Subsection>(ParseTitledSubsection(subbodyText, titleTrimmed));
						break;
					case "Plot":
						this.Plot = ParsePlot(subbodyText, titleTrimmed);
						break;
					case "Subquests":
						this.Subquests = new List<Subsection>(ParseTitledSubsection(subbodyText, titleTrimmed));
						break;
					case "Treasures":
						this.ParseTreasures(subbodyText);
						break;
					default:
						throw new InvalidOperationException("Unrecognized subsection title: " + subsectionTitle);
				}
			}
		}
		#endregion

		#region Public Properties
		public string Area { get; }

		public IList<Subsection> Assassinations { get; } = new List<Subsection>();

		public IList<Companion> Companions { get; } = new List<Companion>();

		public IList<string> Enemies { get; }

		public IList<Line> Notes { get; }

		public IList<Subsection> Other { get; }

		public IList<Line> PlainText { get; }

		public IList<Line> Plot { get; }

		public IList<Subsection> Subquests { get; }

		public string Title { get; }

		public IDictionary<string, ICollection<string>> Treasures { get; } = new SortedDictionary<string, ICollection<string>>();
		#endregion

		#region Public Override Methods
		public override string ToString() => this.Title;
		#endregion

		#region Private Static Methods
		private static IEnumerable<Line> GetLineTypes(List<string> lines, LineType preferredType)
		{
			if (lines.Count == 0)
			{
				yield break;
			}

			var endLines = new Stack<Line>();
			var singleLines = lines.Count - 1;
			LineType lineType;
			do
			{
				lineType = preferredType;
				var text = lines[singleLines];
				var specialOffset = text.IndexOf(':');
				if (specialOffset >= 0 && specialOffset < (text.Length - 1) && (text[specialOffset + 1] == ' ' || text[specialOffset + 1] == '['))
				{
					lineType = LineType.Colon;
				}
				else
				{
					specialOffset = text.IndexOf("--", StringComparison.Ordinal);
					if (specialOffset >= 0)
					{
						lineType = LineType.Dashed;
					}
				}

				if (lineType != preferredType)
				{
					var prefix = text.Substring(0, specialOffset);
					if (text.IndexOf('[', specialOffset) == -1)
					{
						var wordCount = prefix.Split(' ').Length;
						if (wordCount > 4 || char.IsLower(text[0]) || prefix.Contains("."))
						{
							// Assume it's part of previous text if it starts with a lower-case letter or it's exceptionally long.
							break;
						}
					}

					singleLines--;
					text = text.Substring(prefix.Length + 1).TrimStart();
					endLines.Push(new Line(lineType, prefix, text));
				}
			}
			while (singleLines >= 0 && lineType != preferredType);

			if (singleLines != -1)
			{
				var text = string.Join(" ", lines.GetRange(0, singleLines + 1));
				yield return new Line(preferredType, text);
			}

			while (endLines.Count > 0)
			{
				yield return endLines.Pop();
			}
		}

		private static IList<string> ParseEnemies(string subsectionText)
		{
			var lines = new List<Line>(WrappedLines(TrimStart(subsectionText), LineType.Plain));
			if (lines.Count != 1)
			{
				throw new InvalidOperationException("Malformed Enemies section!");
			}

			var retval = new List<string>(lines[0].Text.Split(new[] { ", " }, StringSplitOptions.None));
			if (retval.Count > 0)
			{
				retval.Sort();
			}

			return retval;
		}

		private static IList<Line> ParsePlot(string subsectionText, string areaName)
		{
			var retval = new List<Line>();
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			foreach (var entry in entries)
			{
				var trimmedLines = TrimStart(entry);
				retval.AddRange(WrappedLines(trimmedLines, LineType.Plain));
			}

			foreach (var line in retval)
			{
				line.TrimAreaName(areaName);
			}

			return retval;
		}

		private static IList<Line> ParsePlainText(string subsectionText)
		{
			subsectionText = subsectionText.Trim();
			var textSections = PlainTextFinder.Split(subsectionText);
			if (textSections.Length == 0 || (textSections.Length == 1 && textSections[0].Length == 0))
			{
				return null;
			}

			// This slightly odd loop construct handles both untitled text as well as titled within the same loop.
			var retval = new List<Line>();
			for (var i = -1; i < textSections.Length; i += 2)
			{
				if (i > -1)
				{
					retval.Add(new Line(LineType.Title, textSections[i]));
				}

				if (textSections[i + 1].Length > 0)
				{
					foreach (var line in textSections[i + 1].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
					{
						retval.Add(new Line(LineType.Plain, line));
					}
				}
			}

			return retval;
		}

		private static IEnumerable<Subsection> ParseTitledSubsection(string subsectionText, string areaName)
		{
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			foreach (var entry in entries)
			{
				var trimmedLines = TrimStart(entry);
				if (trimmedLines.Count > 1 || trimmedLines[0].Length > 0)
				{
					var wrapped = WrappedLines(trimmedLines, LineType.Title);
					var subsection = new Subsection(wrapped);
					subsection.TrimAreaName(areaName);
					subsection.ReparseLocations();
					yield return subsection;
				}
				else if (trimmedLines.Count > 0 && trimmedLines[0].Length > 0)
				{
					Debug.WriteLine("WTF?");
				}
			}
		}

		private static List<string> TrimStart(string text)
		{
			var retval = new List<string>();
			foreach (var line in text.Split('\n'))
			{
				retval.Add(line.Trim());
			}

			return retval;
		}

		private static IEnumerable<Line> WrappedLines(IEnumerable<string> lines, LineType lineType)
		{
			var newLines = new List<string>(lines);
			if (newLines[^1].Length > 0)
			{
				newLines.Add(string.Empty); // Avoids having to do post-loop repetition of the line.Length == 0 block.
			}

			var i = 0;
			if (newLines[i].Length > 0 && lineType == LineType.Title)
			{
				yield return new Line(lineType, newLines[i]);
				lineType = LineType.Plain;
				i++;
			}

			var index = i;
			var count = 0;
			for (; i < newLines.Count; i++)
			{
				var line = newLines[i];
				if (line.Length == 0)
				{
					if (count > 0)
					{
						var newerLines = newLines.GetRange(index, count);
						foreach (var newLine in GetLineTypes(newerLines, lineType))
						{
							yield return newLine;
						}
					}

					index = i + 1;
					count = 0;
				}
				else
				{
					count++;
				}
			}
		}
		#endregion

		#region Private Methods
		private void ParseCompanions(string subsectionText)
		{
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			if (entries.Length > 1)
			{
				for (var i = 1; i < entries.Length; i++)
				{
					var entryText = entries[i].TrimEnd();
					var companionLines = TrimStart(entryText);
					this.Companions.Add(new Companion(companionLines));
				}
			}
		}

		private void ParseTreasures(string subsectionText)
		{
			var lines = TrimStart(subsectionText);
			foreach (var line in lines)
			{
				var split = line.Split(new[] { " (" }, 2, StringSplitOptions.None);
				var item = split[0].TrimEnd();
				var quest = split.Length == 2 ? split[1].TrimEnd(')') : string.Empty;
				if (quest == item)
				{
					quest = string.Empty;
				}

				var locSplit = item.Split(new[] { '[' }, 2);
				if (locSplit.Length == 2)
				{
					item = locSplit[0];
					quest += '[' + locSplit[1];
				}

				if (!this.Treasures.TryGetValue(quest, out var treasureList))
				{
					treasureList = new SortedSet<string>();
					this.Treasures.Add(quest, treasureList);
				}

				treasureList.Add(Common.HarmonizeText(item));
			}
		}
		#endregion
	}
}