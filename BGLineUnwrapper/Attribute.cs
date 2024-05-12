namespace LineUnwrapper
{
	public record struct Attribute(string? Key, string? Value)
	{
		public static implicit operator Attribute((string? Key, string? Value) value) => new(value.Key, value.Value);
	}
}
