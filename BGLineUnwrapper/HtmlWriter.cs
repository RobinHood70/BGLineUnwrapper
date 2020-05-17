namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;

	internal class HtmlWriter
	{
		#region Private Constants
		private const string IndentText = "\t";
		#endregion

		#region Fields
		private readonly Stack<(string Tag, bool NewLine)> tags = new Stack<(string, bool)>();
		private readonly StreamWriter writer;
		private int indent = 0;
		private bool atStartOfLine = true;
		#endregion

		#region Constructors
		public HtmlWriter(StreamWriter writer) => this.writer = writer;

		public HtmlWriter(StreamWriter writer, string defaultNamespace)
			: this(writer) => this.DefaultNamespace = defaultNamespace;
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
		public string DefaultNamespace { get; set; }
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

		public HtmlWriter OpenTag(string name, params (string Key, string Value)[] attributes) => this.FullTag(name, TagProperties.Indent | TagProperties.NewLineAfter, attributes);

		public HtmlWriter OpenTagInline(string name, params (string Key, string Value)[] attributes) => this.FullTag(name, TagProperties.None, attributes);

		public HtmlWriter OpenTextTag(string name, params (string Key, string Value)[] attributes) => this.FullTag(name, TagProperties.Indent, attributes);

		public HtmlWriter OpenTextTag(string name, IEnumerable<(string Key, string Value)> attributes) => this.FullTag(name, TagProperties.Indent, attributes);

		public HtmlWriter SelfClosingTagInline(string name, params (string Key, string Value)[] attributes) => this.FullTag(name, TagProperties.SelfClose, attributes);

		public HtmlWriter SelfClosingTag(string name, params (string Key, string Value)[] attributes) => this.FullTag(name, TagProperties.Indent | TagProperties.NewLineAfter | TagProperties.SelfClose, attributes);

		public HtmlWriter TextTag(string name, string content, params (string Key, string Value)[] attributes) => this
			.OpenTextTag(name, attributes)
			.WriteText(content)
			.CloseTag();

		public HtmlWriter TextTagInline(string name, string content, params (string Key, string Value)[] attributes) => this
			.OpenTagInline(name, attributes)
			.WriteText(content)
			.CloseTag();

		public HtmlWriter Write(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				this.writer.Write(text);
				this.atStartOfLine = text.EndsWith(this.writer.NewLine, StringComparison.Ordinal);
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
				this.writer.Write(IndentText);
			}

			return this;
		}

		public HtmlWriter WriteIndentedLine(string text) => this
			.WriteIndent()
			.WriteLine(text);

		public HtmlWriter WriteLine() => this.WriteLine(null);

		public HtmlWriter WriteLine(string text)
		{
			this.writer.WriteLine(text);
			this.atStartOfLine = true;
			return this;
		}

		public HtmlWriter WriteText(string text) => this.Write(WebUtility.HtmlEncode(text));
		#endregion

		#region Private Methods
		private string AddNamespace(string text) =>
			text[0] == ':' ? text.Substring(1) :
			(this.DefaultNamespace == null || text.Contains(":")) ? text :
			this.DefaultNamespace + ':' + text;

		private HtmlWriter FullTag(string tag, TagProperties props, IEnumerable<(string, string)> attributes)
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
			this.writer.Write('<' + tag);
			if (attributes != null)
			{
				foreach (var (key, value) in attributes)
				{
					if (key != null && value != null)
					{
						var newKey = this.AddNamespace(key);
						var newValue = value[0] == '\'' || value[0] == '\"' ? value : '\"' + value + '\"';
						this.writer.Write($" {newKey}={newValue}");
					}
				}
			}

			if (props.HasFlag(TagProperties.SelfClose))
			{
				this.writer.Write(tag[0] == '?' ? "?>" : "/>");
			}
			else
			{
				this.tags.Push((tag, hierarchical));
				this.writer.Write('>');
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
