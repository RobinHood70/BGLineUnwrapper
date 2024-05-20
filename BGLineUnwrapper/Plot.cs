namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class Plot : ITextRegion, ISubsectioned
	{
		#region Public Constants
		public const string Key = "Plot";
		#endregion

		#region Fields
		private readonly List<Subsection> subsections = [];
		#endregion

		#region Constructors
		public Plot(string body)
		{
			this.subsections.AddRange(Common.ParseSubsections(body, false));
		}
		#endregion

		#region Public Properties
		public IReadOnlyList<Subsection> Subsections => this.subsections;

		public string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Plot Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public void Save(Saver saver) => saver.EmitSubsections(Key, this.Subsections);
		#endregion
	}
}
