namespace BGLineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using RobinHood70.CommonCode;

	internal sealed class Treasures : Region
	{
		#region Public Constants
		public const string Key = "Treasures";
		#endregion

		#region Static Fields
		private static readonly char[] OpenSquare = ['['];
		#endregion

		#region Fields
		private readonly SortedDictionary<string, List<string>> treasures = new(StringComparer.CurrentCulture);
		#endregion

		#region Constructors
		public Treasures(string body)
		{
			if (body.Contains("Star Sapphire"))
			{
			}

			var lines = Common.TrimStart(body);
			foreach (var line in lines)
			{
				var (quest, item) = SplitTreasure(line);
				if (!this.treasures.TryGetValue(quest, out var treasureList))
				{
					treasureList = [];
					this.treasures.Add(quest, treasureList);
				}

				treasureList.Add(Common.HarmonizeSpacing(item.UpperFirst(CultureInfo.CurrentCulture)));
			}
		}
		#endregion

		#region Public Properties
		public override string InstanceKey => Key;
		#endregion

		#region Public Static Methods
		public static Treasures Create(string body) => new(body);

		public static void Register(BGDom dom) => dom.Register(Key, Create);
		#endregion

		#region Public Methods
		public override void Save(Saver saver)
		{
			if (this.treasures.Count == 0)
			{
				return;
			}

			saver.WriteHeader(2, "Notable Treasure");
			foreach (var treasure in this.treasures)
			{
				var para = new StylizedParagraph("single");
				if (treasure.Key.Length > 0)
				{
					para.Add(StylizedText.StylizeLocations("bold", treasure.Key));
					para.Add(new StylizedText(": "));
				}

				var list = string.Join(", ", treasure.Value);
				para.Add(StylizedText.StylizeLocations(list));

				saver.WriteParagraph(para);
			}
		}
		#endregion

		#region Private Methods
		private static (string, string) SplitTreasure(string line)
		{
			var offset = line.LastIndexOf(" (", StringComparison.Ordinal);
			if (offset > -1 && line[offset..].Contains("diff:", StringComparison.OrdinalIgnoreCase))
			{
				offset = -1;
			}

			var item = offset == -1 ? line : line[..offset];
			var quest = offset == -1 ? string.Empty : line[(offset + 2)..];
			offset = quest.LastIndexOf(')');
			if (offset > -1)
			{
				quest = quest.Remove(offset, 1);
			}

			if (string.Equals(quest, item, StringComparison.OrdinalIgnoreCase))
			{
				quest = string.Empty;
			}

			var locSplit = item.Split(OpenSquare, 2);
			if (locSplit.Length == 2)
			{
				item = locSplit[0];
				quest += '[' + locSplit[1];
			}

			return (quest, item);
		}
		#endregion
	}
}
