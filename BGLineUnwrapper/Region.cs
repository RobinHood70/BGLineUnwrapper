namespace BGLineUnwrapper
{
	using System.Collections.Generic;
	using System.Text;

	public delegate Region RegionCreator(string body);

	public abstract class Region
	{
		#region Static Fields
		private static readonly char[] Colon = [':'];
		private static readonly SearchEntry[] SearchStrings =
		[
			new SearchEntry("Note:", 5, LineType.Note, true),
			new SearchEntry("Tip:", 4, LineType.Tip, true),
			new SearchEntry(":[", 1, LineType.Colon, false),
			new SearchEntry(": ", 2, LineType.Colon, false),
			new SearchEntry("--", 2, LineType.Dashed, false),
		];
		#endregion

		#region Public Abstract Properties
		public abstract string InstanceKey { get; }
		#endregion

		#region Public Methods
		public IReadOnlyList<Line> TextToParagraphs(IEnumerable<string> lines, LineType lineType)
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
					var newLines = this.TextToLines(sectionLines, lineType);
					retval.AddRange(newLines);
					sectionLines = [];
				}
			}

			if (sectionLines.Count > 0)
			{
				var newLines = this.TextToLines(sectionLines, lineType);
				retval.AddRange(newLines);
			}

			return retval;
		}
		#endregion

		#region Public Abstract Methods
		public abstract void Save(Saver saver);
		#endregion

		#region Protected Virtual Methods
		protected virtual IReadOnlyList<Line> TextToLines(IEnumerable<string> textLines, LineType preferredType)
		{
			var retval = new List<Line>();
			var sb = new StringBuilder(160);
			string? prefix = null;
			var lineType = preferredType;
			foreach (var textLine in textLines)
			{
				var text = textLine;
				foreach (var entry in SearchStrings)
				{
					var split = textLine.Split(entry.Search, 2);
					if (split.Length > 1 && (split[0].Length == 0 || !entry.LineStart))
					{
						if (sb.Length > 0)
						{
							retval.Add(new Line(preferredType, prefix, sb.ToString()));
							sb.Clear();
						}

						if (entry.LineStart)
						{
							// At least so far, search types that are at the start of the line are multi-line, so alter the data as needed and continue into the normal loop.
							lineType = entry.LineType;
							prefix = entry.Search.TrimEnd(Colon);
							text = split[1].TrimStart();
						}
						else
						{
							retval.Add(new Line(entry.LineType, split[0], split[1]));
							lineType = preferredType;
							prefix = null;
							text = null;
						}

						break;
					}
				}

				if (text is not null)
				{
					// No specific line type was added, so assume it's a normal line.
					if (sb.Length != 0)
					{
						sb.Append(' ');
					}

					sb.Append(text);
				}

				/*
				var wordCount = GeneratedRegexes.Whitespace().Count(prefix) + 1;
				if (!prefix.Contains('.', StringComparison.Ordinal) && wordCount < 5)
				{
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

				if (line != null)
				{
					retval.Add(line);
				}*/
			}

			if (sb.Length != 0)
			{
				retval.Add(new Line(lineType, prefix, sb.ToString()));
			}

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

				var wrapped = this.TextToParagraphs(trimmed, LineType.Plain);
				var subsection = new Subsection(title, wrapped);
				retval.Add(subsection);
			}

			return retval;
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
