namespace BGLineUnwrapper
{
	using System.Diagnostics.CodeAnalysis;
	using System.Text.RegularExpressions;

	internal static partial class GeneratedRegexes
	{
		private const int Timeout = 10000;

		[GeneratedRegex(@"\((?<area>([A-Z]{2}\d{4},? ?)+)\)", RegexOptions.ExplicitCapture, Timeout)]
		public static partial Regex AreaFinder();

		[GeneratedRegex(@":?\s*(?<loc>\[.*?\d+\.\d+\])", RegexOptions.ExplicitCapture, Timeout)]
		public static partial Regex BracketedLocFinder();

		[GeneratedRegex(@"Chapter \d: ", RegexOptions.None, Timeout)]
		public static partial Regex ChapterReplacer();

		[SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Not wanted.")]
		[GeneratedRegex(@" (\+|GP|HP|QXP|XP)", RegexOptions.None, Timeout)]
		public static partial Regex HardSpaceReplacer();

		[GeneratedRegex(@"\[[\w]{4,6}\]", RegexOptions.None, Timeout)]
		public static partial Regex IDFinder();

		[GeneratedRegex(@"\A[\w]+\.\d*", RegexOptions.None, Timeout)]
		public static partial Regex NumFinder();

		[GeneratedRegex(@"(?<title>[^\n]*)\n-+\n", RegexOptions.Singleline, Timeout)]
		public static partial Regex PlainTextFinder();

		[GeneratedRegex(@"\n{2,}-{75,}\s*(?<title>[^\n]+)\n+-{75,}\n", RegexOptions.ExplicitCapture | RegexOptions.Compiled, Timeout)]
		public static partial Regex SectionSplitter();

		[GeneratedRegex(@"/\s*", RegexOptions.None, Timeout)]
		public static partial Regex SlashReplacer();

		[GeneratedRegex(@"\ {2,}", RegexOptions.None, Timeout)]
		public static partial Regex SpaceTrimmer();

		[GeneratedRegex(@"\A(?<name>.*?)\s+(?<str>\d+(/\d+)?)\s+(?<dex>\d+)\s+(?<con>\d+)\s+(?<int>\d+)\s+(?<wis>\d+)\s+(?<cha>\d+)\s+(?<race>.*?)\s{2,}(?<class>.*?)\s+(?<align>.*?)\s*\Z", RegexOptions.ExplicitCapture, Timeout)]
		public static partial Regex StatParser();

		[GeneratedRegex(@"\n+(?<subsection>[^ \n][^:\n]*?):", RegexOptions.Singleline, Timeout)]
		public static partial Regex SubsectionFinder();

		[GeneratedRegex(@",?(\s+at)?\s*(\(x +(?<x>\d+),? +y +(?<y>\d+)\)|x +(?<x>\d+),? +y +(?<y>\d+))(?<punc>[\p{P}]*)", RegexOptions.ExplicitCapture, Timeout)]
		public static partial Regex TextLocFinder();
	}
}
