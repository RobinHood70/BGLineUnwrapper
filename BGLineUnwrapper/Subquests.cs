﻿namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal sealed class Subquests : Region, ISubsectioned
	{
		#region Public Constants
		public const string Key = "Subquests";
		#endregion

		#region Constructors
		public Subquests(string body)
		{
			this.Subsections = this.ParseSubsections(body, true);
		}
		#endregion

		#region Public Properties
		public IReadOnlyList<Subsection> Subsections { get; }

		public override string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Subquests Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public override void Save(Saver saver) => saver.EmitSubsections(Key, this.Subsections);
		#endregion
	}
}
