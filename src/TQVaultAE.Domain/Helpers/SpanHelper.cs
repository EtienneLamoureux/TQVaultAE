namespace TQVaultAE.Domain.Helpers;

/// <summary>
/// Span-based string helpers that build a string in one allocation.
/// </summary>
public static class SpanHelper
{
	/// <summary>
	/// Creates a new string by concatenating a prefix with a sliced portion of the input span.
	/// </summary>
	public static string ConcatWithSlice(ReadOnlySpan<char> prefix, ReadOnlySpan<char> span, int skipPrefixChars)
	{
		int suffixLength = span.Length - skipPrefixChars;
		var result = new char[prefix.Length + suffixLength];
		prefix.CopyTo(result);
		span.Slice(skipPrefixChars).CopyTo(result.AsSpan(prefix.Length));
		return new string(result);
	}

	/// <summary>
	/// Creates a new string by concatenating a prefix with a middle portion of the input span.
	/// </summary>
	public static string ConcatWithSlice(ReadOnlySpan<char> prefix, ReadOnlySpan<char> span, int skipPrefixChars, int suffixLength)
	{
		var result = new char[prefix.Length + suffixLength];
		prefix.CopyTo(result);
		span.Slice(skipPrefixChars, suffixLength).CopyTo(result.AsSpan(prefix.Length));
		return new string(result);
	}
}
