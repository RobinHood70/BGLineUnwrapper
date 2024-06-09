namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class Exposition : Region, ISubsectioned
	{
		#region Public Constants
		public const string Key = "Exposition";
		#endregion

		#region Fields
		private readonly List<Subsection> subsections = [];
		#endregion

		#region Constructors
		public Exposition(string body)
		{
			var subs = this.ParseSubsections(body, true);
			this.subsections.AddRange(subs);
		}
		#endregion

		#region Public Properties
		public override string InstanceKey => Key;

		public IReadOnlyList<Subsection> Subsections => this.subsections;
		#endregion

		#region Public Static Methods
		public static Exposition Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public override void Save(Saver saver) => saver.EmitSubsections(Key, this.Subsections);
		#endregion
	}
}
