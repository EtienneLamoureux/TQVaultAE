using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IPlayerService
{
	/// <summary>
	/// Loads a player. Its stash is loaded apart (<see cref="IStashService.LoadPlayerStash"/>), see
	/// <see cref="LoadCharacterSession"/>.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Player load result with the player and error information.</returns>
	PlayerLoadResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher = false);

	/// <summary>
	/// Names the class and the masteries of a loaded character with its own map's data
	/// (<see cref="IGameMapService.ReadWith"/>): a mod can rename the classes and bring its own masteries, and the
	/// active map may be another character's. Also refreshes <see cref="PlayerSave.ClassLabel"/>.
	/// </summary>
	/// <param name="save">The character; <see cref="CharacterLabels.Empty"/> while it is not loaded.</param>
	CharacterLabels DescribeCharacter(PlayerSave save);

	/// <summary>
	/// Loads what a character selection shows: the character, then for an Immortal Throne character
	/// its stash and, when asked, the relic vault and transfer stashes.
	/// A Titan Quest original character has none of these stashes.
	/// </summary>
	/// <param name="selectedSave">The selected character.</param>
	/// <param name="loadTransferStash">Load the transfer stash (it is not loaded yet).</param>
	/// <param name="loadRelicVaultStash">Load the relic vault stash (it is not loaded yet).</param>
	/// <returns>The loads, each with what is needed for its messages.</returns>
	CharacterSessionLoadResult LoadCharacterSession(PlayerSave selectedSave, bool loadTransferStash, bool loadRelicVaultStash);

	/// <summary>
	/// Gets the loaded players that have unsaved changes, with their file.
	/// </summary>
	IReadOnlyList<(string File, PlayerCollection Player)> GetModifiedPlayers();

	/// <summary>
	/// Backs up (unless legacy backups are disabled) and saves one player file, then marks the player saved.
	/// </summary>
	/// <exception cref="IOException">The file could not be written.</exception>
	/// <exception cref="UnauthorizedAccessException">The file or folder cannot be written to.</exception>
	void SaveModifiedPlayer(string file, PlayerCollection player);

	/// <summary>
	/// Gets a list of all of the character files in the save folder.
	/// </summary>
	/// <returns>List of character files descriptor</returns>
	PlayerSave[] GetPlayerSaveList();

	/// <summary>
	/// Gets a list of all of the character files in the save folder.
	/// </summary>
	/// <returns>Read-only list of character files descriptor</returns>
	IReadOnlyList<PlayerSave> GetPlayerSaveListReadOnly();

	/// <summary>
	/// Alters the player name in the save file.
	/// </summary>
	/// <param name="newname">New player name.</param>
	/// <param name="saveFolder">Save folder path.</param>
	void AlterNameInPlayerFileSave(string newname, string saveFolder);

	/// <summary>
	/// Checks the name of a character about to be duplicated from <paramref name="source"/>.
	/// </summary>
	/// <param name="source">The character to duplicate.</param>
	/// <param name="newName">The new name, already trimmed.</param>
	/// <param name="existingNames">The names of the known characters.</param>
	/// <returns><see cref="CharacterNameError.None"/> when the name can be used, else the first problem found.</returns>
	CharacterNameError ValidateNewCharacterName(PlayerSave source, string newName, IEnumerable<string> existingNames);

	/// <summary>
	/// Copies the save files of <paramref name="source"/> to a new save folder and writes <paramref name="newName"/>
	/// into the new player file. On failure, the new folder is removed.
	/// </summary>
	/// <param name="source">The character to duplicate.</param>
	/// <param name="newName">The new name, validated by <see cref="ValidateNewCharacterName"/>.</param>
	/// <returns>The new save folder, or the failure.</returns>
	CharacterDuplicateResult DuplicateCharacter(PlayerSave source, string newName);

	/// <summary>
	/// Updates the player file path in the player collection cache after archiving/unarchiving.
	/// </summary>
	/// <param name="oldPath">The old file path of the player.</param>
	/// <param name="newPath">The new file path of the player.</param>
	void UpdatePlayerFilePath(string oldPath, string newPath);

	/// <summary>
	/// Archives a character (moves its save folder) and keys the loaded player by its new file path.
	/// </summary>
	/// <returns><see langword="true"/> when the save was archived.</returns>
	bool Archive(PlayerSave ps);

	/// <summary>
	/// Unarchives a character (moves its save folder back) and keys the loaded player by its new file path.
	/// </summary>
	/// <returns><see langword="true"/> when the save was unarchived.</returns>
	bool Unarchive(PlayerSave ps);

	/// <summary>
	/// Archives (or unarchives) every save of <paramref name="saves"/> not yet in that state, as <see cref="Archive"/>
	/// and <see cref="Unarchive"/> do one by one. With <paramref name="tag"/>, only the saves having that tag.
	/// </summary>
	/// <param name="saves">The known characters.</param>
	/// <param name="archive"><see langword="true"/> to archive, <see langword="false"/> to unarchive.</param>
	/// <param name="tag">The tag the saves must have, or <see langword="null"/> for all of them.</param>
	/// <returns>The saves moved and the ones that could not be.</returns>
	ArchiveChangeResult SetArchived(IEnumerable<PlayerSave> saves, bool archive, string? tag);

	/// <summary>
	/// Indicates whether the current player meets the requirements (level, strength, dexterity, intelligence) of
	/// <paramref name="item"/>. Without a current player, or for an item without requirements, it does.
	/// </summary>
	/// <param name="item">Item to check.</param>
	/// <returns><see langword="true"/> when the item can be equipped.</returns>
	bool CurrentPlayerMeetsRequirements(Item item);

	/// <summary>
	/// Indicates whether <paramref name="item"/> suits the current player: a Titan Quest original player only takes
	/// the items of the base game. Without a current player, every item suits.
	/// </summary>
	/// <param name="item">Item to check.</param>
	/// <returns><see langword="true"/> when the item can be equipped.</returns>
	bool IsSuitableForCurrentPlayer(Item item);

	/// <summary>
	/// The items the character wears: <paramref name="equipment"/> without the item in hand. An equipped item picked up
	/// stays in its slot until it is placed elsewhere, but no longer counts, for the item requirements as for the
	/// statistics panel.
	/// </summary>
	/// <param name="equipment">The items of the equipment sack.</param>
	/// <param name="held">The item being dragged, if any.</param>
	IReadOnlyList<Item> GetWornEquipment(IEnumerable<Item> equipment, Item? held);

	/// <summary>
	/// Recomputes the bonuses behind the attributes of <paramref name="player"/> (<see cref="PlayerInfo.CalculatedStrength"/>
	/// and the others, which the item requirements are checked against) with the sums of the statistics panel: the
	/// equipped items and their sets (<see cref="ICharacterStatsService.GetGearAttributes"/>), the learned skills at
	/// their level with the items' "+skill" levels and the skills the items grant, no buff. Does nothing for a player
	/// without information.
	/// </summary>
	/// <param name="player">Player to update.</param>
	/// <param name="equipment">The equipped items.</param>
	/// <param name="ignored">An item left out, as the item being dragged (see <see cref="GetWornEquipment"/>).</param>
	void UpdateAttributeBonuses(PlayerCollection player, IEnumerable<Item> equipment, Item? ignored = null);
}
