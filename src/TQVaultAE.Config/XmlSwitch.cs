namespace TQVaultAE.Config;

/// <summary>
/// The on/off attributes of the elements of <c>UserConfig.xml</c>, written 0 / 1 like the other switches.
/// </summary>
internal static class XmlSwitch
{
	/// <summary>"1" for on, "0" for off.</summary>
	public static string Write(bool value) => value ? "1" : "0";

	/// <summary>"1" / "true" or "0" / "false" (any case, spaces ignored); <paramref name="fallback"/> for anything else.</summary>
	public static bool Parse(string? value, bool fallback)
		=> value?.Trim().ToUpperInvariant() switch
		{
			"1" or "TRUE" => true,
			"0" or "FALSE" => false,
			_ => fallback,
		};
}
