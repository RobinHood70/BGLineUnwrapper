namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;

	internal abstract class SaveAs
	{
		#region Public Properties
		public string BaseFont { get; set; } = "Calibri";

		public int BaseFontSize { get; set; } = 10;
		#endregion

		#region Public Methods
		public void Save(IEnumerable<Section> sections)
		{
			this.Setup();
			foreach (var section in sections)
			{
				this.EmitSection(section);
			}

			this.Shutdown();
		}
		#endregion

		#region Protected Static Methods
		protected static Paragraph GetParagraph(string text) => new(null, StylizeLocations(text));

		protected static IEnumerable<StylizedText> StylizeLocations(string text) => StylizeLocations(null, text);

		protected static IEnumerable<StylizedText> StylizeLocations(string? defaultStyle, string text)
		{
			if (text != null)
			{
				var split = Common.LocFinder().Split(text);
				var isLoc = false;
				foreach (var line in split)
				{
					if (!string.IsNullOrEmpty(line))
					{
						yield return new StylizedText(isLoc ? "location" : defaultStyle, line);
					}

					isLoc = !isLoc;
				}
			}
		}
		#endregion

		#region Protected Methods
		protected void WriteTableCell(string style, string text) => this.WriteTableCell(null, 1, [new Paragraph(style, text)]);

		protected void WriteTableCell(string style, IEnumerable<Paragraph> paragraphs) => this.WriteTableCell(style, 1, paragraphs);

		protected void WriteHeader(int level, string text) => this.WriteHeader(level, new Paragraph(null, text));

		protected void WriteStylizedText(IEnumerable<StylizedText> text)
		{
			var writeText = string.Empty;
			string? lastStyle = null;
			foreach (var styledText in text)
			{
				if (styledText.Style == lastStyle)
				{
					writeText += styledText.Text;
				}
				else
				{
					this.WriteStylizedText(lastStyle, writeText);
					lastStyle = styledText.Style;
					writeText = styledText.Text;
				}
			}

			this.WriteStylizedText(lastStyle, writeText);
		}

		protected void WriteTableCell(Paragraph paragraph) => this.WriteTableCell(null, 1, [paragraph]);
		#endregion

		#region Protected Abstract Methods
		protected abstract void Setup();

		protected abstract void Shutdown();

		protected abstract void WriteBulletedListEnd();

		protected abstract void WriteBulletedListItem(string text);

		protected abstract void WriteBulletedListStart();

		protected abstract void WriteHeader(int level, IEnumerable<StylizedText> text);

		protected abstract void WriteParagraph(Paragraph paragraph);

		protected abstract void WriteStylizedText(string? style, string text);

		protected abstract void WriteTableCell(string? style, int mergeCount, IEnumerable<Paragraph> paragraphs);

		protected abstract void WriteTableEnd();

		protected abstract void WriteTableHeader(params (string Title, int Width)[] titles);

		protected abstract void WriteTableRowEnd();

		protected abstract void WriteTableRowStart();

		protected abstract void WriteTableStart(string style, int percentWidth);
		#endregion

		#region Private Methods
		private void EmitCompanions(IList<Companion> companions)
		{
			if (companions.Count == 0)
			{
				return;
			}

			this.WriteHeader(2, "Companions");
			this.WriteTableStart("companions", 100);
			this.WriteTableHeader(
				("Name", 31),
				("Race and Class", 31),
				("Alignment", 31),
				("S", 1),
				("D", 1),
				("C", 1),
				("I", 1),
				("W", 1),
				("Ch", 1));
			foreach (var companion in companions)
			{
				this.WriteTableRowStart();
				var companionName = new Paragraph("companionname")
				{
					companion.Name
				};
				if (companion.Location != null)
				{
					companionName.Add(new StylizedText("location", companion.Location));
				}

				this.WriteTableCell(companionName);
				this.WriteTableCell("companion", companion.Race.Replace(" ", "\xA0") + " " + companion.Class.Replace(" ", "\xA0"));
				this.WriteTableCell("companion", companion.Alignment.Replace(" ", "\xA0"));
				this.WriteTableCell("companion", companion.Strength);
				this.WriteTableCell("companion", companion.Dexterity);
				this.WriteTableCell("companion", companion.Constitution);
				this.WriteTableCell("companion", companion.Intelligence);
				this.WriteTableCell("companion", companion.Wisdom);
				this.WriteTableCell("companion", companion.Charisma);
				this.WriteTableRowEnd();
				if (companion.Description != null)
				{
					this.WriteTableRowStart();
					this.WriteTableCell(null, 9, [GetParagraph(companion.Description)]);
					this.WriteTableRowEnd();
				}
			}

			this.WriteTableEnd();
		}

		private void EmitEnemies(IList<string> enemies)
		{
			if (enemies.Count == 0)
			{
				return;
			}

			this.WriteHeader(2, "Enemies");
			this.WriteParagraph(new Paragraph("single")
			{
				new StylizedText("bold", "Fixed"),
				":\xA0" + string.Join(", ", enemies)
			});
			this.WriteParagraph(new Paragraph("single")
			{
				new StylizedText("bold", "Spawning"),
				":\xA0"
			});
		}

		private void EmitPlainText(IEnumerable<Subsection> subsections)
		{
			foreach (var subsection in subsections)
			{
				if (subsection.Title != null)
				{
					this.WriteHeader(2, subsection.Title.Text);
				}

				this.WriteBulletedListStart();
				foreach (var line in subsection.Lines)
				{
					this.WriteBulletedListItem(line.Text);
				}

				this.WriteBulletedListEnd();
			}
		}

		private void EmitSection(Section section)
		{
			var stylizedText = new List<StylizedText>(StylizeLocations(section.Title));
			if (section.Area != null)
			{
				stylizedText.Add(new StylizedText("\xA0"));
				stylizedText.Add(new StylizedText("area", "(" + section.Area + ")"));
			}

			this.WriteHeader(1, stylizedText);
			this.EmitPlainText(section.PlainText);
			this.EmitLines(section.Notes?.Lines);
			this.EmitCompanions(section.Companions);
			this.EmitEnemies(section.Enemies);
			this.EmitSubsections("Assassination Attempts", section.Assassinations);
			this.EmitTreasures(section.Treasures);
			this.EmitSubsections("Plot", section.Plot);
			this.EmitSubsections("Subquests", section.Subquests);
			this.EmitSubsections("Other", section.Other);
		}

		private void EmitSubsections(string title, List<Subsection> subsections)
		{
			if (subsections.Count == 0)
			{
				return;
			}

			this.WriteHeader(2, title);
			foreach (var subsection in subsections)
			{
				if (subsection.Title != null)
				{
					this.WriteHeader(3, StylizeLocations(subsection.Title.Text));
				}

				this.EmitLines(subsection.Lines);
			}
		}

		private void EmitLines(List<Line>? lines)
		{
			if (lines is null || lines.Count == 0)
			{
				return;
			}

			foreach (var line in lines)
			{
				var text = StylizeLocations(line.Text);
				switch (line.Type)
				{
					case LineType.Colon:
						var output = new List<StylizedText>(text);
						var para = new Paragraph("single")
						{
							new StylizedText("bold", line.Prefix ?? throw new InvalidOperationException())
						};
						if (output.Count > 0 && output[0].Style != "location")
						{
							para.Add(new StylizedText(":\xA0"));
						}

						para.Add(output);
						this.WriteParagraph(para);
						break;
					case LineType.Dashed:
						this.WriteParagraph(new Paragraph("single")
						{
							new StylizedText("bold", line.Prefix ?? throw new InvalidOperationException()),
							"—",
							text
						});
						break;
					case LineType.Note:
						this.WriteParagraph(new Paragraph("note", text));
						break;
					case LineType.Plain:
						this.WriteParagraph(new Paragraph(null, text));
						break;
					case LineType.Tip:
						this.WriteParagraph(new Paragraph("tip", text));
						break;
					case LineType.Title:
						this.WriteHeader(3, text);
						break;
				}
			}
		}

		private void EmitTreasures(SortedDictionary<string, List<string>> treasures)
		{
			if (treasures.Count == 0)
			{
				return;
			}

			this.WriteHeader(2, "Notable Treasure");
			foreach (var treasure in treasures)
			{
				var para = new Paragraph("single");
				if (treasure.Key.Length > 0)
				{
					para.Add(StylizeLocations("bold", treasure.Key));
					para.Add(new StylizedText(": "));
				}

				var list = string.Join(", ", treasure.Value);
				para.Add(StylizeLocations(list));

				this.WriteParagraph(para);
			}
		}
		#endregion
	}
}