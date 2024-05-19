namespace BGLineUnwrapper
{
	using System.Diagnostics.CodeAnalysis;
	using System.Text.RegularExpressions;

	internal static partial class Common
	{
		#region Public GeneratedRegexes
		[GeneratedRegex(@":?\s*(?<loc>\[.*?\d+\.\d+\])", RegexOptions.ExplicitCapture, 10000)]
		public static partial Regex LocFinder();

		#endregion

		#region Public Methods

		// Simple implementation for now, since entire conversion process takes only seconds.
		public static string? HarmonizeSpacing(string? text)
		{
			if (text is null)
			{
				return null;
			}

			text = SpaceTrimmer().Replace(text, " ");
			text = HardSpaceReplacer().Replace(text, "\xA0$1");
			return SlashReplacer().Replace(text, "/\u200b");
		}
		#endregion

		#region Private GeneratedRegexes
		[SuppressMessage("Performance", "MA0023:Add RegexOptions.ExplicitCapture", Justification = "Not wanted.")]
		[GeneratedRegex(@" (\+|GP|HP|QXP|XP)", RegexOptions.None, 10000)]
		private static partial Regex HardSpaceReplacer();

		[GeneratedRegex(@"/\s*", RegexOptions.None, 10000)]
		private static partial Regex SlashReplacer();

		[GeneratedRegex(@"\ {2,}", RegexOptions.None, 10000)]
		private static partial Regex SpaceTrimmer();
		#endregion

	}
}
