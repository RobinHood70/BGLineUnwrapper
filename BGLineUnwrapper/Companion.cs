namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using RobinHood70.CommonCode;

	public class Companion
	{
		#region Constructors
		public Companion(IEnumerable<string> lines)
		{
			var list = new List<string>(lines);
			var stats = GeneratedRegexes.StatParser().Match(list[0]);
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
				var match = GeneratedRegexes.BracketedLocFinder().Match(list[lastLine]);
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

				text = text[1..];
				this.Description = Common.HarmonizeSpacing(text.UpperFirst(CultureInfo.CurrentCulture));
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

		#region Public Methods
		public void Save(Saver saver)
		{
			saver.WriteTableRowStart();
			var companionName = new StylizedParagraph("companionname")
				{
					this.Name
				};
			if (this.Location != null)
			{
				companionName.Add(new StylizedText("location", this.Location));
			}

			saver.WriteTableCell(companionName);
			var infoText =
				this.Race.Replace(' ', '\xA0') +
				" " +
				this.Class.Replace(' ', '\xA0');
			saver.WriteTableCell("companion", infoText);
			saver.WriteTableCell("companion", this.Alignment.Replace(' ', '\xA0'));
			saver.WriteTableCell("companion", this.Strength);
			saver.WriteTableCell("companion", this.Dexterity);
			saver.WriteTableCell("companion", this.Constitution);
			saver.WriteTableCell("companion", this.Intelligence);
			saver.WriteTableCell("companion", this.Wisdom);
			saver.WriteTableCell("companion", this.Charisma);
			saver.WriteTableRowEnd();
			if (this.Description != null)
			{
				saver.WriteTableRowStart();
				saver.WriteTableCell(null, 9, [StylizedParagraph.FromText(this.Description)]);
				saver.WriteTableRowEnd();
			}
		}
		#endregion
	}
}
