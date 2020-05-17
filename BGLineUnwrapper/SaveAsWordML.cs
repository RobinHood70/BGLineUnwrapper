namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	internal class SaveAsWordML : SaveAs
	{
		#region Fields
		private readonly HtmlWriter htmlWriter;
		#endregion

		#region Constructors
		public SaveAsWordML(StreamWriter writer) => this.htmlWriter = new HtmlWriter(writer, "w");
		#endregion

		#region Protected Override Methods
		protected override void Setup()
		{
			this.htmlWriter
				.SelfClosingTag(":?xml", (":version", "1.0"), (":encoding", "UTF-8"), (":standalone", "yes"))
				.SelfClosingTag(":?mso-application", (":progid", "Word.Document"))
				.OpenTag("wordDocument", ("xmlns:w", "http://schemas.microsoft.com/office/word/2003/wordml"), ("xmlns:wx", "http://schemas.microsoft.com/office/word/2003/auxHint"), ("xmlns:o", "urn:schemas-microsoft-com:office:office"), ("macrosPresent", "no"), ("embeddedObjPresent", "no"), ("ocxPresent", "no"), ("xml:space", "preserve"));
			this.EmitFonts();
			this.EmitListDefs();
			this.EmitStyles();
			this.htmlWriter.OpenTag("body");
		}

		protected override void Shutdown() => this.htmlWriter.CloseTags(-1);

		protected override void WriteBulletedListEnd()
		{
		}

		protected override void WriteBulletedListItem(string text)
		{
			this.htmlWriter
				.OpenTag("p")
				.OpenTag("pPr")
				.OpenTag("listPr")
				.SelfClosingTag("ilvl", ("val", "0"))
				.SelfClosingTag("ilfo", ("val", "1"))
				.CloseTags(2);
			this.WriteStylizedText(GetParagraph(text));
			this.htmlWriter.CloseTag();
		}

		protected override void WriteBulletedListStart()
		{
		}

		protected override void WriteHeader(int level, Paragraph paragraph)
		{
			if (paragraph.Style != null)
			{
				throw new InvalidOperationException();
			}

			paragraph.Style = "heading" + level.ToString();
			this.WriteParagraph(paragraph);
		}

		protected override void WriteParagraph(Paragraph paragraph)
		{
			this.htmlWriter.OpenTag("p");
			if (!string.IsNullOrEmpty(paragraph.Style))
			{
				this.htmlWriter
					.OpenTag("pPr")
					.SelfClosingTag("pStyle", ("val", paragraph.Style))
					.CloseTag();
			}

			this.WriteStylizedText(paragraph);
			this.htmlWriter.CloseTag();
		}

		protected override void WriteStylizedText(string style, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			this.htmlWriter.OpenTag("r");
			if (style != null)
			{
				this.htmlWriter
					.OpenTag("rPr")
					.SelfClosingTag("rStyle", ("val", style));
				if (style == "location")
				{
					this.htmlWriter.SelfClosingTag("b", ("val", "off"));
				}

				this.htmlWriter.CloseTag();
			}

			this.htmlWriter
				.TextTag("t", text)
				.CloseTag();
		}

		protected override void WriteTableCell(string style, int mergeCount, IEnumerable<Paragraph> paragraphs)
		{
			this.htmlWriter.OpenTag("tc");
			if (mergeCount > 1)
			{
				this.htmlWriter
					.OpenTag("tcPr")
					.SelfClosingTag("hmerge", ("val", "restart"))
					.CloseTag();
			}

			foreach (var paragraph in paragraphs)
			{
				this.WriteParagraph(paragraph);
			}

			this.htmlWriter.CloseTag();
			if (mergeCount > 1)
			{
				for (var i = 2; i < mergeCount; i++)
				{
					this.htmlWriter
						.OpenTag("tc")
						.OpenTag("tcPr")
						.SelfClosingTag("hmerge", ("val", "continue"))
						.CloseTag()
						.SelfClosingTag("p")
						.CloseTag();
				}

				this.htmlWriter
					.OpenTag("tc")
					.OpenTag("tcPr")
					.SelfClosingTag("hmerge")
					.CloseTag()
					.SelfClosingTag("p")
					.CloseTag();
			}
		}

		protected override void WriteTableEnd() => this.htmlWriter.CloseTag();

		protected override void WriteTableHeader(params (string, int)[] titles)
		{
			this.WriteTableRowStart();
			foreach (var (title, _) in titles)
			{
				this.WriteTableCell(new Paragraph("title", title));
			}

			this.WriteTableRowEnd();
		}

		protected override void WriteTableRowEnd() => this.htmlWriter.CloseTag();

		protected override void WriteTableRowStart() => this.htmlWriter.OpenTag("tr");

		protected override void WriteTableStart(string style, int percentWidth) => this.htmlWriter
			.OpenTag("tbl")
			.OpenTag("tblPr")
			.SelfClosingTag("tblStyle", ("val", style))
			.SelfClosingTag("tblLook", ("val", "0020"))
			.SelfClosingTag("tblW", ("w", IntString(50 * percentWidth)), ("type", percentWidth == 0 ? "auto" : "pct"))
			.CloseTag();
		#endregion

		#region Private Static Methods
		private static string InchesToTwips(double inches) => IntString(inches * 1440);

		private static string IntString(double number) => ((int)Math.Round(number)).ToString();

		private static string PointsToEtips(double points) => IntString(points * 8);

		private static string PointsToTwips(double points) => IntString(points * 20);
		#endregion

		#region Private Methods
		private void EmitCharacterStyles() => this.htmlWriter
			.OpenTag("style", ("type", "character"), ("default", "on"), ("styleId", "DefaultParagraphFont"))
			.SelfClosingTag("name", ("val", "Default Paragraph Font"))
			.SelfClosingTag("semiHidden")
			.CloseTag()

			.OpenTag("style", ("type", "character"), ("styleId", "area"))
			.SelfClosingTag("name", ("val", "Area"))
			.OpenTag("rPr")
			.SelfClosingTag("color", ("val", "808080"))
			.SelfClosingTag("position", ("val", this.FontToWord((1.5 /* heading1 size */ - 1.25 /* this size */) / 2)))
			.SelfClosingTag("sz", ("val", this.FontToWord(1.25)))
			.CloseTags(2)

			.OpenTag("style", ("type", "character"), ("styleId", "bold"))
			.SelfClosingTag("name", ("val", "Emphasized"))
			.OpenTag("rPr")
			.SelfClosingTag("b", ("val", "on"))
			.CloseTags(2)

			.OpenTag("style", ("type", "character"), ("styleId", "location"))
			.SelfClosingTag("name", ("val", "Location"))
			.OpenTag("rPr")
			.SelfClosingTag("b", ("val", "off"))
			.SelfClosingTag("color", ("val", "808080"))
			.SelfClosingTag("vertAlign", ("val", "superscript"))
			.CloseTags(2);

		private void EmitFonts() => this.htmlWriter
			.OpenTag("fonts")
			.SelfClosingTag("defaultFonts", ("ascii", this.BaseFont), ("fareast", this.BaseFont), ("h-ansi", this.BaseFont))
			.OpenTag("font", ("name", this.BaseFont))
			.SelfClosingTag("panose-1", ("val", "020F0502020204030204"))
			.SelfClosingTag("charset", ("val", "00"))
			.SelfClosingTag("family", ("val", "Swiss"))
			.SelfClosingTag("pitch", ("val", "variable"))
			.SelfClosingTag("sig", ("usb-0", "E0002EFF"), ("usb-1", "C000247B"), ("usb-2", "00000009"), ("usb-3", "00000000"), ("csb-0", "000001FF"), ("csb-1", "00000000"))
			.CloseTags(2);

		private void EmitListDefs() => this.htmlWriter
			.OpenTag("lists")
			.OpenTag("listDef", ("listDefId", "0"))
			.SelfClosingTag("plt", ("val", "SingleLevel"))
			.OpenTag("lvl", ("ilvl", "0"))
			.SelfClosingTag("nfc", ("val", "23"))
			.SelfClosingTag("lvlText", ("val", "•"))
			.OpenTag("pPr")
			.SelfClosingTag("ind", ("left", InchesToTwips(0.25)), ("hanging", InchesToTwips(0.25)))
			.SelfClosingTag("contextualSpacing")
			.CloseTags(3)
			.OpenTag("list", ("ilfo", "1"))
			.SelfClosingTag("ilst", ("val", "0"))
			.CloseTags(2);

		private void EmitParagraphStyles() => this.htmlWriter
			.OpenTag("style", ("type", "paragraph"), ("styleId", "companion"))
			.SelfClosingTag("name", ("val", "Companion"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("jc", ("val", "center"))
			.SelfClosingTag("spacing", ("after", "0"))
			.CloseTag()
			.OpenTag("rPr")
			.SelfClosingTag("b", ("val", "on"))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "companionname"))
			.SelfClosingTag("name", ("val", "Companion Name"))
			.SelfClosingTag("basedOn", ("val", "companion"))
			.OpenTag("pPr")
			.SelfClosingTag("jc", ("val", "left"))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "heading1"))
			.SelfClosingTag("name", ("val", "heading 1"))
			.SelfClosingTag("wx:uiName", ("wx:val", "Heading 1"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("keepNext")
			.SelfClosingTag("pageBreakBefore")
			.SelfClosingTag("widowControl", ("val", "off"))
			.OpenTag("pBdr")
			.SelfClosingTag("top", ("val", "double"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.1)), ("space", this.FontScaled(.1)), ("color", "auto"))
			.SelfClosingTag("bottom", ("val", "double"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.1)), ("space", this.FontScaled(.1)), ("color", "auto"))
			.CloseTag()
			.SelfClosingTag("shd", ("val", "clear"), ("color", "auto"), ("fill", "F5F5F5"))
			.SelfClosingTag("spacing", ("after", this.FontToTwips(0.3)))
			.SelfClosingTag("outlineLvl", ("val", "1"))
			.CloseTag()
			.OpenTag("rPr")
			.SelfClosingTag("wx:font", ("wx:val", this.BaseFont))
			.SelfClosingTag("b")
			.SelfClosingTag("sz", ("val", this.FontToWord(1.5)))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "heading2"))
			.SelfClosingTag("name", ("val", "heading 2"))
			.SelfClosingTag("wx:uiName", ("wx:val", "Heading 2"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("keepNext")
			.SelfClosingTag("spacing", ("before", this.FontToTwips(1.2)), ("after", "0"))
			.SelfClosingTag("outlineLvl", ("val", "2"))
			.CloseTag()
			.OpenTag("rPr")
			.SelfClosingTag("wx:font", ("wx:val", this.BaseFont))
			.SelfClosingTag("color", ("val", "FA8072"))
			.SelfClosingTag("b")
			.SelfClosingTag("sz", ("val", this.FontToWord(1.2)))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "heading3"))
			.SelfClosingTag("name", ("val", "heading 3"))
			.SelfClosingTag("wx:uiName", ("wx:val", "Heading 3"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("keepNext")
			.SelfClosingTag("spacing", ("before", this.FontToTwips(0.7)), ("after", "0"))
			.SelfClosingTag("outlineLvl", ("val", "3"))
			.CloseTag()
			.OpenTag("rPr")
			.SelfClosingTag("wx:font", ("wx:val", this.BaseFont))
			.SelfClosingTag("color", ("val", "FFA07A"))
			.SelfClosingTag("b")
			.SelfClosingTag("sz", ("val", this.FontToWord(1.1)))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("default", "on"), ("styleId", "Normal"))
			.SelfClosingTag("name", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("spacing", ("after", this.FontToTwips(0.75)))
			.SelfClosingTag("jc", ("val", "both"))
			.CloseTag()
			.OpenTag("rPr")
			.SelfClosingTag("wx:font", ("wx:val", this.BaseFont))
			.SelfClosingTag("sz", ("val", this.FontToWord(1)))
			.SelfClosingTag("lang", ("val", "EN-CA"))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "note"))
			.SelfClosingTag("name", ("val", "Tip"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.OpenTag("pBdr")
			.SelfClosingTag("top", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.4)), ("color", "4169E1"))
			.SelfClosingTag("left", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.8)), ("color", "4169E1"))
			.SelfClosingTag("bottom", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.4)), ("color", "4169E1"))
			.SelfClosingTag("right", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.8)), ("color", "4169E1"))
			.CloseTag()
			.SelfClosingTag("shd", ("val", "clear"), ("color", "auto"), ("fill", "87CEEB"))
			.SelfClosingTag("ind", ("left", PointsToTwips(36)), ("right", PointsToTwips(36)))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "right"))
			.SelfClosingTag("name", ("val", "Right"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("jc", ("val", "right"))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "single"))
			.SelfClosingTag("name", ("val", "Single-Spaced"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("spacing", ("after", "0"))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "tip"))
			.SelfClosingTag("name", ("val", "Tip"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.OpenTag("pBdr")
			.SelfClosingTag("top", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.4)), ("color", "008000"))
			.SelfClosingTag("left", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.8)), ("color", "008000"))
			.SelfClosingTag("bottom", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.4)), ("color", "008000"))
			.SelfClosingTag("right", ("val", "single"), ("sz", this.FontToEtips(.1)), ("wx:bdrwidth", this.FontToTwips(.4)), ("space", this.FontScaled(.8)), ("color", "008000"))
			.CloseTag()
			.SelfClosingTag("shd", ("val", "clear"), ("color", "auto"), ("fill", "90EE90"))
			.SelfClosingTag("ind", ("left", PointsToTwips(36)), ("right", PointsToTwips(36)))
			.CloseTags(2)

			.OpenTag("style", ("type", "paragraph"), ("styleId", "title"))
			.SelfClosingTag("name", ("val", "Table Title"))
			.SelfClosingTag("basedOn", ("val", "Normal"))
			.OpenTag("pPr")
			.SelfClosingTag("jc", ("val", "center"))
			.SelfClosingTag("spacing", ("after", "0"))
			.CloseTags(2)
			;

		private void EmitStyles()
		{
			this.htmlWriter.OpenTag("styles");
			this.EmitParagraphStyles();
			this.EmitCharacterStyles();
			this.EmitTableStyles();
			this.htmlWriter.CloseTag();
		}

		private void EmitTableStyles() => this.htmlWriter
			.OpenTag("style", ("type", "table"), ("styleId", "companions"))
			.SelfClosingTag("name", ("val", "Companions"))
			.SelfClosingTag("basedOn", ("val", "TableNormal"))
			.OpenTag("rPr")
			.SelfClosingTag("rfonts", ("ascii", this.BaseFont), ("h-ansi", this.BaseFont))
			.SelfClosingTag("wx:font", ("wx:val", this.BaseFont))
			.CloseTag()
			.OpenTag("tblPr")
			.SelfClosingTag("tblStyleRowBandSize", ("val", "2"))
			.OpenTag("tblCellMar")
			.SelfClosingTag("left", ("w", this.FontToTwips(0.1)), ("type", "dxa"))
			.SelfClosingTag("right", ("w", this.FontToTwips(0.1)), ("type", "dxa"))
			.CloseTags(2)
			.OpenTag("trPr")
			.SelfClosingTag("cantSplit")
			.CloseTag()
			.OpenTag("tblStylePr", ("type", "firstRow"))
			.OpenTag("pPr")
			.SelfClosingTag("jc", ("val", "center"))
			.CloseTag()
			.OpenTag("rPr")
			.SelfClosingTag("b")
			.SelfClosingTag("color", ("val", "FFFFFF"))
			.CloseTag()
			.OpenTag("tcPr")
			.SelfClosingTag("shd", ("val", "clear"), ("color", "auto"), ("fill", "4472C4"))
			.CloseTags(2)
			.OpenTag("tblStylePr", ("type", "band2Horz"))
			.OpenTag("tcPr")
			.SelfClosingTag("shd", ("val", "clear"), ("color", "auto"), ("fill", "D9E2F3"))
			.CloseTags(3)

			.OpenTag("style", ("type", "table"), ("styleId", "treasure"))
			.SelfClosingTag("name", ("val", "Companions"))
			.SelfClosingTag("basedOn", ("val", "TableNormal"))
			.OpenTag("rPr")
			.SelfClosingTag("rfonts", ("ascii", this.BaseFont), ("h-ansi", this.BaseFont))
			.SelfClosingTag("wx:font", ("wx:val", this.BaseFont))
			.CloseTag()
			.OpenTag("tblPr")
			.SelfClosingTag("tblStyleRowBandSize", ("val", "1"))
			.OpenTag("tblCellMar")
			.SelfClosingTag("left", ("w", this.FontToTwips(0.25)), ("type", "dxa"))
			.SelfClosingTag("right", ("w", this.FontToTwips(0.25)), ("type", "dxa"))
			.CloseTags(2)
			.OpenTag("trPr")
			.SelfClosingTag("cantSplit")
			.CloseTag()
			.OpenTag("tblStylePr", ("type", "firstRow"))
			.OpenTag("pPr")
			.SelfClosingTag("jc", ("val", "center"))
			.CloseTag()
			.OpenTag("rPr")
			.SelfClosingTag("b")
			.SelfClosingTag("color", ("val", "FFFFFF"))
			.CloseTag()
			.OpenTag("tcPr")
			.SelfClosingTag("shd", ("val", "clear"), ("color", "auto"), ("fill", "4472C4"))
			.CloseTags(2)
			.OpenTag("tblStylePr", ("type", "band2Horz"))
			.OpenTag("tcPr")
			.SelfClosingTag("shd", ("val", "clear"), ("color", "auto"), ("fill", "D9E2F3"))
			.CloseTags(3);

		private string FontScaled(double multiplier) => IntString(this.BaseFontSize * multiplier);

		private string FontToEtips(double multiplier) => PointsToEtips(this.BaseFontSize * multiplier);

		private string FontToTwips(double multiplier) => PointsToTwips(this.BaseFontSize * multiplier);

		private string FontToWord(double multiplier) => this.FontScaled(2 * multiplier);
		#endregion
	}
}