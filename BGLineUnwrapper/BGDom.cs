namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using RobinHood70.CommonCode;

	public partial class BGDom(IList<Section> sections) : ReadOnlyCollection<Section>(sections)
	{
		#region Protected Static GeneratedRegexes
		[GeneratedRegex(@"\n{2,}-{75,}\s*(?<title>[^\n]+)\n+-{75,}\n", RegexOptions.ExplicitCapture | RegexOptions.Compiled, 10000)]
		public static partial Regex SectionSplitter();
		#endregion

		#region Protected Static Methods
		public static string HarmonizeText(string text)
		{
			ArgumentNullException.ThrowIfNull(text);
			text = text.Replace("\r", string.Empty, StringComparison.Ordinal);
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

					lines[lineNum] = new string(' ', 4 * charNum) + line[charNum..].TrimEnd();
					if (lines[lineNum].Contains('\t', StringComparison.Ordinal))
					{
						Debug.WriteLine($"Unexpected tab on line {lineNum.ToStringInvariant()}: {lines[lineNum]}");
					}
				}
			}

			return string.Join('\n', lines);
		}

		#endregion
	}
}
