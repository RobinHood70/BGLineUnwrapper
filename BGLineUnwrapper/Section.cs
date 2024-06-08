namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	public abstract class Section(SectionTitle title)
	{
		#region Public Properties
		public IDictionary<string, ITextRegion> Regions { get; } = new Dictionary<string, ITextRegion>(StringComparer.Ordinal);

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
				subsection.Title?.TrimAreaName(areaName);
				foreach (var line in subsection.Lines)
				{
					line.TrimAreaName(areaName);
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
	}
}