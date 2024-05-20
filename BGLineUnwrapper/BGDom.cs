namespace BGLineUnwrapper
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using RobinHood70.CommonCode;

	public class BGDom : IEnumerable<Section>
	{
		#region Fields
		private readonly List<Section> sections = [];
		#endregion

		#region Public Properties
		public IDictionary<string, RegionCreator> RegionCreators { get; } = new Dictionary<string, RegionCreator>(StringComparer.Ordinal);
		#endregion

		#region Protected Static Methods
		public static string HarmonizeText(string text)
		{
			ArgumentNullException.ThrowIfNull(text);
			text = text.Replace("\r", string.Empty, StringComparison.Ordinal);
			var lines = new List<string>(text.Split('\n'));
			if (lines[0][0] == '-')
			{
				lines.RemoveAt(0);
			}

			for (var lineNum = 0; lineNum < lines.Count; lineNum++)
			{
				var line = lines[lineNum];
				if (line.Length > 0)
				{
					var charNum = 0;
					while (charNum < line.Length && line[charNum] == '\t')
					{
						charNum++;
					}

					lines[lineNum] = new string(' ', 4 * charNum) + line[charNum..].TrimEnd();
					if (lines[lineNum].Contains('\t', StringComparison.Ordinal))
					{
						Debug.WriteLine($"Unexpected tab on line {lineNum.ToStringInvariant()}: {lines[lineNum]}");
					}
				}
			}

			return string.Join('\n', lines);
		}
		#endregion

		#region Public Methods
		public void AddSections(IEnumerable<Section> sections)
		{
			ArgumentNullException.ThrowIfNull(sections);
			this.sections.AddRange(sections);
		}

		public IEnumerator<Section> GetEnumerator() => ((IEnumerable<Section>)this.sections).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.sections.GetEnumerator();
		#endregion

		#region Internal Methods
		internal void Register(string key, RegionCreator creator) => this.RegionCreators[key] = creator;
		#endregion
	}
}
