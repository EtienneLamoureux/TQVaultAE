using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IPlayerCollectionProvider
	{
		void CommitPlayerInfo(PlayerCollection pc, PlayerInfo playerInfo);
		/// <summary>
		/// Converts the live data back into the raw binary data format.
		/// </summary>
		/// <returns>Byte Array holding the converted binary data</returns>
		byte[] Encode(PlayerCollection pc);
		/// <summary>
		/// Looks for the next begin_block or end_block.
		/// </summary>
		/// <param name="start">offset where we are starting our search</param>
		/// <returns>Returns the index of the first char indicating the block delimiter or -1 if none is found.</returns>
		int FindNextBlockDelim(PlayerCollection pc, int start);
		/// <summary>
		/// Attempts to load a player file
		/// </summary>
		void LoadFile(PlayerCollection pc);
		/// <summary>
		/// Attempts to save the file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		void Save(PlayerCollection pc, string fileName);
	}
}