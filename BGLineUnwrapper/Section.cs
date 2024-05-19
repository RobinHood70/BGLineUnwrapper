namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using RobinHood70.CommonCode;

	public abstract partial class Section(SectionTitle title)
	{
		#region Constants
		public const string EntryDelimiter = "\n  - ";
		#endregion

		#region Static Fields
		private static readonly char[] Colon = [':'];
		private static readonly (string Search, int Offset, LineType LineType)[] SearchStrings =
		[
			(":[", 1, LineType.Colon),
			(": ", 2, LineType.Colon),
			("--", 2, LineType.Dashed)
		];
		#endregion

		#region Public Properties
		public SectionTitle Title => title;
		#endregion

		#region Public Abstract Methods
		public abstract void Save(Saver saver);
		#endregion

		#region Public Override Methods
		public override string ToString() => title.ToString();
		#endregion

		#region Protected Static Methods
		protected static IReadOnlyList<string> TrimStart(string text)
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

		protected static IReadOnlyList<Line> WrappedLines(IEnumerable<string> lines, LineType lineType)
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

		#region Protected Static GeneratedRegexes
		[GeneratedRegex(@",?(\s+at)?\s*(\(x +(?<x>\d+),? +y +(?<y>\d+)\)|x +(?<x>\d+),? +y +(?<y>\d+))(?<punc>[\p{P}]*)", RegexOptions.ExplicitCapture, 10000)]
		protected static partial Regex LocFinder();

		[GeneratedRegex(@"(?<title>[^\n]*)\n-+\n", RegexOptions.Singleline, 10000)]
		protected static partial Regex PlainTextFinder();

		[GeneratedRegex(@"\n+(?<subsection>[^ \n][^:\n]*?):", RegexOptions.Singleline, 10000)]
		protected static partial Regex SubsectionFinder();
		#endregion

		#region Private Static Methods
		private static IEnumerable<Line> GetLineTypes(List<string> lines, LineType preferredType)
		{
			// Checks if line text looks valid. Rejects it if prefix is abnormally long or contains a period. Skips check if the new text contains a '[', suggesting that it's a line with a location attached.
			static Line? CheckValidLine(LineType lineType, string prefix, string newText)
			{
				if (prefix.Contains('.', StringComparison.Ordinal))
				{
					// Debug.WriteLine("Excluded for period: " + prefix);
					return null;
				}

				prefix = prefix.UpperFirst(CultureInfo.CurrentCulture);
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
					if (!newText.Contains('[', StringComparison.Ordinal))
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
						line = CheckValidLine(entry.LineType, text[..specialOffset], text[(specialOffset + entry.Offset)..].TrimStart());
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
				var text = string.Join(' ', lines.GetRange(0, singleLines + 1));
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
		#endregion
	}
}