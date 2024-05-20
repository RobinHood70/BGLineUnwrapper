namespace BGLineUnwrapper
{
	public delegate ITextRegion RegionCreator(string body);

	public interface ITextRegion
	{
		string InstanceKey { get; }

		void Save(Saver saver);
	}
}
