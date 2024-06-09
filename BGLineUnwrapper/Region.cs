namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using RobinHood70.CommonCode;

	public delegate Region RegionCreator(string body);

	public abstract class Region
	{
		#region Static Fields
		private static readonly char[] Colon = [':'];
		private static readonly SearchEntry[] SearchStrings =
		[
			new SearchEntry(":[", 1, LineType.Colon),
			new SearchEntry(": ", 2, LineType.Colon),
			new SearchEntry("--", 2, LineType.Dashed)
		];
		#endregion

		#region Public Abstract Properties
		public abstract string InstanceKey { get; }
		#endregion

		#region Public Methods
		public IReadOnlyList<Line> TextToLines(IEnumerable<string> lines, LineType lineType)
		{
			var retval = new List<Line>();
			var sectionLines = new List<string>();
			foreach (var line in lines)
			{
				if (line.Length > 0)
				{
					sectionLines.Add(line);
				}
				else
				{
					var newLines = this.GetLineTypes(sectionLines, lineType);
					retval.AddRange(newLines);
					sectionLines = [];
				}
			}

			if (sectionLines.Count > 0)
			{
				var newLines = this.GetLineTypes(sectionLines, lineType);
				retval.AddRange(newLines);
			}

			return retval;
		}
		#endregion

		#region Public Abstract Methods
		public abstract void Save(Saver saver);
		#endregion

		#region Protected Virtual Methods
		protected virtual Line? CheckValidLine(LineType lineType, string prefix, string newText)
		{
			// Checks if line text looks valid. Rejects it if prefix is abnormally long or contains a period. Skips check if the new text contains a '[', suggesting that it's a line with a location attached.
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

		protected virtual IList<Line> GetLineTypes(IReadOnlyList<string> lines, LineType preferredType)
		{
			if (lines.Count == 0)
			{
				return [];
			}

			var retval = new List<Line>();
			var (body, tail) = this.SplitLines(lines);
			if (body.Count > 0)
			{
				var text = string.Join(' ', body);
				var lineType = preferredType;
				if (preferredType == LineType.Plain && text.Split(Colon, 2) is var textSplit && textSplit.Length == 2)
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

				retval.Add(new Line(lineType, text));
			}

			retval.AddRange(tail);
			return retval;
		}

		protected virtual IReadOnlyList<Subsection> ParseSubsections(string subsectionText, bool checkTitle)
		{
			var retval = new List<Subsection>();
			var entries = GeneratedRegexes.DashedTitleFinder().Split(subsectionText);
			for (var entryNum = 0; entryNum < entries.Length; entryNum++)
			{
				var entry = entries[entryNum];
				if (entry.Length == 0)
				{
					continue;
				}

				var trimmed = TrimStart(entry);
				Line? title = null;

				// Check for entryNum != 0 handles cases of plaintext followed by titled sections.
				if (checkTitle && entryNum != 0)
				{
					title = new Line(LineType.Title, trimmed[0]);
					trimmed.RemoveAt(0);
				}

				var wrapped = this.TextToLines(trimmed, LineType.Plain);
				var subsection = new Subsection(title, wrapped);
				retval.Add(subsection);
			}

			return retval;
		}

		protected virtual (List<string>, List<Line>) SplitLines(IReadOnlyList<string> lines)
		{
			var tail = new List<Line>();
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
					if (specialOffset != -1)
					{
						line = this.CheckValidLine(entry.LineType, text[..specialOffset], text[(specialOffset + entry.Offset)..].TrimStart());
						if (line != null)
						{
							tail.Add(line);
							singleLines--;
						}
					}
				}
			}
			while (singleLines >= 0 && line != null);

			var body = new List<string>(lines.Take(singleLines + 1));
			tail.Reverse();

			return (body, tail);
		}
		#endregion

		#region Private Static Methods
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
		#endregion
	}
}
