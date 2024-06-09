namespace BGLineUnwrapper
{
	#region Private Classes
	internal sealed class SearchEntry(string search, int offset, LineType lineType)
	{
		public LineType LineType { get; } = lineType;

		public int Offset { get; } = offset;

		public string Search { get; } = search;
	}
	#endregion
}
