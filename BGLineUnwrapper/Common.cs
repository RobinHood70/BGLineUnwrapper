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
