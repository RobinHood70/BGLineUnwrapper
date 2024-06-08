namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	internal static class Common
	{
		#region Public Methods

		// Simple implementation for now, since entire conversion process takes only seconds.
		public static string HarmonizeSpacing(string text)
		{
			ArgumentNullException.ThrowIfNull(text);
			text = GeneratedRegexes.SpaceTrimmer().Replace(text, " ");
			text = GeneratedRegexes.HardSpaceReplacer().Replace(text, "\xA0$1");
			return GeneratedRegexes.SlashReplacer().Replace(text, "/\u200b");
		}

		public static IReadOnlyList<Subsection> ParseSubsections(string subsectionText, bool checkTitle)
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

				var wrapped = Line.TextToLines(trimmed, LineType.Plain);
				var subsection = new Subsection(title, wrapped);
				retval.Add(subsection);
			}

			return retval;
		}

		public static IList<string> TrimStart(string text)
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
