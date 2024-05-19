namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;

	internal sealed class HtmlWriter(StreamWriter writer, string? defaultNamespace)
	{
		#region Private Constants
		private const string IndentText = "\t";
		#endregion

		#region Fields
		private readonly Stack<(string Tag, bool NewLine)> tags = new();
		private int indent;
		private bool atStartOfLine = true;
		#endregion

		#region Constructors
		public HtmlWriter(StreamWriter writer)
			: this(writer, null)
		{
		}
		#endregion

		#region Private Enumerations
		private enum TagProperties
		{
			None = 0,
			Indent = 1,
			SelfClose = 1 << 1,
			NewLineAfter = 1 << 2,
		}
		#endregion

		#region Public Properties
		public string? DefaultNamespace { get; set; } = defaultNamespace;
		#endregion

		#region Public Methods
		public HtmlWriter CloseTag()
		{
			var (tag, hierarchical) = this.tags.Pop();
			tag = "</" + tag + ">";
			if (hierarchical)
			{
				this.indent--;
				return this.WriteIndentedLine(tag);
			}

			return this.Write(tag);
		}

		public HtmlWriter CloseTags(int count)
		{
			if (count < 0)
			{
				count = this.tags.Count;
			}
			else
			{
				if (count > this.tags.Count)
				{
					throw new InvalidOperationException("Tried to close more tags than are open.");
				}
			}

			for (var i = 0; i < count; i++)
			{
				this.CloseTag();
			}

			return this;
		}

		public HtmlWriter OpenTag(string name) => this.FullTag(name, TagProperties.Indent | TagProperties.NewLineAfter, null);

		public HtmlWriter OpenTag(string name, params Attribute[] attributes) => this.FullTag(name, TagProperties.Indent | TagProperties.NewLineAfter, attributes);

		public HtmlWriter OpenTagInline(string name, params Attribute[] attributes) => this.FullTag(name, TagProperties.None, attributes);

		public HtmlWriter OpenTextTag(string name, params Attribute[] attributes) => this.FullTag(name, TagProperties.Indent, attributes);

		public HtmlWriter OpenTextTag(string name, IEnumerable<Attribute> attributes) => this.FullTag(name, TagProperties.Indent, attributes);

		public HtmlWriter SelfClosingTagInline(string name) => this.FullTag(name, TagProperties.SelfClose, null);

		public HtmlWriter SelfClosingTagInline(string name, params Attribute[] attributes) => this.FullTag(name, TagProperties.SelfClose, attributes);

		public HtmlWriter SelfClosingTag(string name) => this.FullTag(name, TagProperties.Indent | TagProperties.NewLineAfter | TagProperties.SelfClose, null);

		public HtmlWriter SelfClosingTag(string name, params Attribute[] attributes) => this.FullTag(name, TagProperties.Indent | TagProperties.NewLineAfter | TagProperties.SelfClose, attributes);

		public HtmlWriter TextTag(string name, string content, params Attribute[] attributes) => this
			.OpenTextTag(name, attributes)
			.WriteText(content)
			.CloseTag();

		public HtmlWriter TextTagInline(string name, string content, params Attribute[] attributes) => this
			.OpenTagInline(name, attributes)
			.WriteText(content)
			.CloseTag();

		public HtmlWriter Write(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				writer.Write(text);
				this.atStartOfLine = text.EndsWith(writer.NewLine, StringComparison.Ordinal);
			}

			return this;
		}

		public HtmlWriter WriteIndent()
		{
			if (!this.atStartOfLine)
			{
				this.WriteLine();
			}

			for (var i = 0; i < this.indent; i++)
			{
				writer.Write(IndentText);
			}

			return this;
		}

		public HtmlWriter WriteIndentedLine(string text) => this
			.WriteIndent()
			.WriteLine(text);

		public HtmlWriter WriteLine() => this.WriteLine(null);

		public HtmlWriter WriteLine(string? text)
		{
			writer.WriteLine(text);
			this.atStartOfLine = true;
			return this;
		}

		public HtmlWriter WriteText(string text) => this.Write(WebUtility.HtmlEncode(text));
		#endregion

		#region Private Methods
		private string AddNamespace(string text) =>
			text[0] == ':' ? text[1..] :
			(this.DefaultNamespace == null || text.Contains(':', StringComparison.Ordinal)) ? text :
			this.DefaultNamespace + ':' + text;

		private HtmlWriter FullTag(string tag, TagProperties props, IEnumerable<Attribute>? attributes)
		{
			var hierarchical = false;
			if (props.HasFlag(TagProperties.Indent))
			{
				this.WriteIndent();
				if (props.HasFlag(TagProperties.NewLineAfter) && !props.HasFlag(TagProperties.SelfClose))
				{
					this.indent++;
					hierarchical = true;
				}
			}

			tag = this.AddNamespace(tag);
			writer.Write('<' + tag);
			if (attributes != null)
			{
				foreach (var attribute in attributes)
				{
					if (attribute.Key != null && attribute.Value != null)
					{
						var newKey = this.AddNamespace(attribute.Key);
						writer.Write($" {newKey}=\"{attribute.Value}\"");
					}
				}
			}

			if (props.HasFlag(TagProperties.SelfClose))
			{
				writer.Write(tag[0] == '?' ? "?>" : "/>");
			}
			else
			{
				this.tags.Push((tag, hierarchical));
				writer.Write('>');
			}

			if (props.HasFlag(TagProperties.NewLineAfter))
			{
				this.WriteLine();
			}

			return this;
		}
		#endregion
	}
}
