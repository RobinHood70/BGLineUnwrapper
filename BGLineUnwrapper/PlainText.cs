namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class PlainText : Region, ISubsectioned
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
			this.subsections.AddRange(this.ParseSubsections(body, false));
		}
		#endregion

		#region Public Properties
		public override string InstanceKey => Key;

		public IReadOnlyList<Subsection> Subsections => this.subsections;
		#endregion

		#region Public Static Methods
		public static PlainText Create(string body) => new(body);

		// Does not register itself since it should never have a key match.
		public static void Register(BGDom dom) => _ = dom;
		#endregion

		#region Public Methods
		public override void Save(Saver saver) => saver.EmitSubsections(null, this.Subsections);
		#endregion
	}
}
