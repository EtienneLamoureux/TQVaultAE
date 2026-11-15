namespace TQVaultAE.GUI.Automation;

/// <summary>
/// The names of the characters, vaults and tags automation may change. Debug builds, used by the developers' agents on
/// their own saves, take only the test ones (see Safety in AUTOMATION.md): the neutral test data (<c>player-test*</c>,
/// <c>player-mod*</c> characters, <c>vault-*</c> vaults, <c>test-*</c> tags), whose names may show in a picture. Release builds serve the user's own agent on the user's own files (documentation/mcp.md): every name is
/// accepted there, the operator confirms each change instead.
/// </summary>
internal static class AutomationTestNames
{
	private static readonly string[] Prefixes = ["player-test", "player-mod", "vault-"];

	/// <summary>The accepted prefixes, for error messages.</summary>
	internal static string Accepted { get; } = string.Join(", ", Prefixes.Select(p => $"'{p}'"));

	/// <summary>The prefix of automation's own test tags: tags are shared by every character, so only these are changed.</summary>
	internal const string TagPrefix = "test-";

	/// <summary>Whether automation only changes the test names: in Debug builds.</summary>
#if DEBUG
	internal const bool Enforced = true;
#else
	internal const bool Enforced = false;
#endif

	/// <summary>Whether automation may change a tag: a test tag's (<see cref="TagPrefix"/>) in Debug builds, any in Release builds.</summary>
	internal static bool IsTestTag(string? name)
		=> !Enforced || (name is not null && name.StartsWith(TagPrefix, StringComparison.OrdinalIgnoreCase));

	/// <summary>Whether automation may change a character or vault: a test file's in Debug builds, any in Release builds.</summary>
	internal static bool IsTestFile(string? name)
		=> !Enforced || (name is not null && Prefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase)));
}
