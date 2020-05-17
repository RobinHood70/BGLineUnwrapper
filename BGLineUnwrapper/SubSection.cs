namespace LineUnwrapper
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	public class Subsection
	{
		public Subsection(IEnumerable<Line> lines)
		{
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

		public IList<Line> Lines { get; } = new List<Line>();

		public Line Title { get; set; }

		public void ReparseLocations()
		{
			if (this.Lines.Count == 0)
			{
				return;
			}

			var lineNum = this.Lines.Count - 1;
			var searchLine = this.Lines[lineNum];
			while (searchLine.Type == LineType.Colon)
			{
				if (searchLine.Text.StartsWith("[", StringComparison.Ordinal) && searchLine.Text.EndsWith("]", StringComparison.Ordinal))
				{
					foreach (var line in this.Lines)
					{
						if (line.Type == LineType.Plain)
						{
							var index = line.Text.IndexOf(searchLine.Prefix, StringComparison.OrdinalIgnoreCase);
							if (index > -1)
							{
								line.Text = line.Text.Insert(index + searchLine.Prefix.Length, searchLine.Text);
								this.Lines.RemoveAt(lineNum);
								break;
							}
						}
					}
				}

				lineNum--;
				if (lineNum < 0)
				{
					break;
				}

				searchLine = this.Lines[lineNum];
			}

			foreach (var line in this.Lines)
			{
				if (line.Type == LineType.Colon)
				{
					Debug.WriteLine(line);
				}
			}
		}

		public void TrimAreaName(string areaName)
		{
			this.Title.TrimAreaName(areaName);
			foreach (var line in this.Lines)
			{
				line.TrimAreaName(areaName);
			}
		}

		/*
		public IEnumerator<Line> GetEnumerator()
		{
			yield return this.Title;
			foreach (var line in this.Lines)
			{
				yield return line;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException(); */
	}
}
