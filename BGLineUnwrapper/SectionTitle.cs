namespace BGLineUnwrapper
{
	using System;

	public class SectionTitle
	{
		#region Static Fields
		private static readonly string[] SpaceParens = [" ("];
		#endregion

		#region Constructors
		public SectionTitle(string title)
		{
			title = title.Trim();
			ArgumentException.ThrowIfNullOrEmpty(title);

			var sectionNum = GeneratedRegexes.NumFinder().Match(title);
			if (sectionNum.Success && sectionNum.Length <= 4)
			{
				this.Number = sectionNum.Value;
				title = title.Remove(sectionNum.Index, sectionNum.Length);
			}
			else
			{
				this.Number = string.Empty;
			}

			title = GeneratedRegexes.IDFinder().Replace(title, string.Empty);
			var areaMatch = GeneratedRegexes.AreaFinder().Match(title);
			if (areaMatch.Success)
			{
				this.Area = areaMatch.Groups["area"].Value.Replace(" ", string.Empty, StringComparison.Ordinal);
				title = title.Remove(areaMatch.Index, areaMatch.Length);
			}
			else
			{
				this.Area = string.Empty;
			}

			title = GeneratedRegexes.ChapterReplacer().Replace(title, string.Empty);
			this.Name = title.Trim();
			this.BaseName = this.Name.Split(SpaceParens, StringSplitOptions.None)[0];
		}
		#endregion

		#region Public Properties
		public string Area { get; }

		public string BaseName { get; }

		public string Name { get; }

		public string Number { get; }
		#endregion

		#region Public Override Methods
		public override string ToString()
		{
			var retval = this.Number.Length > 0
				? $"{this.Number}   "
				: string.Empty;
			retval += this.Name;
			if (this.Area.Length > 0)
			{
				retval += $" ({this.Area})";
			}

			return retval;
		}
		#endregion
	}
}
