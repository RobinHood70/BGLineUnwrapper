namespace LineUnwrapper
{
	using System.Collections.Generic;
	using System.IO;

	internal class SaveAsHtml : SaveAs
	{
		#region Fields
		private readonly HtmlWriter htmlWriter;
		#endregion

		#region Constructors
		public SaveAsHtml(StreamWriter writer) => this.htmlWriter = new HtmlWriter(writer);
		#endregion

		#region Protected Override Methods
		protected override void Setup() => this.htmlWriter
			.OpenTag("html")
			.OpenTag("head")
			.OpenTag("style")
			.WriteIndentedLine("body { font-family:Calibri; }")
			.WriteIndentedLine("h1 { width:100%; border-top:1px double; border-bottom:1px double; background:#F5F5F5; }")
			.WriteIndentedLine("h2 { margin-block-end:0.25em; color:#FA8072; }")
			.WriteIndentedLine("h3 { margin-block-end:0.25em; color:#FFA07A; }")
			.WriteIndentedLine("p { margin-block-start:0; margin-block-end:0; text-align:justify; margin-bottom:12px; }")
			.WriteIndentedLine("p.single { margin-bottom:0px; }")
			.WriteIndentedLine("table.companions, table.treasure { border-style:double; border-collapse:collapse; border-spacing:10px; }")
			.WriteIndentedLine("table.companions th, table.treasure th { background:#00BFFF; border:1px solid; padding:0 0.25em; }")
			.WriteIndentedLine("table.companions td, table.treasure td { border:1px solid; padding:0.25em; }")
			.WriteIndentedLine("ul { list-style-type:disc; }")
			.WriteIndentedLine(".area { font-size:smaller; color:#696969; }")
			.WriteIndentedLine(".centernowrap { text-align:center; white-space:nowrap; }")
			.WriteIndentedLine(".intro { font-weight:bold; color:#4B0082; }")
			.WriteIndentedLine(".location { font-size:smaller; font-family:Courier New; color:#808080; vertical-align:super; }")
			.WriteIndentedLine(".note { width:80%; margin:1em auto; padding:0.5em; border:2px solid #4169E1; margin-top:1em; background:#87CEEB; }")
			.WriteIndentedLine(".npc { font-weight:bold; color:#0000FF; }")
			.WriteIndentedLine(".right { text-align:right; }")
			.WriteIndentedLine(".tip { width:80%; margin:1em auto; padding:0.5em; border:2px solid #008000; margin-top:1em; background:#90EE90; }")
			.CloseTags(2)
			.OpenTag("body");

		protected override void Shutdown() => this.htmlWriter.CloseTags(-1);

		protected override void WriteBulletedListEnd() => this.htmlWriter.CloseTag();

		protected override void WriteBulletedListItem(string text) => this.WriteTextTag("li", text);

		protected override void WriteBulletedListStart() => this.htmlWriter.OpenTag("ul");

		protected override void WriteHeader(int level, Paragraph paragraph) => this.WriteTextTag("h" + level.ToString(), paragraph);

		protected override void WriteParagraph(Paragraph paragraph) => this.WriteTextTag("p", paragraph, ("class", paragraph.Style));

		protected override void WriteStylizedText(string style, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			if (style == null)
			{
				this.htmlWriter.WriteText(text);
			}
			else
			{
				this.htmlWriter.TextTagInline("span", text, ("class", style));
			}
		}

		protected override void WriteTableEnd() => this.htmlWriter.CloseTag();

		protected override void WriteTableHeader(params (string Title, int Width)[] titles)
		{
			this.htmlWriter.OpenTag("tr");
			foreach (var (title, _) in titles)
			{
				// CONSIDER: Handle width (currently _)?
				this.htmlWriter.TextTag("th", title);
			}

			this.htmlWriter.CloseTag();
		}

		protected override void WriteTableCell(string style, int mergeCount, IEnumerable<Paragraph> paragraphs)
		{
			var attrs = new List<(string, string)> { ("class", style) };
			if (mergeCount > 1)
			{
				attrs.Add(("colspan", mergeCount.ToString()));
			}

			this.htmlWriter.OpenTextTag("td", attrs);
			if (!(paragraphs is List<Paragraph> newParas))
			{
				newParas = new List<Paragraph>(paragraphs);
			}

			if (newParas.Count == 1)
			{
				this.WriteStylizedText(newParas[0]);
			}
			else
			{
				foreach (var para in paragraphs)
				{
					this.WriteParagraph(para);
				}
			}

			this.htmlWriter.CloseTag();
		}

		protected override void WriteTableRowEnd() => this.htmlWriter.CloseTag();

		protected override void WriteTableRowStart() => this.htmlWriter.OpenTag("tr");

		protected override void WriteTableStart(string type, int percentWidth)
		{
			var attributes = new List<(string, string)>()
			{
				("class", type)
			};

			if (percentWidth != 0)
			{
				attributes.Add(("style", $"width:{percentWidth}%"));
			}

			this.htmlWriter.OpenTag("table", attributes.ToArray());
		}
		#endregion

		#region Private Methods
		private void WriteTextTag(string tag, string text, params (string Key, string Value)[] attributes) => this.WriteTextTag(tag, GetParagraph(text), attributes);

		private void WriteTextTag(string tag, Paragraph paragraph, params (string Key, string Value)[] attributes)
		{
			this.htmlWriter.OpenTextTag(tag, attributes);
			this.WriteStylizedText(paragraph);
			this.htmlWriter.CloseTag();
		}
		#endregion
	}
}
