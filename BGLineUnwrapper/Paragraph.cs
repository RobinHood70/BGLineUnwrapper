namespace BGLineUnwrapper
{
	using System.Collections;
	using System.Collections.Generic;

	public sealed class Paragraph(string? style, IEnumerable<StylizedText> text) : IList<StylizedText>
	{
		#region Fields
		private readonly IList<StylizedText> text = text is IList<StylizedText> listText ? listText : new List<StylizedText>(text);
		#endregion

		#region Constructors
		public Paragraph(string? style)
			: this(style, new List<StylizedText>())
		{
		}

		public Paragraph(string? style, string text)
			: this(style, new List<StylizedText>() { new(text) })
		{
		}
		#endregion

		#region Public Properties
		public int Count => this.text.Count;

		public bool IsReadOnly => this.text.IsReadOnly;

		public string? Style { get; set; } = style;
		#endregion

		#region Public Indexers
		public StylizedText this[int index]
		{
			get => this.text[index];
			set => this.text[index] = value;
		}
		#endregion

		#region Public Static Methds
		public static Paragraph FromText(string text) => new(null, StylizedText.StylizeLocations(text));

		#endregion

		#region Public Methods
		public void Add(string text) => this.Add(new StylizedText(text));

		public void Add(StylizedText item)
		{
			if (this.text.Count > 0 && this.text[this.text.Count - 1] is var last && string.Equals(item.Style, last.Style, System.StringComparison.Ordinal))
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
			foreach (var stylizedText in texts)
			{
				this.Add(stylizedText);
			}
		}

		public void Clear() => this.text.Clear();

		public bool Contains(StylizedText item) => this.text.Contains(item);

		public void CopyTo(StylizedText[] array, int arrayIndex) => this.text.CopyTo(array, arrayIndex);

		public IEnumerator<StylizedText> GetEnumerator() => this.text.GetEnumerator();

		public int IndexOf(StylizedText item) => this.text.IndexOf(item);

		public void Insert(int index, StylizedText item) => this.text.Insert(index, item);

		public bool Remove(StylizedText item) => this.text.Remove(item);

		public void RemoveAt(int index) => this.text.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => this.text.GetEnumerator();
		#endregion
	}
}
