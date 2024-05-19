namespace BGLineUnwrapper
{
	public class Attribute(string? key, string? value)
	{
		#region Public Properties
		public string? Key => key;

		public string? Value { get; } =
			value is not null &&
			value.Length > 1 &&
			((value[0] == '\'' && value[^1] == '\'') ||
			(value[0] == '\"' && value[^1] == '\"'))
				? value[1..^1]
				: value;
		#endregion

		#region Implicit Operators
		public static implicit operator Attribute((string? Key, string? Value) value) => new(value.Key, value.Value);
		#endregion
	}
}
