namespace LineUnwrapper
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

	public class Line
	{
		#region Static Fields
		private static readonly char[] Colon = new char[] { ':' };
		private static readonly char[] TrimChars = new char[] { ' ', ',', '.' };
		#endregion

		#region Public Constructors
		public Line(LineType lineType, string text)
		{
			text = Common.HarmonizeText(text);
			if (lineType == LineType.Plain && text.Split(Colon, 2) is var textSplit && textSplit.Length == 2)
			{
				switch (textSplit[0])
				{
					case "Note":
						lineType = LineType.Note;
						text = textSplit[1].TrimStart();
						break;
					case "Tip":
						lineType = LineType.Tip;
						text = textSplit[1].TrimStart();
						break;
				}
			}

			this.Type = lineType;
			this.Text = text;
		}

		public Line(LineType lineType, string prefix, string text)
		{
			text = Common.HarmonizeText(text);
			if (lineType == LineType.Colon && text.EndsWith("].", StringComparison.Ordinal))
			{
				text = text[0..^1];
			}

			this.Type = prefix switch
			{
				"Note" => LineType.Note,
				"Tip" => LineType.Tip,
				_ => lineType
			};
			this.Prefix = prefix;
			this.Text = text;
		}
		#endregion

		#region Public Properties
		public string Prefix { get; set; }

		public string Text { get; set; }

		public LineType Type { get; set; }
		#endregion

		#region Public Methods
		public void TrimAreaName(string areaName)
		{
			if (this.Type != LineType.Colon && this.Type != LineType.Title)
			{
				return;
			}

			if (this.Text.StartsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				this.Text = this.Text.Substring(areaName.Length).TrimStart(TrimChars);
			}
			else if (this.Text.EndsWith(areaName, StringComparison.OrdinalIgnoreCase))
			{
				this.Text = this.Text.Substring(0, this.Text.Length - areaName.Length).TrimEnd(TrimChars);
			}
			else
			{
				var parens = " (" + areaName + ")";
				if (this.Text.IndexOf(parens, StringComparison.OrdinalIgnoreCase) > -1)
				{
					this.Text = this.Text.Replace(parens, string.Empty);
				}
			}
		}
		#endregion

		#region Public Override Methods
		public override string ToString() => (this.Prefix == null ? string.Empty : this.Prefix + ": ") + this.Text;
		#endregion
	}
}