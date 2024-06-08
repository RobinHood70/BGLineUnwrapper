namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	internal sealed partial class BG2Dom : BGDom
	{
		public BG2Dom()
		{
			Exposition.Register(this);
			PlainText.Register(this);
		}

		public static BG2Dom FromFile(string fileName)
		{
			var text = File.ReadAllText(fileName, Encoding.UTF8); // Encoding.GetEncoding(1252)
			text = HarmonizeText(text);
			text = GeneratedRegexes.ArrowFixer().Replace(text, "→");
			text = GeneratedRegexes.LongDashFixer().Replace(text, "${before}—${after}");

			var split = GeneratedRegexes.SectionSplitter().Split(text);
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

			var dom = new BG2Dom();
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

					var section = new BG2Section(title, split[index + 1], dom);
					sections.Add(section);
				}

				index += 2;
			}

			dom.AddSections(sections);
			return dom;
		}
	}
}
