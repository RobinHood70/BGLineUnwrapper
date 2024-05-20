namespace BGLineUnwrapper
{
	internal sealed class Note : ITextRegion
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
			this.notes = new Subsection(null, Common.WrappedLines(trimmed, LineType.Note));
		}
		#endregion

		#region Public Properties
		public string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Note Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public void Save(Saver saver) => saver.EmitSubsection(this.notes);
		#endregion
	}
}
