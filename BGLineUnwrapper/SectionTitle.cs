namespace BGLineUnwrapper
{
	using System;
	using System.Text.RegularExpressions;

	public partial class SectionTitle
	{
		#region Static Fields
		private static readonly string[] SpaceParens = [" ("];
		#endregion

		#region Constructors
		public SectionTitle(string title)
		{
			title = title.Trim();
			ArgumentException.ThrowIfNullOrEmpty(title);

			var sectionNum = NumFinder().Match(title);
			if (sectionNum.Success && sectionNum.Length <= 4)
			{
				this.Number = sectionNum.Value;
				title = title.Remove(sectionNum.Index, sectionNum.Length);
			}
			else
			{
				this.Number = string.Empty;
			}

			title = IDFinder().Replace(title, string.Empty);
			var areaMatch = AreaFinder().Match(title);
			if (areaMatch.Success)
			{
				this.Area = areaMatch.Groups["area"].Value;
				title = title.Remove(areaMatch.Index, areaMatch.Length);
			}
			else
			{
				this.Area = string.Empty;
			}

			title = ChapterReplacer().Replace(title, string.Empty);
			this.Text = title.Trim();
		}
		#endregion

		#region Public Properties
		public string Area { get; }

		public string Number { get; }

		public string Text { get; }
		#endregion

		#region Public Methods
		public string TrimText() => this.Text.Split(SpaceParens, StringSplitOptions.None)[0];
		#endregion

		#region Public Override Methods
		public override string ToString()
		{
			var retval = this.Number.Length > 0
				? $"{this.Number}   "
				: string.Empty;
			retval += this.Text;
			if (this.Area.Length > 0)
			{
				retval += $" ({this.Area})";
			}

			return retval;
		}
		#endregion

		#region Private Static GeneratedRegexes
		[GeneratedRegex(@"\((?<area>([A-Z]{2}\d{4},? ?)+)\)", RegexOptions.ExplicitCapture, 10000)]
		private static partial Regex AreaFinder();

		[GeneratedRegex(@"\[[\w]{4,6}\]", RegexOptions.None, 10000)]
		private static partial Regex IDFinder();

		[GeneratedRegex(@"\A[\w]+\.\d*", RegexOptions.None, 10000)]
		private static partial Regex NumFinder();

		[GeneratedRegex(@"Chapter \d: ", RegexOptions.None, 10000)]
		private static partial Regex ChapterReplacer();
		#endregion
	}
}
