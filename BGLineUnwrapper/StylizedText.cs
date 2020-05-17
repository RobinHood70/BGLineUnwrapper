namespace LineUnwrapper
{
	internal class StylizedText
	{
		#region Constructors
		public StylizedText(string text)
			: this(null, text)
		{
		}

		public StylizedText(string style, string text)
		{
			this.Style = style;
			this.Text = text;
		}
		#endregion

		#region Public Properties
		public string Style { get; set; }

		public string Text { get; set; }
		#endregion

		#region Public Methods
		public void Deconstruct(out string style, out string text)
		{
			style = this.Style;
			text = this.Text;
		}
		#endregion
	}
}
