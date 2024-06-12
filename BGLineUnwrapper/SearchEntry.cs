namespace BGLineUnwrapper
{
	#region Private Classes
	internal sealed class SearchEntry(string search, int offset, LineType lineType, bool lineStart)
	{
		public bool LineStart { get; } = lineStart;

		public LineType LineType { get; } = lineType;

		public int Offset { get; } = offset;

		public string Search { get; } = search;
	}
	#endregion
}
