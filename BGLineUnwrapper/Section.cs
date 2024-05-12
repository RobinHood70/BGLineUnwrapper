namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	public class Section
	{
		#region Constants
		public const string Divider = "-------------------------------------------------------------------------------";
		public const string EntryDelimiter = "\n  - ";
		#endregion

		#region Static Fields
		private static readonly char[] Colon = new char[] { ':' };
		private static readonly Regex SubsectionFinder = new Regex(@"\n+(?<subsection>[^ \n][^:\n]*?):", RegexOptions.Singleline);
		private static readonly Regex PlainTextFinder = new Regex(@"(?<title>[^\n]*)\n-+\n", RegexOptions.Singleline);
		private static readonly Regex OldLocFinder = new Regex(@",?(\s+at)?\s*(\(x +(?<x>\d+),? +y +(?<y>\d+)\)|x +(?<x>\d+),? +y +(?<y>\d+))(?<punc>[\p{P}]*)");
		private static readonly Regex AreaFinder = new Regex(@"\((?<area>([A-Z]{2}\d{4},? ?)+)\)");
		private static readonly (string Search, int Offset, LineType LineType)[] SearchStrings = new[]
		{
			(":[", 1, LineType.Colon),
			(": ", 2, LineType.Colon),
			("--", 2, LineType.Dashed)
		};
		#endregion

		#region Constructors
		public Section(string sectionText)
		{
			sectionText = OldLocFinder.Replace(sectionText, "${punc}[${x}.${y}]");
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
				this.PlainText = new List<Subsection>(ParsePlainText(bodyText));

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
						if (this.Assassinations != null)
						{
							throw new InvalidOperationException("Duplicate entry.");
						}

						this.Assassinations = new List<Subsection>(ParseSubsection(subbodyText, titleTrimmed, LineType.Title));
						break;
					case "Companions":
						if (this.Companions != null)
						{
							throw new InvalidOperationException("Duplicate entry.");
						}

						this.Companions = new List<Companion>(ParseCompanions(subbodyText));
						break;
					case "Enemies":
						if (this.Enemies != null)
						{
							throw new InvalidOperationException("Duplicate entry.");
						}

						var enemies = new List<string>(ParseEnemies(subbodyText));
						enemies.Sort();
						this.Enemies = enemies;
						break;
					case "Note":
						if (this.Notes != null)
						{
							throw new InvalidOperationException("Duplicate entry.");
						}

						this.Notes = new Subsection(WrappedLines(TrimStart(subbodyText), LineType.Note), false);
						break;
					case "Other":
						if (this.Other != null)
						{
							throw new InvalidOperationException("Duplicate entry.");
						}

						this.Other = new List<Subsection>(ParseSubsection(subbodyText, titleTrimmed, LineType.Title));
						break;
					case "Plot":
						if (this.Plot != null)
						{
							throw new InvalidOperationException("Duplicate entry.");
						}

						this.Plot = new List<Subsection>(ParseSubsection(subbodyText, titleTrimmed, LineType.Plain));
						break;
					case "Subquests":
						if (this.Subquests != null)
						{
							throw new InvalidOperationException("Duplicate entry.");
						}

						this.Subquests = new List<Subsection>(ParseSubsection(subbodyText, titleTrimmed, LineType.Title));
						break;
					case "Treasures":
						// This one's the exception to the rule. Just due to the dictionary structure, it's initialized with the class and able to handle multiple entries in different areas of the section (not that that should occur).
						this.ParseTreasures(subbodyText);
						break;
					default:
						throw new InvalidOperationException("Unrecognized subsection title: " + subsectionTitle);
				}
			}
		}
		#endregion

		#region Public Properties
		public string? Area { get; }

		public IList<Subsection> Assassinations { get; }

		public IList<Companion> Companions { get; }

		public IList<string>? Enemies { get; }

		public Subsection? Notes { get; }

		public IList<Subsection>? Other { get; }

		public IList<Subsection>? PlainText { get; }

		public IList<Subsection>? Plot { get; }

		public IList<Subsection>? Subquests { get; }

		public string Title { get; }

		public IDictionary<string, ICollection<string>> Treasures { get; } = new SortedDictionary<string, ICollection<string>>();
		#endregion

		#region Public Override Methods
		public override string ToString() => this.Title;
		#endregion

		#region Private Static Methods
		private static IEnumerable<Line> GetLineTypes(List<string> lines, LineType preferredType)
		{
			// Checks if line text looks valid. Rejects it if prefix is abnormally long or contains a period. Skips check if the new text contains a '[', suggesting that it's a line with a location attached.
			static Line? CheckValidLine(LineType lineType, string prefix, string newText)
			{
				if (prefix.Contains('.'))
				{
					// Debug.WriteLine("Excluded for period: " + prefix);
					return null;
				}

				var wordCount = prefix.Split(' ').Length;
				if (wordCount > 4)
				{
					// Debug.WriteLine("Excluded for word count: " + prefix);
					return null;
				}

				switch (prefix)
				{
					case "Note":
						lineType = LineType.Note;
						break;
					case "Tip":
						lineType = LineType.Tip;
						break;
				}

				if (lineType == LineType.Colon)
				{
					if (!newText.Contains('['))
					{
						// Debug.WriteLine("Excluded for no location: " + newText);
						return null;
					}

					if (newText.EndsWith("].", StringComparison.Ordinal))
					{
						newText = newText[0..^1];
					}
				}

				return new Line(lineType, prefix, newText);
			}

			if (lines.Count == 0)
			{
				yield break;
			}

			var endLines = new Stack<Line>();
			var singleLines = lines.Count - 1;
			Line? line;
			do
			{
				line = null;
				var text = lines[singleLines];
				if (char.IsLower(text[0]))
				{
					// If text starts with a lower-case letter, assume it's part of the previous line.
					continue;
				}

				foreach (var entry in SearchStrings)
				{
					// There cannot be more than one search entry found, so using a naive IndexOf loop as opposed to trying to ensure we have the earliest occurrence of any of them.
					var specialOffset = text.IndexOf(entry.Search, StringComparison.Ordinal);
					if (specialOffset > -1)
					{
						line = CheckValidLine(entry.LineType, text.Substring(0, specialOffset), text.Substring(specialOffset + entry.Offset).TrimStart());
						if (line != null)
						{
							endLines.Push(line);
							singleLines--;
						}
					}
				}
			}
			while (singleLines >= 0 && line != null);

			if (singleLines != -1)
			{
				var text = string.Join(" ", lines.GetRange(0, singleLines + 1));
				var lineType = preferredType;
				if (lineType == LineType.Plain && text.Split(Colon, 2) is var textSplit && textSplit.Length == 2)
				{
					switch (textSplit[0])
					{
						case "Note":
							lineType = LineType.Note;
							text = textSplit[1].TrimStart();
							break;
						case "Tip":
							lineType = LineType.Tip;
							text = textSplit[1].TrimStart();
							break;
					}
				}

				yield return new Line(lineType, text);
			}

			while (endLines.Count > 0)
			{
				yield return endLines.Pop();
			}
		}

		private static IEnumerable<string> ParseEnemies(string subsectionText)
		{
			var lines = WrappedLines(TrimStart(subsectionText), LineType.Plain);
			if (lines.Count != 1)
			{
				throw new InvalidOperationException("Malformed Enemies section!");
			}

			return lines[0].Text.Split(new[] { ", " }, StringSplitOptions.None);
		}

		private static IEnumerable<Subsection> ParsePlot(string subsectionText, string areaName)
		{
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			foreach (var entry in entries)
			{
				if (entry.Length > 0)
				{
					var wrapped = WrappedLines(TrimStart(entry), LineType.Plain);
					var subsection = new Subsection(wrapped, false);
					subsection.TrimAreaName(areaName);
					yield return subsection;
				}
			}
		}

		private static IEnumerable<Subsection> ParsePlainText(string subsectionText)
		{
			subsectionText = subsectionText.Trim();
			var textSections = PlainTextFinder.Split(subsectionText);
			if (textSections.Length == 0 || (textSections.Length == 1 && textSections[0].Length == 0))
			{
				throw new InvalidOperationException();
			}

			// This slightly odd loop construct handles both untitled text as well as titled within the same loop.
			for (var i = -1; i < textSections.Length; i += 2)
			{
				Line? title = null;
				var lines = new List<Line>();
				if (i > -1)
				{
					title = new Line(LineType.Title, textSections[i]);
				}

				if (textSections[i + 1].Length > 0)
				{
					foreach (var line in textSections[i + 1].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
					{
						lines.Add(new Line(LineType.Plain, line));
					}
				}

				yield return new Subsection(title, lines);
			}
		}

		private static IEnumerable<Subsection> ParseSubsection(string subsectionText, string areaName, LineType firstLine)
		{
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			foreach (var entry in entries)
			{
				if (entry.Length > 0)
				{
					var wrapped = WrappedLines(TrimStart(entry), firstLine);
					var subsection = new Subsection(wrapped, firstLine == LineType.Title);
					subsection.TrimAreaName(areaName);
					yield return subsection;
				}
			}
		}

		private static List<string> TrimStart(string text)
		{
			var hasContent = false;
			var retval = new List<string>();
			foreach (var line in text.Split('\n'))
			{
				var trimmed = line.Trim();
				if (trimmed.Length > 0)
				{
					hasContent = true;
				}

				if (hasContent)
				{
					retval.Add(line.Trim());
				}
			}

			return retval;
		}

		private static List<Line> WrappedLines(List<string> lines, LineType lineType)
		{
			var retval = new List<Line>();
			var sectionLines = new List<string>();
			foreach (var line in lines)
			{
				if (lineType == LineType.Title)
				{
					retval.Add(new Line(lineType, line));
					lineType = LineType.Plain;
				}
				else if (line.Length > 0)
				{
					sectionLines.Add(line);
				}
				else
				{
					retval.AddRange(GetLineTypes(sectionLines, lineType));
					sectionLines.Clear();
				}
			}

			if (sectionLines.Count > 0)
			{
				retval.AddRange(GetLineTypes(sectionLines, lineType));
			}

			return retval;
		}
		#endregion

		#region Private Methods
		private static IEnumerable<Companion> ParseCompanions(string subsectionText)
		{
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			if (entries.Length > 1)
			{
				for (var i = 1; i < entries.Length; i++)
				{
					var entryText = entries[i].TrimEnd();
					var companionLines = TrimStart(entryText);
					yield return new Companion(companionLines);
				}
			}
		}

		private void ParseTreasures(string subsectionText)
		{
			var lines = TrimStart(subsectionText);
			foreach (var line in lines)
			{
				var (quest, item) = SplitTreasure(line);
				if (!this.Treasures.TryGetValue(quest, out var treasureList))
				{
					treasureList = new SortedSet<string>();
					this.Treasures.Add(quest, treasureList);
				}

				treasureList.Add(Common.HarmonizeText(item));
			}

			static (string, string) SplitTreasure(string line)
			{
				var offset = line.LastIndexOf(" (");
				if (offset > -1 && line.Substring(offset).Contains("diff:", StringComparison.OrdinalIgnoreCase))
				{
					offset = -1;
				}

				var item = offset == -1 ? line : line.Substring(0, offset);
				var quest = offset == -1 ? string.Empty : line.Substring(offset + 2);
				offset = quest.LastIndexOf(')');
				if (offset > -1)
				{
					quest = quest.Remove(offset, 1);
				}

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

				return (quest, item);
			}
		}
		#endregion
	}
}