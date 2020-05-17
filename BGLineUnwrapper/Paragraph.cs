namespace LineUnwrapper
{
	using System.Collections;
	using System.Collections.Generic;

	internal class Paragraph : IList<StylizedText>
	{
		#region Fields
		private readonly IList<StylizedText> text;
		#endregion

		#region Constructors
		public Paragraph(params StylizedText[] text)
			: this(new List<StylizedText>(text))
		{
		}

		public Paragraph(IList<StylizedText> text) => this.text = text is IList<StylizedText> listText ? listText : new List<StylizedText>(text);

		public Paragraph(string style, string text)
			: this(style, new[] { new StylizedText(text) })
		{
		}

		public Paragraph(string style, params StylizedText[] text)
			: this(style, new List<StylizedText>(text))
		{
		}

		public Paragraph(string style, IList<StylizedText> text)
			: this(text) => this.Style = style;
		#endregion

		#region Public Properties
		public int Count => this.text.Count;

		public bool IsReadOnly => this.text.IsReadOnly;

		public string Style { get; set; }
		#endregion

		#region Public Indexers
		public StylizedText this[int index]
		{
			get => this.text[index];
			set => this.text[index] = value;
		}
		#endregion

		#region Public Methods
		public void Add(StylizedText item) => this.text.Add(item);

		public void Add(IEnumerable<StylizedText> texts)
		{
			foreach (var text in texts)
			{
				this.Add(text);
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
