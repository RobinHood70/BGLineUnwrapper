namespace LineUnwrapper
{
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
		protected static Paragraph GetParagraph(string text)
		{
			var retval = new Paragraph();
			if (text != null)
			{
				var split = Common.LocFinder.Split(text);
				var isLoc = false;
				foreach (var line in split)
				{
					if (!string.IsNullOrEmpty(line))
					{
						retval.Add(new StylizedText(isLoc ? "location" : null, line));
					}

					isLoc = !isLoc;
				}
			}

			return retval;
		}
		#endregion

		#region Protected Methods
		protected void WriteTableCell(string style, string text) => this.WriteTableCell(null, 1, new[] { new Paragraph(style, text) });

		protected void WriteTableCell(string style, IEnumerable<Paragraph> paragraphs) => this.WriteTableCell(style, 1, paragraphs);

		protected void WriteHeader(int level, string text) => this.WriteHeader(level, new Paragraph(null, text));

		protected void WriteStylizedText(Paragraph paragraph)
		{
			var writeText = string.Empty;
			string lastStyle = null;
			foreach (var (style, text) in paragraph)
			{
				if (style == lastStyle)
				{
					writeText += text;
				}
				else
				{
					this.WriteStylizedText(lastStyle, writeText);
					lastStyle = style;
					writeText = text;
				}
			}

			this.WriteStylizedText(lastStyle, writeText);
		}

		protected void WriteTableCell(Paragraph paragraph) => this.WriteTableCell(null, 1, new[] { paragraph });
		#endregion

		#region Protected Abstract Methods
		protected abstract void Setup();

		protected abstract void Shutdown();

		protected abstract void WriteBulletedListEnd();

		protected abstract void WriteBulletedListItem(string text);

		protected abstract void WriteBulletedListStart();

		protected abstract void WriteHeader(int level, Paragraph paragraph);

		protected abstract void WriteParagraph(Paragraph paragraph);

		protected abstract void WriteStylizedText(string style, string text);

		protected abstract void WriteTableCell(string style, int mergeCount, IEnumerable<Paragraph> paragraphs);

		protected abstract void WriteTableEnd();

		protected abstract void WriteTableHeader(params (string Title, int Width)[] titles);

		protected abstract void WriteTableRowEnd();

		protected abstract void WriteTableRowStart();

		protected abstract void WriteTableStart(string style, int percentWidth);
		#endregion

		#region Private Methods
		private void EmitCompanions(IList<Companion> companions)
		{
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
					new StylizedText(companion.Name)
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
				this.WriteTableRowStart();
				this.WriteTableCell(null, 9, new[] { GetParagraph(companion.Description) });
				this.WriteTableRowEnd();
			}

			this.WriteTableEnd();
		}

		private void EmitEnemies(IList<string> enemies)
		{
			this.WriteHeader(2, "Enemies");

			var para = new Paragraph("single", ":\xA0" + string.Join(", ", enemies));
			para.Insert(0, new StylizedText("intro", "Fixed"));
			this.WriteParagraph(para);

			para = new Paragraph("single", ":\xA0");
			para.Insert(0, new StylizedText("intro", "Spawning"));
			this.WriteParagraph(para);
		}

		private void EmitPlainText(IList<Line> lines)
		{
			var needToOpen = true;
			var needToClose = false;
			foreach (var line in lines)
			{
				switch (line.Type)
				{
					case LineType.Title:
						if (needToClose)
						{
							this.WriteBulletedListEnd();
						}

						this.WriteHeader(2, line.Text);
						needToOpen = true;
						needToClose = true;
						break;
					default:
						if (needToOpen)
						{
							this.WriteBulletedListStart();
							needToOpen = false;
							needToClose = true;
						}

						this.WriteBulletedListItem(line.Text);
						break;
				}
			}

			if (needToClose)
			{
				this.WriteBulletedListEnd();
			}
		}

		private void EmitSection(Section section)
		{
			var stylizedText = GetParagraph(section.Title);
			if (section.Area != null)
			{
				stylizedText.Add(new StylizedText("\xA0"));
				stylizedText.Add(new StylizedText("area", "(" + section.Area + ")"));
			}

			this.WriteHeader(1, stylizedText);
			if (section.PlainText != null)
			{
				this.EmitPlainText(section.PlainText);
			}

			if (section.Notes != null)
			{
				this.EmitLines(section.Notes);
			}

			if (section.Companions.Count > 0)
			{
				this.EmitCompanions(section.Companions);
			}

			if (section.Enemies != null)
			{
				this.EmitEnemies(section.Enemies);
			}

			if (section.Assassinations.Count > 0)
			{
				this.EmitSubsections("Assassination Attempts", section.Assassinations);
			}

			if (section.Treasures.Count > 0)
			{
				this.EmitTreasures(section.Treasures);
			}

			if (section.Plot != null)
			{
				this.WriteHeader(2, "Plot");
				this.EmitLines(section.Plot);
			}

			if (section.Subquests != null)
			{
				this.EmitSubsections("Subquests", section.Subquests);
			}

			if (section.Other != null)
			{
				this.EmitSubsections("Other", section.Other);
			}
		}

		private void EmitSubsections(string title, IEnumerable<Subsection> subsections)
		{
			this.WriteHeader(2, title);
			foreach (var subsection in subsections)
			{
				this.WriteHeader(3, GetParagraph(subsection.Title.Text));
				this.EmitLines(subsection.Lines);
			}
		}

		private void EmitLines(IEnumerable<Line> lines)
		{
			foreach (var line in lines)
			{
				var para = GetParagraph(line.Text);
				switch (line.Type)
				{
					case LineType.Colon:
						para.Style = "single";
						para.Insert(0, new StylizedText("intro", line.Prefix));
						if (para[1].Style != "location")
						{
							para.Insert(1, new StylizedText(":\xA0"));
						}

						this.WriteParagraph(para);
						break;
					case LineType.Dashed:
						para.Style = "single";
						para.Insert(0, new StylizedText("bold", line.Prefix));
						para.Insert(1, new StylizedText("—"));
						this.WriteParagraph(para);
						break;
					case LineType.Note:
						para.Style = "note";
						this.WriteParagraph(para);
						break;
					case LineType.Tip:
						para.Style = "tip";
						this.WriteParagraph(para);
						break;
					case LineType.Title:
						this.WriteHeader(3, para);
						break;
					default:
						this.WriteParagraph(para);
						break;
				}
			}
		}

		private void EmitTreasures(IDictionary<string, ICollection<string>> treasures)
		{
			this.WriteHeader(2, "Notable Treasure");
			/*
			this.WriteTableStart("treasure", 0);
			this.WriteTableHeader(("Quest", 0), ("Item", 0));
			foreach (var treasure in treasures)
			{
				this.WriteTableRowStart();
				this.WriteTableCell(new Paragraph("single", treasure.Key));
				var itemList = new List<Paragraph>();
				foreach (var item in treasure.Value)
				{
					var para = GetParagraph(item);
					para.Style = "single";
					itemList.Add(para);
				}

				this.WriteTableCell(null, itemList);
				this.WriteTableRowEnd();
			}

			this.WriteTableEnd();
			*/

			foreach (var treasure in treasures)
			{
				var para = new Paragraph("single");
				if (treasure.Key.Length > 0)
				{
					var quest = GetParagraph(treasure.Key);
					foreach (var styleText in quest)
					{
						if (styleText.Style == null)
						{
							styleText.Style = "bold";
						}
					}

					para.Add(quest);
					para.Add(new StylizedText(": "));
				}

				foreach (var item in treasure.Value)
				{
					para.Add(GetParagraph(item));
					para.Add(new StylizedText(", "));
				}

				para.RemoveAt(para.Count - 1);
				this.WriteParagraph(para);
			}
		}
		#endregion
	}
}