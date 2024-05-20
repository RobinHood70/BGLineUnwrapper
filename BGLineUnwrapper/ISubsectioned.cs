namespace BGLineUnwrapper
{
	using System.Collections.Generic;

	internal interface ISubsectioned
	{
		IReadOnlyList<Subsection> Subsections { get; }
	}
}
