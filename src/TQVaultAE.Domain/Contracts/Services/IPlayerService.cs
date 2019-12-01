using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IPlayerService
	{
		/// <summary>
		/// Loads a player using the drop down list.
		/// Assumes designators are appended to character name.
		/// </summary>
		/// <param name="selectedSave">Player string from the drop down list.</param>
		/// <param name="isIT"></param>
		/// <returns></returns>
		LoadPlayerResult LoadPlayer(PlayerSave selectedSave, bool isIT = false);

		/// <summary>
		/// Attempts to save all modified player files
		/// </summary>
		/// <param name="playerOnError"></param>
		/// <returns>True if there were any modified player files.</returns>
		/// <exception cref="IOException">can happen during file save</exception>
		bool SaveAllModifiedPlayers(ref PlayerCollection playerOnError);

		/// <summary>
		/// Gets a list of all of the character files in the save folder.
		/// </summary>
		/// <returns>List of character files descriptor</returns>
		PlayerSave[] GetPlayerSaveList();
	}
}