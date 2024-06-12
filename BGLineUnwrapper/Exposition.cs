namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class Exposition : Region, ISubsectioned
	{
		#region Public Constants
		public const string Key = "Exposition";
		#endregion

		#region Constructors
		public Exposition(string body)
		{
			this.Subsections = this.ParseSubsections(body, true);
		}
		#endregion

		#region Public Properties
		public override string InstanceKey => Key;

		public IReadOnlyList<Subsection> Subsections { get; }
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
