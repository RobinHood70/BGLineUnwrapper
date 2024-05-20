namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class AssassinationAttempts : ITextRegion, ISubsectioned
	{
		#region Public Constants
		public const string Key = "Assassination Attempts";
		#endregion

		#region Fields
		private readonly List<Subsection> subsections = [];
		#endregion

		#region Constructors
		public AssassinationAttempts(string body)
		{
			this.subsections.AddRange(Common.ParseSubsections(body, true));
		}
		#endregion

		#region Public Properties
		public IReadOnlyList<Subsection> Subsections => this.subsections;

		public string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static AssassinationAttempts Create(string body) => new(body);

		public static void Register(BGDom dom)
		{
			dom.Register("Assassination Attempt", Create);
			dom.Register("Enemy Wizards", Create);
			dom.Register(Key, Create);
		}
		#endregion

		#region Public Methods
		public void Save(Saver saver) => saver.EmitSubsections("Assassination Attempts", this.Subsections);
		#endregion
	}
}
