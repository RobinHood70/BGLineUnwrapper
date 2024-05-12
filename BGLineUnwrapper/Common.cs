namespace LineUnwrapper
{
	using System;
	using System.Text.RegularExpressions;

	internal static partial class Common
	{
		#region Public GeneratedRegexes
		[GeneratedRegex(@":?\s*(?<loc>\[.*?\d+\.\d+\])")]
		public static partial Regex LocFinder();
		#endregion

		#region Public Methods

		// Simple implementation for now, since entire conversion process takes only seconds.
		public static string HarmonizeText(string text)
		{
			if (text == null || text.Length < 2)
			{
				throw new ArgumentNullException(nameof(text));
			}

			text = char.ToUpperInvariant(text[0]) + text[1..];
			text = SpaceTrimmer().Replace(text, " ");
			return text
				.Replace(" +", "\xA0+")
				.Replace(" GP", "\xA0GP")
				.Replace(" HP", "\xA0HP")
				.Replace(" XP", "\xA0XP")
				.Replace("/ ", "/\u200b")
				.Replace("/", "/\u200b");
		}

		public static string LocFormatter(Match loc) => LocFormatter(loc.Groups["x"].Value, loc.Groups["y"].Value);

		public static string LocFormatter(string x, string y) => $"[{x}.{y}]";
		#endregion

		#region Private GeneratedRegexes
		[GeneratedRegex(@"\ {2,}")]
		private static partial Regex SpaceTrimmer();
		#endregion

	}
}
