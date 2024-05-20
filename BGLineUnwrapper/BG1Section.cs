namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	internal sealed class BG1Section : Section
	{
		#region Fields
		private readonly Dictionary<string, ITextRegion> regions = new(StringComparer.Ordinal);
		private readonly string[] regionOrder =
		[
			PlainText.Key,
			Note.Key,
			Companions.Key,
			Enemies.Key,
			AssassinationAttempts.Key,
			Treasures.Key,
			Plot.Key,
			Subquests.Key,
			Other.Key
		];
		#endregion

		#region Constructors
		public BG1Section(SectionTitle sectionTitle, string body, BGDom dom)
			: base(sectionTitle)
		{
			if (sectionTitle.Number.Length == 0)
			{
				var pt = PlainText.Create(body);
				this.regions.Add(pt.InstanceKey, pt);
			}
			else
			{
				body = GeneratedRegexes.TextLocFinder().Replace(body, "${punc}[${x}.${y}]");
				var matches = GetMatches(body);
				this.ParseRegions(sectionTitle, dom, matches);
			}
		}
		#endregion

		#region Public Override Methods
		public override void Save(Saver saver)
		{
			var stylizedText = new List<StylizedText>(StylizedText.StylizeLocations(this.Title.Name));
			if (this.Title.Area != null)
			{
				stylizedText.Add(new StylizedText("\xA0"));
				stylizedText.Add(new StylizedText("area", "(" + this.Title.Area + ")"));
			}

			saver.WriteHeader(1, stylizedText);
			foreach (var regionKey in this.regionOrder)
			{
				if (this.regions.TryGetValue(regionKey, out var region))
				{
					region.Save(saver);
				}
			}
		}
		#endregion

		#region Private Static Methods
		private static void CheckForDupes(string[] matches)
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

		private static string[] GetMatches(string body)
		{
			var matches = GeneratedRegexes.SubsectionFinder().Split(body);
			if (matches[0].Length > 0)
			{
				throw new InvalidOperationException("Invalid section format!");
			}

			CheckForDupes(matches);
			return matches;
		}
		#endregion

		#region Private Methods
		private void ParseRegions(SectionTitle sectionTitle, BGDom dom, string[] matches)
		{
			for (var i = 1; i < matches.Length; i += 2)
			{
				var subbodyText = matches[i + 1];
				if (dom.RegionCreators.TryGetValue(matches[i], out var creator))
				{
					var region = creator(subbodyText);
					if (region is ISubsectioned subsectioned)
					{
						Common.CleanupSubsections(subsectioned.Subsections, sectionTitle.BaseName);
					}

					this.regions.Add(region.InstanceKey, region);
				}
				else
				{
					throw new InvalidOperationException("Unrecognized subsection title: " + matches[i]);
				}
			}
		}
		#endregion
	}
}
