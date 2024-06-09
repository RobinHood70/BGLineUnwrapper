namespace BGLineUnwrapper
{
	#region Public Enumerations
	public enum LineType
	{
		Plain,
		Title,
		Colon,
		Dashed,
		Note,
		Tip,
	}
	#endregion

	public class Line(LineType lineType, string? prefix, string text)
	{
		#region Constructors
		public Line(LineType lineType, string text)
			: this(lineType, null, text)
		{
		}
		#endregion

		#region Public Properties
		public string? Prefix { get; } = prefix is null ? null : Common.HarmonizeSpacing(prefix);

		public string Text { get; set; } = Common.HarmonizeSpacing(text) ?? string.Empty;

		public LineType Type { get; set; } = lineType;
		#endregion

		#region Public Override Methods
		public override string ToString() => (this.Prefix == null ? string.Empty : this.Prefix + ": ") + this.Text;
		#endregion
	}
}