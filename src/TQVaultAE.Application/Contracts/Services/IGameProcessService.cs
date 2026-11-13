namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Whether the game is running: it keeps the character it plays in memory and writes its save over TQVault's changes.
/// </summary>
public interface IGameProcessService
{
	/// <summary>True when a game process (Titan Quest Anniversary Edition, Immortal Throne or the original game) runs.</summary>
	bool IsGameRunning();
}
