namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	public class Companion
	{
		#region Static Fields
		private static readonly Regex StatParser = new Regex(@"\A(?<name>.*?)\s+(?<str>\d+(/\d+)?)\s+(?<dex>\d+)\s+(?<con>\d+)\s+(?<int>\d+)\s+(?<wis>\d+)\s+(?<cha>\d+)\s+(?<race>.*?)\s{2,}(?<class>.*?)\s+(?<align>.*?)\s*\Z");
		#endregion

		#region Constructors
		public Companion(IEnumerable<string> lines)
		{
			var list = new List<string>(lines);
			var stats = StatParser.Match(list[0]);
			if (!stats.Success)
			{
				throw new InvalidOperationException("Invalid stats line!");
			}

			this.Name = stats.Groups["name"].Value;
			this.Strength = stats.Groups["str"].Value;
			this.Dexterity = stats.Groups["dex"].Value;
			this.Constitution = stats.Groups["con"].Value;
			this.Intelligence = stats.Groups["int"].Value;
			this.Wisdom = stats.Groups["wis"].Value;
			this.Charisma = stats.Groups["cha"].Value;
			this.Race = stats.Groups["race"].Value;
			this.Class = stats.Groups["class"].Value;
			this.Alignment = stats.Groups["align"].Value;

			var lastLine = list.Count - 1;
			if (list[lastLine].StartsWith("Where ", StringComparison.Ordinal))
			{
				var match = Common.LocFinder.Match(list[lastLine]);
				if (match.Success)
				{
					this.Location = match.Groups["loc"].Value;
				}

				lastLine--;
			}

			if (lastLine >= 1)
			{
				var text = string.Empty;
				for (var i = 1; i <= lastLine; i++)
				{
					text += " " + list[i];
				}

				text = text.Substring(1);
				this.Description = Common.HarmonizeText(text);
			}
		}
		#endregion

		#region Public Properties
		public string Alignment { get; }

		public string Charisma { get; }

		public string Class { get; }

		public string Constitution { get; }

		public string? Description { get; }

		public string Dexterity { get; }

		public string Intelligence { get; }

		public string? Location { get; }

		public string Name { get; }

		public string Race { get; }

		public string Strength { get; }

		public string Wisdom { get; }
		#endregion
	}
}
