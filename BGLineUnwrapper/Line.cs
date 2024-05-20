namespace BGLineUnwrapper
{
	using System;

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
		#region Static Fields
		private static readonly char[] TrimChars = [' ', ',', '.'];
		#endregion

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

		#region Public Methods
		public void TrimAreaName(string areaName)
		{
			if (this.Type is not LineType.Colon and not LineType.Title)
			{
				return;
			}

			if (this.Text.StartsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				this.Text = this.Text[areaName.Length..].TrimStart(TrimChars);
			}
			else if (this.Text.EndsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				this.Text = this.Text[..^areaName.Length].TrimEnd(TrimChars);
			}
			else
			{
				var parens = " (" + areaName + ")";
				this.Text = this.Text.Replace(parens, string.Empty, StringComparison.Ordinal);
			}
		}
		#endregion

		#region Public Override Methods
		public override string ToString() => (this.Prefix == null ? string.Empty : this.Prefix + ": ") + this.Text;
		#endregion
	}
}