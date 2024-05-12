namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;

	public class Subsection
	{
		#region Constructors
		public Subsection(Line? title, List<Line> lines)
		{
			this.Title = title;
			this.Lines = lines;
		}

		public Subsection(List<Line> lines, bool checkForTitle)
		{
			if (checkForTitle)
			{
				this.Lines = [];
				foreach (var line in lines)
				{
					if (line.Type == LineType.Title)
					{
						this.Title = line;
					}
					else
					{
						this.Lines.Add(line);
					}
				}
			}
			else
			{
				this.Lines = lines;
			}
		}
		#endregion

		#region Public Properties
		public List<Line> Lines { get; }

		public Line? Title { get; set; }
		#endregion

		#region Public Methods
		public void TrimAreaName(string areaName)
		{
			this.Title?.TrimAreaName(areaName);
			foreach (var line in this.Lines)
			{
				line.TrimAreaName(areaName);
			}

			this.ReparseLocations();
		}
		#endregion

		#region Private Methods
		private void ReparseLocations()
		{
			if (this.Lines.Count == 0)
			{
				return;
			}

			var lineNum = this.Lines.Count - 1;
			while (lineNum > -1 && this.Lines[lineNum] is var searchLine && searchLine.Type == LineType.Colon)
			{
				var replaced = false;
				foreach (var line in this.Lines)
				{
					if (line.Type == LineType.Plain && this.MoveLocation(line, lineNum, searchLine))
					{
						replaced = true;
						break;
					}
				}

				if (!replaced)
				{
					// Try replacing in title as a last resort if not replaced in text. Output was too messy when replacing in title first.
					this.MoveLocation(this.Title, lineNum, searchLine);
				}

				lineNum--;
			}
		}

		private bool MoveLocation(Line? line, int lineNum, Line searchLine)
		{
			if (line != null && searchLine.Prefix != null)
			{
				var index = line.Text.IndexOf(searchLine.Prefix, StringComparison.OrdinalIgnoreCase);
				if (index > -1)
				{
					index += searchLine.Prefix.Length;
					while (index < line.Text.Length && !" \n".Contains(line.Text[index]))
					{
						index++;
					}

					var insertText = searchLine.Text
						.Replace("[", " ")
						.Replace("  ", " ")
						.Replace("]", string.Empty)
						.Trim();
					insertText = '[' + insertText + ']';
					line.Text = line.Text.Insert(index, insertText);
					this.Lines.RemoveAt(lineNum);
					return true;
				}
			}

			return false;
		}
		#endregion
	}
}
