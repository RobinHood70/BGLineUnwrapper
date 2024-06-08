namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	internal sealed class BG2Section : Section
	{
		#region Static Fields
		private static readonly string[] RegionOrder =
		[
			PlainText.Key,
			Exposition.Key,
			/*
				Companions.Key,
				Walkthrough.Key,
				Quests.Key
			*/
		];
		#endregion

		public BG2Section(SectionTitle sectionTitle, string body, BGDom dom)
			: base(sectionTitle)
		{
			body = body.Replace("\r", string.Empty, StringComparison.Ordinal);
			body = GeneratedRegexes.TextLocFinder().Replace(body, "${punc}[${x}.${y}]");
			var matches = GetMatches(body);
			if (matches[0].Length > 0)
			{
				this.Regions.Add(PlainText.Key, PlainText.Create(matches[0]));
			}

			this.ParseRegions(sectionTitle, dom, matches);
		}

		#region Public Properties
		public List<Companion> Companions { get; } = [];

		public Exposition? Exposition { get; }

		public Subsection? Notes { get; }

		public List<Subsection> Quests { get; } = [];

		public List<Subsection> Walkthrough { get; } = [];
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
			var split = GeneratedRegexes.BG2SubsectionFinder().Split(body);
			CheckForDupes(split);
			return split;
		}
		#endregion
	}
}
