namespace LineUnwrapper
{
	using System;
	using System.Text.RegularExpressions;

	internal static class Common
	{
		#region Fields
		private static Regex SpaceTrimmer = new Regex(@"\ {2,}");
		#endregion

		#region Public Properties
		public static Regex LocFinder { get; } = new Regex(@":?\s*(?<loc>\[.*?\d+\.\d+\])");
		#endregion

		#region Public Methods

		// Simple implementation for now, since entire conversion process takes only seconds.
		public static string HarmonizeText(string text)
		{
			if (text == null || text.Length < 2)
			{
				throw new ArgumentNullException();
			}

			text = char.ToUpperInvariant(text[0]) + text.Substring(1);
			text = SpaceTrimmer.Replace(text, " ");
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

	}
}
