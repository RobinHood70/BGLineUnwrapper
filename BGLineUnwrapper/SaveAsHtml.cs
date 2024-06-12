namespace BGLineUnwrapper
{
	using System.Collections.Generic;
	using System.IO;
	using RobinHood70.CommonCode;

	internal sealed class SaveAsHtml(StreamWriter writer) : Saver
	{
		#region Fields
		private readonly HtmlWriter htmlWriter = new(writer);
		#endregion

		#region Public Override Methods
		public override void WriteBulletedListEnd() => this.htmlWriter.CloseTag();

		public override void WriteBulletedListItem(string text) => this.WriteTextTag("li", StylizedText.StylizeLocations(text));

		public override void WriteBulletedListStart() => this.htmlWriter.OpenTag("ul");

		public override void WriteHeader(int level, IEnumerable<StylizedText> text) => this.WriteTextTag($"h{level.ToStringInvariant()}", text);

		public override void WriteParagraph(StylizedParagraph paragraph) => this.WriteTextTag("p", paragraph, ("class", paragraph.Style));

		public override void WriteStylizedText(string? style, string text)
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

		public override void WriteTableCell(string? style, int mergeCount, IEnumerable<StylizedParagraph> paragraphs)
		{
			var attrs = new List<HtmlAttrib> { new("class", style) };
			if (mergeCount > 1)
			{
				attrs.Add(("colspan", mergeCount.ToStringInvariant()));
			}

			this.htmlWriter.OpenTextTag("td", attrs);
			if (paragraphs is not List<StylizedParagraph> newParas)
			{
				newParas = new List<StylizedParagraph>(paragraphs);
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

		public override void WriteTableEnd() => this.htmlWriter.CloseTag();

		public override void WriteTableHeader(params (string Title, int Width)[] titles)
		{
			this.htmlWriter.OpenTag("tr");
			foreach (var (title, _) in titles)
			{
				// CONSIDER: Handle width (currently _)?
				this.htmlWriter.TextTag("th", title);
			}

			this.htmlWriter.CloseTag();
		}

		public override void WriteTableRowEnd() => this.htmlWriter.CloseTag();

		public override void WriteTableRowStart() => this.htmlWriter.OpenTag("tr");

		public override void WriteTableStart(string type, int percentWidth)
		{
			var attributes = new List<HtmlAttrib>()
			{
				("class", type)
			};

			if (percentWidth != 0)
			{
				attributes.Add(("style", $"width:{percentWidth.ToStringInvariant()}%"));
			}

			this.htmlWriter.OpenTag("table", [.. attributes]);
		}
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
			.WriteIndentedLine(".bold { font-weight:bold; }")
			.WriteIndentedLine(".centernowrap { text-align:center; white-space:nowrap; }")
			.WriteIndentedLine(".location { font-size:smaller; font-family:Courier New; color:#808080; vertical-align:super; }")
			.WriteIndentedLine(".note { width:80%; margin:1em auto; padding:0.5em; border:2px solid #4169E1; margin-top:1em; background:#87CEEB; }")
			.WriteIndentedLine(".npc { font-weight:bold; color:#0000FF; }")
			.WriteIndentedLine(".right { text-align:right; }")
			.WriteIndentedLine(".tip { width:80%; margin:1em auto; padding:0.5em; border:2px solid #008000; margin-top:1em; background:#90EE90; }")
			.CloseTags(2)
			.OpenTag("body");

		protected override void Shutdown() => this.htmlWriter.CloseTags(-1);
		#endregion

		#region Private Methods
		private void WriteTextTag(string tag, IEnumerable<StylizedText> text, params HtmlAttrib[] attributes)
		{
			this.htmlWriter.OpenTextTag(tag, attributes);
			this.WriteStylizedText(text);
			this.htmlWriter.CloseTag();
		}
		#endregion
	}
}
