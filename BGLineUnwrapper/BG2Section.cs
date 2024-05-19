namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class BG2Section : Section
	{
		public BG2Section(SectionTitle sectionTitle, string body)
			: base(sectionTitle)
		{
		}

		#region Public Properties
		public List<Companion> Companions { get; } = [];

		public List<string> Enemies { get; } = [];

		public List<Subsection> Exposition { get; } = [];

		public Subsection? Notes { get; }

		public List<Subsection> Quests { get; } = [];

		public SortedDictionary<string, List<string>> Treasures { get; } = [];
		#endregion

		#region Public Override Methods
		public override void Save(Saver saver) => throw new System.NotImplementedException();
		#endregion
	}
}
