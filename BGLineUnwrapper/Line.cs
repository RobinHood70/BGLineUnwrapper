namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using RobinHood70.CommonCode;

	#region Public Enumerations
	public enum LineType
	{
		Plain,
		Title,
		Colon,
		Dashed,
		Note,
		Tip,
	}
	#endregion

	public class Line(LineType lineType, string? prefix, string text)
	{
		#region Static Fields
		private static readonly char[] Colon = [':'];
		private static readonly SearchEntry[] SearchStrings =
		[
			new SearchEntry(":[", 1, LineType.Colon),
			new SearchEntry(": ", 2, LineType.Colon),
			new SearchEntry("--", 2, LineType.Dashed)
		];

		private static readonly char[] TrimChars = [' ', ',', '.'];
		#endregion

		#region Constructors
		public Line(LineType lineType, string text)
			: this(lineType, null, text)
		{
		}
		#endregion

		#region Public Properties
		public string? Prefix { get; } = prefix is null ? null : Common.HarmonizeSpacing(prefix);

		public string Text { get; set; } = Common.HarmonizeSpacing(text) ?? string.Empty;

		public LineType Type { get; set; } = lineType;
		#endregion

		#region Public Static Methods
		public static IReadOnlyList<Line> TextToLines(IEnumerable<string> lines, LineType lineType)
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
					var newLines = GetLineTypes(sectionLines, lineType);
					retval.AddRange(newLines);
					sectionLines.Clear();
				}
			}

			if (sectionLines.Count > 0)
			{
				var newLines = GetLineTypes(sectionLines, lineType);
				retval.AddRange(newLines);
			}

			return retval;
		}
		#endregion

		#region Public Methods
		public void TrimAreaName(string areaName)
		{
			if (this.Type is not LineType.Colon and not LineType.Title)
			{
				return;
			}

			if (this.Text.StartsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				this.Text = this.Text[areaName.Length..].TrimStart(TrimChars);
			}
			else if (this.Text.EndsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				this.Text = this.Text[..^areaName.Length].TrimEnd(TrimChars);
			}
			else
			{
				var parens = " (" + areaName + ")";
				this.Text = this.Text.Replace(parens, string.Empty, StringComparison.Ordinal);
			}
		}
		#endregion

		#region Public Override Methods
		public override string ToString() => (this.Prefix == null ? string.Empty : this.Prefix + ": ") + this.Text;
		#endregion

		#region Private Static Methods
		private static Line? CheckValidLine(LineType lineType, string prefix, string newText)
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

		private static IEnumerable<Line> GetLineTypes(List<string> lines, LineType preferredType)
		{
			// Checks if line text looks valid. Rejects it if prefix is abnormally long or contains a period. Skips check if the new text contains a '[', suggesting that it's a line with a location attached.
			if (lines.Count == 0)
			{
				yield break;
			}

			var (singleLines, endLines) = SplitEndLines(lines);
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

		private static (int, Stack<Line>) SplitEndLines(List<string> lines)
		{
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
					if (specialOffset != -1)
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

			return (singleLines, endLines);
		}
		#endregion

		#region Private Classes
		private sealed class SearchEntry(string search, int offset, LineType lineType)
		{
			public LineType LineType { get; } = lineType;

			public int Offset { get; } = offset;

			public string Search { get; } = search;
		}
		#endregion
	}
}