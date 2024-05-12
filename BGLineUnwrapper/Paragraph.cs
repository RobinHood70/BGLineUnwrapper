namespace LineUnwrapper
{
	using System.Collections;
	using System.Collections.Generic;

	internal class Paragraph : IEnumerable<StylizedText>
	{
		#region Fields
		private readonly IList<StylizedText> text;
		#endregion

		#region Constructors
		public Paragraph(string? style)
			: this(style, new List<StylizedText>())
		{
		}

		public Paragraph(string? style, string text)
			: this(style, new[] { new StylizedText(text) })
		{
		}

		public Paragraph(string? style, IEnumerable<StylizedText> text)
		{
			this.text = new List<StylizedText>(text);
			this.Style = style;
		}
		#endregion

		#region Public Properties
		public string? Style { get; set; }
		#endregion

		#region Public Indexers
		public StylizedText this[int index]
		{
			get => this.text[index];
			set => this.text[index] = value;
		}
		#endregion

		#region Public Methods
		public void Add(string text) => this.Add(new StylizedText(text));

		public void Add(StylizedText item)
		{
			if (this.text.Count > 0 && this.text[this.text.Count - 1] is var last && item.Style == last.Style)
			{
				last.Text += item.Text;
			}
			else
			{
				this.text.Add(item);
			}
		}

		public void Add(IEnumerable<StylizedText> texts)
		{
			foreach (var text in texts)
			{
				this.Add(text);
			}
		}

		public IEnumerator<StylizedText> GetEnumerator() => this.text.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.text.GetEnumerator();
		#endregion
	}
}
