//-----------------------------------------------------------------------
// <copyright file="InstallLocations.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace TQVaultAE.GameFiles;

/// <summary>
/// Where Steam and GOG install Titan Quest Anniversary Edition, read from the registry and from
/// Steam's libraryfolders.vdf. Shared by TQVault and tqdb.
/// </summary>
public static partial class InstallLocations
{
	/// <summary>Steam app id of Titan Quest Anniversary Edition.</summary>
	public const string SteamAppId = "475150";

	/// <summary>Folder of the game under steamapps\common.</summary>
	public const string SteamFolderName = "Titan Quest Anniversary Edition";

	/// <summary>GOG game id of Titan Quest Anniversary Edition.</summary>
	public const string GogGameId = "1196955511";

	/// <summary>
	/// Folders where Steam may have installed the game: <c>&lt;library&gt;\steamapps\common\Titan Quest Anniversary Edition</c>
	/// for the Steam folder and each library it lists. Empty when Steam is not installed or not on Windows.
	/// The folders are not checked.
	/// </summary>
	public static IEnumerable<string> SteamCandidates()
		=> OperatingSystem.IsWindows() ? SteamCandidates(ReadRegistry, ReadTextFile) : [];

	/// <summary>
	/// <see cref="SteamCandidates()"/> with the registry and the file reads given (tests).
	/// </summary>
	/// <param name="registry">Reads a registry string value, <see langword="null"/> when missing or empty.</param>
	/// <param name="readFile">Reads a text file, <see langword="null"/> when missing or unreadable.</param>
	internal static IEnumerable<string> SteamCandidates(RegistryReader registry, Func<string, string?> readFile)
	{
		var steam = SteamRoot(registry);
		if (steam is null)
			yield break;

		var libraries = new List<string> { steam };
		if (readFile(Path.Combine(steam, "steamapps", "libraryfolders.vdf")) is { } content)
			libraries.AddRange(ParseLibraryFolders(content));

		foreach (var lib in libraries.Distinct(StringComparer.OrdinalIgnoreCase))
			yield return Path.Combine(lib, "steamapps", "common", SteamFolderName);
	}

	/// <summary>Install folders that GOG registered for the game (64 and 32-bit registry views). Empty when not on Windows.</summary>
	public static IEnumerable<string> GogCandidates()
		=> OperatingSystem.IsWindows() ? GogCandidates(ReadRegistry) : [];

	/// <summary><see cref="GogCandidates()"/> with the registry reads given (tests).</summary>
	internal static IEnumerable<string> GogCandidates(RegistryReader registry)
	{
		foreach (var key in new[] { $"""SOFTWARE\GOG.com\Games\{GogGameId}""", $"""SOFTWARE\WOW6432Node\GOG.com\Games\{GogGameId}""" }) {
			var path = registry(RegistryRoot.LocalMachine, key, "PATH");
			if (path is not null)
				yield return path;
		}
	}

	/// <summary>Registry hive read by <see cref="RegistryReader"/>.</summary>
	internal enum RegistryRoot
	{
		CurrentUser,
		LocalMachine,
	}

	/// <summary>Reads a registry string value, <see langword="null"/> when missing, empty or not readable.</summary>
	internal delegate string? RegistryReader(RegistryRoot root, string subKey, string name);

	/// <summary>
	/// Library roots listed in a Steam libraryfolders.vdf, with backslashes unescaped: the <c>"path"</c> keys of
	/// the current format, or the <c>"1"		"D:\\games\\Steam"</c> entries of the old one.
	/// </summary>
	public static IReadOnlyList<string> ParseLibraryFolders(string vdf)
	{
		var result = new List<string>();
		foreach (Match m in VdfPath().Matches(vdf))
			result.Add(Unescape(m.Groups[1].Value));

		if (result.Count == 0) {
			foreach (Match m in VdfOldPath().Matches(vdf))
				result.Add(Unescape(m.Groups[1].Value));
		}

		return result;
	}

	private static string Unescape(string value) => value.Replace("""\\""", """\""", StringComparison.Ordinal);

	/// <summary>The Steam install folder, with backslashes.</summary>
	private static string? SteamRoot(RegistryReader registry)
		=> (registry(RegistryRoot.CurrentUser, """Software\Valve\Steam""", "SteamPath")
			?? registry(RegistryRoot.LocalMachine, """SOFTWARE\WOW6432Node\Valve\Steam""", "InstallPath")
			?? registry(RegistryRoot.LocalMachine, """SOFTWARE\Valve\Steam""", "InstallPath"))
			?.Replace('/', '\\');

	private static string? ReadTextFile(string path)
	{
		try {
			return File.Exists(path) ? File.ReadAllText(path) : null;
		} catch (IOException) {
			return null;
		} catch (UnauthorizedAccessException) {
			return null;
		}
	}

	[SupportedOSPlatform("windows")]
	private static string? ReadRegistry(RegistryRoot root, string subKey, string name)
	{
		try {
			var hive = root == RegistryRoot.CurrentUser ? Microsoft.Win32.Registry.CurrentUser : Microsoft.Win32.Registry.LocalMachine;
			using var key = hive.OpenSubKey(subKey);
			return key?.GetValue(name) is string s && s.Length > 0 ? s : null;
		} catch (System.Security.SecurityException) {
			return null;
		} catch (IOException) {
			return null;
		} catch (UnauthorizedAccessException) {
			return null;
		}
	}

	[GeneratedRegex("""
		"path"\s+"((?:[^"\\]|\\.)*)"
		""", RegexOptions.IgnoreCase)]
	private static partial Regex VdfPath();

	[GeneratedRegex("""
		^\s*"\d+"\s+"((?:[^"\\]|\\.)*)"\s*$
		""", RegexOptions.Multiline)]
	private static partial Regex VdfOldPath();
}
