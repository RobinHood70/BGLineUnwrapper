namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;

	internal sealed class Companions : ITextRegion
	{
		#region Public Constants
		public const string Key = "Companions";
		#endregion

		#region Fields
		private readonly List<Companion> companions = [];
		#endregion

		#region Constructors
		public Companions(string body)
		{
			var entries = body.Split(new[] { Common.EntryDelimiter }, StringSplitOptions.None);
			if (entries.Length > 1)
			{
				for (var i = 1; i < entries.Length; i++)
				{
					var entryText = entries[i].TrimEnd();
					var companionLines = Common.TrimStart(entryText);
					this.companions.Add(new Companion(companionLines));
				}
			}
		}
		#endregion

		#region Public Properties
		public string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Companions Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Override Methods
		public void Save(Saver saver)
		{
			if (this.companions.Count == 0)
			{
				return;
			}

			saver.WriteHeader(2, "Companions");
			saver.WriteTableStart("companions", 100);
			saver.WriteTableHeader(
				("Name", 31),
				("Race and Class", 31),
				("Alignment", 31),
				("S", 1),
				("D", 1),
				("C", 1),
				("I", 1),
				("W", 1),
				("Ch", 1));
			foreach (var companion in this.companions)
			{
				companion.Save(saver);
			}

			saver.WriteTableEnd();
		}
		#endregion
	}
}
