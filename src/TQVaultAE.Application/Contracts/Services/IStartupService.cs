using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Startup decisions made from the user settings and the command line.
/// </summary>
public interface IStartupService
{
	/// <summary>
	/// Gets the name of the culture to show the UI in, or null to keep the culture of the system.
	/// </summary>
	string? ResolveUICultureName();

	/// <summary>
	/// Sets the game paths, auto detected or configured, the game type and the vault folder.
	/// </summary>
	/// <returns>The message the caller shows about the vault folder.</returns>
	/// <exception cref="Domain.Exceptions.ExGamePathNotFound">No game folder was found, or a game folder has no
	/// <c>Database\database.arz</c>: the caller asks for the game folder and gives it to <see cref="ApplyChosenGamePath"/>.</exception>
	StartupNotice SetupGamePaths();

	/// <summary>
	/// Sets the game folder the user chose after <see cref="SetupGamePaths"/> failed in the settings, for the next
	/// start: as the forced game path, and as both configured paths when the paths are not auto detected.
	/// The caller saves the settings.
	/// </summary>
	void ApplyChosenGamePath(string gamePath);
}
