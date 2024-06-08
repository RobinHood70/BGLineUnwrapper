namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	internal sealed class BG1Section : Section
	{
		#region Fields
		private static readonly string[] RegionOrder =
		[
			BulletedText.Key,
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
				var pt = BulletedText.Create(body);
				this.Regions.Add(pt.InstanceKey, pt);
			}
			else
			{
				body = GeneratedRegexes.TextLocFinder().Replace(body, "${punc}[${x}.${y}]");
				var matches = GetMatches(body);
				if (matches[0].Length > 0)
				{
					this.Regions.Add(BulletedText.Key, BulletedText.Create(matches[0]));
				}

				this.ParseRegions(sectionTitle, dom, matches);
			}
		}
		#endregion

		#region Public Override Methods
		public override void Save(Saver saver)
		{
			var stylizedText = new List<StylizedText>(StylizedText.StylizeLocations(this.Title.Name));
			if (this.Title.Area.Length > 0)
			{
				stylizedText.Add(new StylizedText("\xA0"));
				stylizedText.Add(new StylizedText("area", "(" + this.Title.Area + ")"));
			}

			saver.WriteHeader(1, stylizedText);
			foreach (var regionKey in RegionOrder)
			{
				if (this.Regions.TryGetValue(regionKey, out var region))
				{
					region.Save(saver);
				}
			}
		}
		#endregion

		#region Private Static Methods
		private static string[] GetMatches(string body)
		{
			var matches = GeneratedRegexes.BG1SubsectionFinder().Split(body);
			if (matches[0].Length > 0)
			{
				throw new InvalidOperationException("Invalid section format!");
			}

			CheckForDupes(matches);
			return matches;
		}
		#endregion
	}
}
