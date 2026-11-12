//-----------------------------------------------------------------------
// <copyright file="GameLocator.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.DbCli.Core;

/// <summary>Located game install.</summary>
/// <param name="Root">Install folder.</param>
/// <param name="Database">database.arz path.</param>
/// <param name="Archives">Every .arc under the install (Resources, Resources\XPack*, Audio, Text, Toolset).</param>
public sealed record GameInstall(string Root, string Database, IReadOnlyList<string> Archives)
{
	/// <summary>Text archive for a language (e.g. "EN" → Text\Text_EN.arc), <see langword="null"/> when missing.</summary>
	public string? TextArchive(string language)
	{
		var p = System.IO.Path.Combine(this.Root, "Text", $"Text_{language.ToUpperInvariant()}.arc");
		return File.Exists(p) ? p : null;
	}
}

/// <summary>Finds the game install.</summary>
public static class GameLocator
{
	/// <summary>Environment variable that overrides detection.</summary>
	public const string EnvironmentVariable = "TQ_GAME_PATH";

	private static readonly string[] WellKnownFolders = [
		"""C:\Program Files (x86)\Steam\steamapps\common\Titan Quest Anniversary Edition""",
		"""C:\Program Files\Steam\steamapps\common\Titan Quest Anniversary Edition""",
		"""C:\GOG Games\Titan Quest - Anniversary Edition""",
		"""C:\Program Files (x86)\GOG Galaxy\Games\Titan Quest - Anniversary Edition""",
		"""D:\SteamLibrary\steamapps\common\Titan Quest Anniversary Edition""",
		"""E:\SteamLibrary\steamapps\common\Titan Quest Anniversary Edition""",
		"""D:\Games\Titan Quest Anniversary Edition""",
		"""C:\Games\Titan Quest Anniversary Edition""",
	];

	/// <summary>
	/// Order: <paramref name="explicitRoot"/>, env TQ_GAME_PATH, Steam (registry + libraryfolders.vdf, app 475150),
	/// GOG registry, well-known folders. Throws <see cref="DirectoryNotFoundException"/> when not found.
	/// </summary>
	public static GameInstall Locate(string? explicitRoot = null)
		=> Locate(explicitRoot, Environment.GetEnvironmentVariable(EnvironmentVariable), Candidates());

	/// <summary><see cref="Locate(string?)"/> with the environment value and the detected folders given (tests).</summary>
	internal static GameInstall Locate(string? explicitRoot, string? env, IEnumerable<string> candidates)
	{
		if (!string.IsNullOrWhiteSpace(explicitRoot)) {
			return IsGameRoot(explicitRoot)
				? FromRoot(explicitRoot)
				: throw new DirectoryNotFoundException($"'{explicitRoot}' is not a Titan Quest install (Database\\database.arz not found).");
		}

		if (!string.IsNullOrWhiteSpace(env)) {
			return IsGameRoot(env)
				? FromRoot(env)
				: throw new DirectoryNotFoundException($"{EnvironmentVariable}='{env}' is not a Titan Quest install (Database\\database.arz not found).");
		}

		foreach (var candidate in candidates)
			if (IsGameRoot(candidate))
				return FromRoot(candidate);

		throw new DirectoryNotFoundException($"Titan Quest Anniversary Edition not found. Pass the install folder explicitly or set {EnvironmentVariable}.");
	}

	/// <summary>Builds the install description for a known root (enumerates every .arc recursively).</summary>
	public static GameInstall FromRoot(string root)
	{
		var full = Path.GetFullPath(root.Trim().Trim('"'));
		var archives = Directory.EnumerateFiles(full, "*.arc", SearchOption.AllDirectories)
			.Order(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		return new GameInstall(full, DatabasePath(full), archives);
	}

	/// <summary>True when <paramref name="root"/> contains Database\database.arz.</summary>
	internal static bool IsGameRoot(string root)
	{
		try {
			return File.Exists(DatabasePath(root.Trim().Trim('"')));
		} catch (ArgumentException) {
			return false;
		}
	}

	internal static string DatabasePath(string root) => Path.Combine(root, "Database", "database.arz");

	private static IEnumerable<string> Candidates()
		=> Candidates(InstallLocations.SteamCandidates(), InstallLocations.GogCandidates());

	/// <summary>Detection order after the explicit folder and the environment: Steam, GOG, well-known folders.</summary>
	internal static IEnumerable<string> Candidates(IEnumerable<string> steam, IEnumerable<string> gog)
		=> steam.Concat(gog).Concat(WellKnownFolders);
}
