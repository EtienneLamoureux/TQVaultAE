namespace TQVaultAE.GUI.Automation;

/// <summary>
/// The names of the test characters and vaults automation may change (see Safety in AUTOMATION.md): its own
/// <c>claude-</c> files, and the neutral test data of the documentation captures (<c>player-test*</c>,
/// <c>player-mod*</c>, <c>vault-*</c>), whose names may show in a picture.
/// </summary>
internal static class AutomationTestNames
{
	private static readonly string[] Prefixes = ["claude-", "player-test", "player-mod", "vault-"];

	/// <summary>The accepted prefixes, for error messages.</summary>
	internal static string Accepted { get; } = string.Join(", ", Prefixes.Select(p => $"'{p}'"));

	/// <summary>The prefix of automation's own test tags: tags are shared by every character, so only these are changed.</summary>
	internal const string TagPrefix = "claude-";

	/// <summary>Whether a tag name is a test tag's (<see cref="TagPrefix"/>).</summary>
	internal static bool IsTestTag(string? name)
		=> name is not null && name.StartsWith(TagPrefix, StringComparison.OrdinalIgnoreCase);

	/// <summary>Whether a character or vault name is a test file's.</summary>
	internal static bool IsTestFile(string? name)
		=> name is not null && Prefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase));
}
