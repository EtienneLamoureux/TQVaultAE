using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IStashService
{
	/// <summary>
	/// Loads a player stash.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	StashLoadResult LoadPlayerStash(PlayerSave? selectedSave, bool fromFileWatcher = false);

	/// <summary>
	/// Loads the relic vault stash.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	StashLoadResult LoadRelicVaultStash(bool fromFileWatcher = false);

	/// <summary>
	/// Loads the transfer stash for immortal throne.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	StashLoadResult LoadTransferStash(bool fromFileWatcher = false);

	/// <summary>
	/// Gets the loaded stashes that have unsaved changes, with their file.
	/// </summary>
	IReadOnlyList<(string File, Stash Stash)> GetModifiedStashes();

	/// <summary>
	/// Backs up (unless legacy backups are disabled) and saves one stash file and its .dxg twin, then marks the stash saved.
	/// </summary>
	/// <exception cref="IOException">The file could not be written.</exception>
	/// <exception cref="UnauthorizedAccessException">The file or folder cannot be written to.</exception>
	void SaveModifiedStash(string file, Stash stash);

	/// <summary>
	/// Brings a stash panel bag id within the bags it has: below the equipment panel gives the equipment panel,
	/// beyond the relic vault stash gives the relic vault stash.
	/// </summary>
	/// <param name="bagId">The requested bag id (see <see cref="BagIdConstants"/>).</param>
	/// <returns>A bag id from <see cref="BagIdConstants.BAGID_EQUIPMENTPANEL"/> to <see cref="BagIdConstants.BAGID_RELICVAULTSTASH"/>.</returns>
	int ClampBagId(int bagId);

	/// <summary>
	/// Gets the sack a stash panel bag shows.
	/// </summary>
	/// <param name="bagId">The bag id (see <see cref="BagIdConstants"/>).</param>
	/// <param name="player">The character, for the equipment panel.</param>
	/// <param name="transferStash">The transfer stash (caravan).</param>
	/// <param name="playerStash">The character's stash.</param>
	/// <param name="relicVaultStash">The relic vault stash.</param>
	/// <returns>The sack; null when the bag has nothing to show (no character, or a stash without sacks).</returns>
	SackCollection? GetBagSack(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash);

	/// <summary>
	/// Describes what a stash panel bag shows when it is selected: its sack and the size used to lay it out.
	/// </summary>
	/// <param name="bagId">The bag id (see <see cref="BagIdConstants"/>), clamped like <see cref="ClampBagId"/>.</param>
	/// <param name="player">The character, for the equipment panel.</param>
	/// <param name="transferStash">The transfer stash (caravan).</param>
	/// <param name="playerStash">The character's stash.</param>
	/// <param name="relicVaultStash">The relic vault stash.</param>
	/// <returns>The bag; null for a stash bag whose stash is not loaded. The equipment panel is always described,
	/// with a null sack when there is no character.</returns>
	StashBag? GetBag(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash);
}
