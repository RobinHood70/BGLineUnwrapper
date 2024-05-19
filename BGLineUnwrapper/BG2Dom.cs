namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	internal sealed class BG2Dom(IList<Section> sections) : BGDom(sections)
	{
		public static BG2Dom FromFile(string fileName)
		{
			var text = File.ReadAllText(fileName, Encoding.UTF8); // Encoding.GetEncoding(1252)
			text = HarmonizeText(text);
			var split = SectionSplitter().Split(text);
			if (split.Length < 2)
			{
				throw new InvalidOperationException("No sections!");
			}

			// Find section 1
			var index = 0;
			while (index < split.Length && split[index][0] != '1')
			{
				index++;
			}

			var sections = new List<Section>();
			while (index < split.Length)
			{
				var title = new SectionTitle(split[index]);
				if (title.Number.Length > 0)
				{
					if (title.Number[0] == 'A')
					{
						// Stop if we've hit Appendix A
						break;
					}

					var section = new BG2Section(title, split[index + 1]);
					sections.Add(section);
				}

				index += 2;
			}

			return new BG2Dom(sections);
		}
	}
}
