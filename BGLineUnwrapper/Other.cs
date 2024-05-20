namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class Other : ITextRegion, ISubsectioned
	{
		#region Public Constants
		public const string Key = "Other";
		#endregion

		#region Fields
		private readonly List<Subsection> subsections = [];
		#endregion

		#region Constructors
		public Other(string body)
		{
			this.subsections.AddRange(Common.ParseSubsections(body, true));
		}
		#endregion

		#region Public Properties
		public IReadOnlyList<Subsection> Subsections => this.subsections;

		public string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Other Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public void Save(Saver saver) => saver.EmitSubsections(Key, this.subsections);
		#endregion
	}
}
