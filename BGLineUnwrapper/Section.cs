namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	public abstract class Section(SectionTitle title)
	{
		#region Static Fields
		private static readonly char[] TrimChars = [' ', ',', '.'];
		#endregion

		#region Public Properties
		public IDictionary<string, Region> Regions { get; } = new Dictionary<string, Region>(StringComparer.Ordinal);

		public SectionTitle Title => title;
		#endregion

		#region Public Abstract Methods
		public abstract void Save(Saver saver);
		#endregion

		#region Public Override Methods
		public override string ToString() => title.ToString();
		#endregion

		#region Protected Static Methods
		protected static void CheckForDupes(string[] matches)
		{
			var found = new HashSet<string>(StringComparer.Ordinal);
			for (var i = 1; i < matches.Length; i += 2)
			{
				if (!found.Add(matches[i]))
				{
					throw new InvalidOperationException("Duplicate entry.");
				}
			}
		}

		protected static void CleanupSubsections(IEnumerable<Subsection> subsections, string areaName)
		{
			foreach (var subsection in subsections)
			{
				if (subsection.Title is not null)
				{
					TrimAreaName(subsection.Title, areaName);
				}

				foreach (var line in subsection.Lines)
				{
					TrimAreaName(line, areaName);
				}

				subsection.ReparseLocations();
			}
		}

		protected void ParseRegions(SectionTitle sectionTitle, BGDom dom, string[] matches)
		{
			for (var i = 1; i < matches.Length; i += 2)
			{
				var subbodyText = matches[i + 1];
				if (dom.RegionCreators.TryGetValue(matches[i], out var creator))
				{
					var region = creator(subbodyText);
					if (region is ISubsectioned subsectioned)
					{
						CleanupSubsections(subsectioned.Subsections, sectionTitle.BaseName);
					}

					this.Regions.Add(region.InstanceKey, region);
				}
				else
				{
					// throw new InvalidOperationException("Unrecognized subsection title: " + matches[i]);
				}
			}
		}
		#endregion

		#region Private Methods
		private static void TrimAreaName(Line line, string areaName)
		{
			if (line.Type is not LineType.Colon and not LineType.Title)
			{
				return;
			}

			if (line.Text.StartsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				line.Text = line.Text[areaName.Length..].TrimStart(TrimChars);
			}
			else if (line.Text.EndsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				line.Text = line.Text[..^areaName.Length].TrimEnd(TrimChars);
			}
			else
			{
				var parens = " (" + areaName + ")";
				line.Text = line.Text.Replace(parens, string.Empty, StringComparison.Ordinal);
			}
		}
		#endregion
	}
}