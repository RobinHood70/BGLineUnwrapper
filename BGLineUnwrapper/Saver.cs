namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	public abstract class Saver
	{
		#region Public Properties
		public string BaseFont { get; set; } = "Calibri";

		public int BaseFontSize { get; set; } = 10;
		#endregion

		#region Public Methods
		public void EmitSubsection(Subsection subsection)
		{
			if (subsection.Title is not null)
			{
				this.WriteHeader(3, StylizedText.StylizeLocations(subsection.Title.Text));
			}

			this.EmitLines(subsection.Lines);
		}

		public void EmitSubsections(string? title, IReadOnlyCollection<Subsection>? subsections)
		{
			if (subsections is null || subsections.Count == 0)
			{
				return;
			}

			if (title is not null)
			{
				this.WriteHeader(2, title);
			}

			foreach (var subsection in subsections)
			{
				this.EmitSubsection(subsection);
			}
		}

		public void Save(BGDom dom)
		{
			this.Setup();
			foreach (var section in dom)
			{
				section.Save(this);
			}

			this.Shutdown();
		}

		public void WriteHeader(int level, string text) => this.WriteHeader(level, new StylizedParagraph(null, text));

		public void WriteStylizedText(IEnumerable<StylizedText> text)
		{
			var writeText = string.Empty;
			string? lastStyle = null;
			foreach (var styledText in text)
			{
				if (string.Equals(styledText.Style, lastStyle, StringComparison.Ordinal))
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

		public void WriteTableCell(StylizedParagraph paragraph) => this.WriteTableCell(null, 1, [paragraph]);

		public void WriteTableCell(string style, string text) => this.WriteTableCell(null, 1, [new StylizedParagraph(style, text)]);
		#endregion

		#region Public Abstract Methods

		public abstract void WriteBulletedListEnd();

		public abstract void WriteBulletedListItem(string text);

		public abstract void WriteBulletedListStart();

		public abstract void WriteHeader(int level, IEnumerable<StylizedText> text);

		public abstract void WriteParagraph(StylizedParagraph paragraph);

		public abstract void WriteStylizedText(string? style, string text);

		public abstract void WriteTableCell(string? style, int mergeCount, IEnumerable<StylizedParagraph> paragraphs);

		public abstract void WriteTableEnd();

		public abstract void WriteTableHeader(params (string Title, int Width)[] titles);

		public abstract void WriteTableRowEnd();

		public abstract void WriteTableRowStart();

		public abstract void WriteTableStart(string style, int percentWidth);
		#endregion

		#region Protected Abstract Methods
		protected abstract void Setup();

		protected abstract void Shutdown();
		#endregion

		#region Private Methods
		private void EmitLine(Line line)
		{
			var text = StylizedText.StylizeLocations(line.Text);
			switch (line.Type)
			{
				case LineType.Colon:
					var output = new List<StylizedText>(text);
					var para = new StylizedParagraph("single")
						{
							new StylizedText("bold", line.Prefix ?? throw new InvalidOperationException())
						};
					if (output.Count > 0 && !string.Equals(output[0].Style, "location", StringComparison.Ordinal))
					{
						para.Add(new StylizedText(":\xA0"));
					}

					para.Add(output);
					this.WriteParagraph(para);
					break;
				case LineType.Dashed:
					this.WriteParagraph(new StylizedParagraph("single")
						{
							new StylizedText("bold", line.Prefix ?? throw new InvalidOperationException()),
							"—",
							text
						});
					break;
				case LineType.Note:
					this.WriteParagraph(new StylizedParagraph("note", text));
					break;
				case LineType.Plain:
					this.WriteParagraph(new StylizedParagraph(null, text));
					break;
				case LineType.Tip:
					this.WriteParagraph(new StylizedParagraph("tip", text));
					break;
				case LineType.Title:
					this.WriteHeader(3, text);
					break;
				default:
					break;
			}
		}

		private void EmitLines(IReadOnlyCollection<Line>? lines)
		{
			if (lines is null || lines.Count == 0)
			{
				return;
			}

			foreach (var line in lines)
			{
				this.EmitLine(line);
			}
		}
		#endregion
	}
}