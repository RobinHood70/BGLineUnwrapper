namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	internal sealed class BG1Dom(IList<Section> sections) : BGDom(sections)
	{
		public static BG1Dom FromFile(string fileName)
		{
			var text = File.ReadAllText(fileName, Encoding.UTF8); // Encoding.GetEncoding(1252)
			text = HarmonizeText(text);
			var split = SectionSplitter().Split(text);
			if (split.Length < 2)
			{
				throw new InvalidOperationException("No sections!");
			}

			var i = 0;
			while (i < split.Length && !char.IsDigit(split[i][0]))
			{
				i++;
			}

			var sections = new List<Section>();
			while (i < split.Length)
			{
				var title = new SectionTitle(split[i]);
				var section = new BG1Section(title, split[i + 1]);
				sections.Add(section);
				i += 2;
			}

			return new BG1Dom(sections);
		}
	}
}
