namespace LineUnwrapper
{
	internal sealed class StylizedText(string? style, string text)
	{
		#region Constructors
		public StylizedText(string text)
			: this(null, text)
		{
		}
		#endregion

		#region Public Properties
		public string? Style { get; set; } = style;

		public string Text { get; set; } = text;
		#endregion

		#region Public Methods
		public void Deconstruct(out string? style, out string text)
		{
			style = this.Style;
			text = this.Text;
		}
		#endregion

		#region Public Override Methods
		public override string ToString() => this.Style == null ? this.Text : $"<{this.Style}>{this.Text}</{this.Style}>";
		#endregion
	}
}
