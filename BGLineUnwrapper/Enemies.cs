namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using RobinHood70.CommonCode;

	internal sealed class Enemies : Region
	{
		#region Public Constants
		public const string Key = "Enemies";
		#endregion

		#region Fields
		private readonly SortedSet<string> enemies;
		#endregion

		#region Constructors
		public Enemies(string body)
		{
			var lines = this.TextToLines(Common.TrimStart(body), LineType.Plain);
			if (lines.Count != 1)
			{
				throw new InvalidOperationException("Malformed Enemies section!");
			}

			var split = lines[0].Text.Split(TextArrays.CommaSpace, StringSplitOptions.None);
			this.enemies = new(split, StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region Public Properties
		public override string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Enemies Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public override void Save(Saver saver)
		{
			if (this.enemies.Count == 0)
			{
				return;
			}

			saver.WriteHeader(2, "Enemies");
			saver.WriteParagraph(new StylizedParagraph("single")
			{
				new StylizedText("bold", "Fixed"),
				":\xA0" + string.Join(", ", this.enemies)
			});
			saver.WriteParagraph(new StylizedParagraph("single")
			{
				new StylizedText("bold", "Spawning"),
				":\xA0"
			});
		}
		#endregion
	}
}
