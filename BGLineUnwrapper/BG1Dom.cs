namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	internal sealed class BG1Dom : BGDom
	{
		public BG1Dom()
		{
			AssassinationAttempts.Register(this);
			BulletedText.Register(this);
			Companions.Register(this);
			Enemies.Register(this);
			Note.Register(this);
			Other.Register(this);
			Plot.Register(this);
			Subquests.Register(this);
			Treasures.Register(this);
		}

		public static BG1Dom FromFile(string fileName)
		{
			var text = File.ReadAllText(fileName, Encoding.UTF8); // Encoding.GetEncoding(1252)
			text = HarmonizeText(text);
			var split = GeneratedRegexes.SectionSplitter().Split(text);
			if (split.Length < 2)
			{
				throw new InvalidOperationException("No sections!");
			}

			var dom = new BG1Dom();
			var sections = new List<Section>();
			var i = 0;
			var introText = string.Empty;
			while (i < split.Length && !char.IsDigit(split[i][0]))
			{
				introText += split[i] + '\n';
				i++;
			}

			if (introText.Length > 0)
			{
				var title = new SectionTitle("0 Intro");
				var introSection = new BG1Section(title, introText, dom);
				sections.Add(introSection);
			}

			while (i < split.Length)
			{
				var title = new SectionTitle(split[i]);
				var section = new BG1Section(title, split[i + 1], dom);
				sections.Add(section);
				i += 2;
			}

			dom.AddSections(sections);
			return dom;
		}
	}
}
