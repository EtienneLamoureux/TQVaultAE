using System.IO;

namespace TQVaultAE.Tests;

/// <summary>
/// Provides static properties for xUnit v3 conditional skipping (SkipWhen/SkipUnless)
/// </summary>
public static class TestingConditions
{
	// Default game installation paths to check (WSL first, then Windows)
	private static readonly string[] GamePaths = new[]
	{
		"/mnt/d/DATA/Titan Quest Anniversary Edition",
		"/mnt/d/DATA/Titan Quest",
		"D:\\DATA\\Titan Quest Anniversary Edition",
		"D:\\Games\\Titan Quest Anniversary Edition",
		@"d:\games\steam\SteamApps\common\Titan Quest Anniversary Edition",
		"C:\\Games\\Titan Quest Anniversary Edition"
	};

	// Cache for game path
	private static string? _GamePath;

	/// <summary>
	/// Returns the detected Titan Quest game installation path, or null if not found.
	/// </summary>
	public static string? TitanQuestPath => GetGamePath();

	/// <summary>
	/// Returns true if Titan Quest game is installed
	/// </summary>
	public static bool IsGameInstalled => TitanQuestPath is not null;

	/// <summary>
	/// Returns true if database.arz exists in the game installation
	/// </summary>
	public static bool HasDatabaseArz
	{
		get
		{
			var gamePath = TitanQuestPath;
			return gamePath is not null && File.Exists(Path.Combine(gamePath, "Database", "database.arz"));
		}
	}

	/// <summary>
	/// Returns true if Text_EN.arc exists in the game installation
	/// </summary>
	public static bool HasTextEnArc
	{
		get
		{
			var gamePath = TitanQuestPath;
			return gamePath is not null && File.Exists(Path.Combine(gamePath, "Text", "Text_EN.arc"));
		}
	}

	/// <summary>
	/// Returns true if Text_fr.arc or Text_FR.arc exists in the game installation
	/// </summary>
	public static bool HasTextFrArc
	{
		get
		{
			var gamePath = TitanQuestPath;
			if (gamePath is null) return false;
			return File.Exists(Path.Combine(gamePath, "Text", "Text_FR.arc"));
		}
	}

	private static string? GetGamePath()
	{
		if (_GamePath is not null) return _GamePath;

		foreach (var path in GamePaths)
		{
			if (Directory.Exists(path))
			{
				_GamePath = path;
				return path;
			}
		}
		return null;
	}
}
