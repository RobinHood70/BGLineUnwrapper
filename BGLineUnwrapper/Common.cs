namespace LineUnwrapper
{
	using System.Text.RegularExpressions;

	internal static class Common
	{
		#region Public Static Properties
		public static Regex LocFinder { get; } = new Regex(@":?\s*(?<loc>\[\d+\.\d+\])");
		#endregion

		#region Public Static Methods

		// Simple implementation for now, since entire conversion process takes only seconds.
		public static string HarmonizeText(string text) => text
			.Replace(" +", "\xA0+")
			.Replace(" GP", "\xA0GP")
			.Replace(" HP", "\xA0HP")
			.Replace(" XP", "\xA0XP")
			.Replace("/ ", "/\u200b")
			.Replace("/", "/\u200b");

		public static string LocFormatter(Match loc) => LocFormatter(loc.Groups["x"].Value, loc.Groups["y"].Value);

		public static string LocFormatter(string x, string y) => $"[{x}.{y}]";
		#endregion

	}
}
