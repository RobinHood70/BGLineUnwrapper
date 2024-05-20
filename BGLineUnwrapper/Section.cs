namespace BGLineUnwrapper
{
	public abstract class Section(SectionTitle title)
	{
		#region Public Properties
		public SectionTitle Title => title;
		#endregion

		#region Public Abstract Methods
		public abstract void Save(Saver saver);
		#endregion

		#region Public Override Methods
		public override string ToString() => title.ToString();
		#endregion
	}
}