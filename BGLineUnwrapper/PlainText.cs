namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using RobinHood70.CommonCode;

	internal sealed class PlainText : ITextRegion, ISubsectioned
	{
		#region Public Constants
		public const string Key = "PlainText";
		#endregion

		#region Fields
		private readonly List<Subsection> subsections = [];
		#endregion

		#region Constructors
		public PlainText(string body)
		{
			body = body.Trim();
			var textSections = GeneratedRegexes.PlainTextFinder().Split(body);
			if (textSections.Length == 0 || (textSections.Length == 1 && textSections[0].Length == 0))
			{
				throw new InvalidOperationException();
			}

			// This slightly odd loop construct handles both untitled text as well as titled within the same loop.
			for (var i = -1; i < textSections.Length; i += 2)
			{
				Line? title = null;
				var lines = new List<Line>();
				if (i > -1)
				{
					title = new Line(LineType.Title, textSections[i]);
				}

				if (textSections[i + 1].Length > 0)
				{
					foreach (var line in textSections[i + 1].Split(TextArrays.NewLineChars, StringSplitOptions.RemoveEmptyEntries))
					{
						lines.Add(new Line(LineType.Plain, line));
					}
				}

				this.subsections.Add(new Subsection(title, lines));
			}
		}
		#endregion

		#region Public Properties
		public IReadOnlyList<Subsection> Subsections => this.subsections;

		public string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static PlainText Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public void Save(Saver saver)
		{
			foreach (var subsection in this.subsections)
			{
				if (subsection.Title != null)
				{
					saver.WriteHeader(2, subsection.Title.Text);
				}

				saver.WriteBulletedListStart();
				foreach (var line in subsection.Lines)
				{
					saver.WriteBulletedListItem(line.Text);
				}

				saver.WriteBulletedListEnd();
			}
		}
		#endregion
	}
}
