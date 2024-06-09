namespace BGLineUnwrapper
{
	internal sealed class Note : Region
	{
		#region Public Constants
		public const string Key = "Note";
		#endregion

		#region Fields
		private readonly Subsection notes;
		#endregion

		#region Constructors
		public Note(string body)
		{
			var trimmed = Common.TrimStart(body);
			this.notes = new Subsection(null, this.TextToLines(trimmed, LineType.Note));
		}
		#endregion

		#region Public Properties
		public override string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Note Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public override void Save(Saver saver) => saver.EmitSubsection(this.notes);
		#endregion
	}
}
