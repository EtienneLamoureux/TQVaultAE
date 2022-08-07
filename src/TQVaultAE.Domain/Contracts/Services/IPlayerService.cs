using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using System.IO;

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
		/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
		/// <returns></returns>
		LoadPlayerResult LoadPlayer(PlayerSave selectedSave, bool isIT = false, bool fromFileWatcher = false);

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

		void AlterNameInPlayerFileSave(string newname, string saveFolder);
	}
}