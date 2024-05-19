namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using RobinHood70.CommonCode;

	internal sealed class BG1Section : Section
	{
		#region Static Fields
		private static readonly string[] CommaSpace = [", "];
		private static readonly char[] NewLine = ['\n'];
		private static readonly char[] OpenSquare = ['['];
		#endregion

		#region Fields
		private readonly List<Subsection> assassinations = [];
		private readonly List<Companion> companions = [];
		private readonly SortedSet<string> enemies = new(StringComparer.OrdinalIgnoreCase);
		private readonly Subsection? notes;
		private readonly List<Subsection> other = [];
		private readonly List<Subsection> plainText = [];
		private readonly List<Subsection> plot = [];
		private readonly List<Subsection> subquests = [];
		private readonly SortedDictionary<string, List<string>> treasures = [];
		#endregion

		#region Constructors
		public BG1Section(SectionTitle sectionTitle, string body)
			: base(sectionTitle)
		{
			if (sectionTitle.Number.Length == 0)
			{
				this.plainText.AddRange(ParsePlainText(body));
				return;
			}

			body = LocFinder().Replace(body, "${punc}[${x}.${y}]");
			var matches = SubsectionFinder().Split(body);
			if (matches[0].Length > 0)
			{
				throw new InvalidOperationException("Invalid section format!");
			}

			var found = new HashSet<string>(StringComparer.Ordinal);
			for (var i = 1; i < matches.Length; i += 2)
			{
				var subsectionTitle = matches[i];
				if (string.Equals(subsectionTitle, "Assassination Attempt", StringComparison.Ordinal) ||
					string.Equals(subsectionTitle, "Enemy Wizards", StringComparison.Ordinal))
				{
					subsectionTitle = "Assassination Attempts";
				}

				if (found.Contains(subsectionTitle))
				{
					throw new InvalidOperationException("Duplicate entry.");
				}

				found.Add(subsectionTitle);
				var subbodyText = matches[i + 1];
				switch (subsectionTitle)
				{
					case "Assassination Attempts":
						this.assassinations.AddRange(ParseSubsection(subbodyText, this.Title.TrimText(), LineType.Title));
						break;
					case "Companions":
						this.companions.AddRange(ParseCompanions(subbodyText));
						break;
					case "Enemies":
						foreach (var enemy in ParseEnemies(subbodyText))
						{
							this.enemies.Add(enemy);
						}

						break;
					case "Note":
						this.notes = new Subsection(WrappedLines(TrimStart(subbodyText), LineType.Note), false);
						break;
					case "Other":
						this.other.AddRange(ParseSubsection(subbodyText, this.Title.TrimText(), LineType.Title));
						break;
					case "Plot":
						this.plot.AddRange(ParseSubsection(subbodyText, this.Title.TrimText(), LineType.Plain));
						break;
					case "Subquests":
						this.subquests.AddRange(ParseSubsection(subbodyText, this.Title.TrimText(), LineType.Title));
						break;
					case "Treasures":
						var unsorted = ParseTreasures(subbodyText);
						foreach (var (key, value) in unsorted)
						{
							this.treasures[key] = value;
						}

						break;
					default:
						throw new InvalidOperationException("Unrecognized subsection title: " + subsectionTitle);
				}
			}
		}
		#endregion

		#region Public Override Methods
		public override void Save(Saver saver)
		{
			var stylizedText = new List<StylizedText>(StylizedText.StylizeLocations(this.Title.Text));
			if (this.Title.Area != null)
			{
				stylizedText.Add(new StylizedText("\xA0"));
				stylizedText.Add(new StylizedText("area", "(" + this.Title.Area + ")"));
			}

			saver.WriteHeader(1, stylizedText);
			saver.EmitPlainText(this.plainText);
			saver.EmitLines(this.notes?.Lines);
			this.EmitCompanions(saver);
			this.EmitEnemies(saver);
			saver.EmitSubsections("Assassination Attempts", this.assassinations);
			this.EmitTreasures(saver);
			saver.EmitSubsections("Plot", this.plot);
			saver.EmitSubsections("Subquests", this.subquests);
			saver.EmitSubsections("Other", this.other);
		}
		#endregion

		#region Private Static Methods
		private static IEnumerable<Companion> ParseCompanions(string subsectionText)
		{
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			if (entries.Length > 1)
			{
				for (var i = 1; i < entries.Length; i++)
				{
					var entryText = entries[i].TrimEnd();
					var companionLines = TrimStart(entryText);
					yield return new Companion(companionLines);
				}
			}
		}

		private static string[] ParseEnemies(string subsectionText)
		{
			var lines = WrappedLines(TrimStart(subsectionText), LineType.Plain);
			return lines.Count != 1
				? throw new InvalidOperationException("Malformed Enemies section!")
				: lines[0].Text.Split(CommaSpace, StringSplitOptions.None);
		}

		private static IEnumerable<Subsection> ParsePlainText(string subsectionText)
		{
			subsectionText = subsectionText.Trim();
			var textSections = PlainTextFinder().Split(subsectionText);
			if (textSections.Length == 0 || (textSections.Length == 1 && textSections[0].Length == 0))
			{
				throw new InvalidOperationException();
			}

			// This slightly odd loop construct handles both untitled text as well as titled within the same loop.
			for (var i = -1; i < textSections.Length; i += 2)
			{
				Line? title = null;
				var lines = new List<Line>();
				if (i > -1)
				{
					title = new Line(LineType.Title, textSections[i]);
				}

				if (textSections[i + 1].Length > 0)
				{
					foreach (var line in textSections[i + 1].Split(NewLine, StringSplitOptions.RemoveEmptyEntries))
					{
						lines.Add(new Line(LineType.Plain, line));
					}
				}

				yield return new Subsection(title, lines);
			}
		}

		private static IEnumerable<Subsection> ParseSubsection(string subsectionText, string areaName, LineType firstLine)
		{
			var entries = subsectionText.Split(new[] { EntryDelimiter }, StringSplitOptions.None);
			foreach (var entry in entries)
			{
				if (entry.Length > 0)
				{
					var wrapped = WrappedLines(TrimStart(entry), firstLine);
					var subsection = new Subsection(wrapped, firstLine == LineType.Title);
					subsection.TrimAreaName(areaName);
					yield return subsection;
				}
			}
		}

		private static Dictionary<string, List<string>> ParseTreasures(string subsectionText)
		{
			var retval = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			var lines = TrimStart(subsectionText);
			foreach (var line in lines)
			{
				var (quest, item) = SplitTreasure(line);
				if (!retval.TryGetValue(quest, out var treasureList))
				{
					treasureList = [];
					retval.Add(quest, treasureList);
				}

				treasureList.Add(Common.HarmonizeSpacing(item.UpperFirst(CultureInfo.CurrentCulture)));
			}

			return retval;
		}

		private static (string, string) SplitTreasure(string line)
		{
			var offset = line.LastIndexOf(" (", StringComparison.Ordinal);
			if (offset > -1 && line[offset..].Contains("diff:", StringComparison.OrdinalIgnoreCase))
			{
				offset = -1;
			}

			var item = offset == -1 ? line : line[..offset];
			var quest = offset == -1 ? string.Empty : line[(offset + 2)..];
			offset = quest.LastIndexOf(')');
			if (offset > -1)
			{
				quest = quest.Remove(offset, 1);
			}

			if (string.Equals(quest, item, StringComparison.OrdinalIgnoreCase))
			{
				quest = string.Empty;
			}

			var locSplit = item.Split(OpenSquare, 2);
			if (locSplit.Length == 2)
			{
				item = locSplit[0];
				quest += '[' + locSplit[1];
			}

			return (quest, item);
		}
		#endregion

		#region Private Methods
		private void EmitCompanions(Saver saver)
		{
			if (this.companions.Count == 0)
			{
				return;
			}

			saver.WriteHeader(2, "Companions");
			saver.WriteTableStart("companions", 100);
			saver.WriteTableHeader(
				("Name", 31),
				("Race and Class", 31),
				("Alignment", 31),
				("S", 1),
				("D", 1),
				("C", 1),
				("I", 1),
				("W", 1),
				("Ch", 1));
			foreach (var companion in this.companions)
			{
				saver.WriteTableRowStart();
				var companionName = new Paragraph("companionname")
				{
					companion.Name
				};
				if (companion.Location != null)
				{
					companionName.Add(new StylizedText("location", companion.Location));
				}

				saver.WriteTableCell(companionName);
				var infoText =
					companion.Race.Replace(' ', '\xA0') +
					" " +
					companion.Class.Replace(' ', '\xA0');
				saver.WriteTableCell("companion", infoText);
				saver.WriteTableCell("companion", companion.Alignment.Replace(' ', '\xA0'));
				saver.WriteTableCell("companion", companion.Strength);
				saver.WriteTableCell("companion", companion.Dexterity);
				saver.WriteTableCell("companion", companion.Constitution);
				saver.WriteTableCell("companion", companion.Intelligence);
				saver.WriteTableCell("companion", companion.Wisdom);
				saver.WriteTableCell("companion", companion.Charisma);
				saver.WriteTableRowEnd();
				if (companion.Description != null)
				{
					saver.WriteTableRowStart();
					saver.WriteTableCell(null, 9, [Paragraph.FromText(companion.Description)]);
					saver.WriteTableRowEnd();
				}
			}

			saver.WriteTableEnd();
		}

		private void EmitEnemies(Saver saver)
		{
			if (this.enemies.Count == 0)
			{
				return;
			}

			saver.WriteHeader(2, "Enemies");
			saver.WriteParagraph(new Paragraph("single")
			{
				new StylizedText("bold", "Fixed"),
				":\xA0" + string.Join(", ", this.enemies)
			});
			saver.WriteParagraph(new Paragraph("single")
			{
				new StylizedText("bold", "Spawning"),
				":\xA0"
			});
		}

		private void EmitTreasures(Saver saver)
		{
			if (this.treasures.Count == 0)
			{
				return;
			}

			saver.WriteHeader(2, "Notable Treasure");
			foreach (var treasure in this.treasures)
			{
				var para = new Paragraph("single");
				if (treasure.Key.Length > 0)
				{
					para.Add(StylizedText.StylizeLocations("bold", treasure.Key));
					para.Add(new StylizedText(": "));
				}

				var list = string.Join(", ", treasure.Value);
				para.Add(StylizedText.StylizeLocations(list));

				saver.WriteParagraph(para);
			}
		}
		#endregion
	}
}
