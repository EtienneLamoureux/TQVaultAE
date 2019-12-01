using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IArcFileProvider
	{

		/// <summary>
		/// Extracts the decoded ARC file contents into a folder.
		/// </summary>
		/// <param name="destination">Destination folder for the files.</param>
		/// <returns>true if successful, false on error.</returns>
		bool ExtractArcFile(ArcFile file, string destination);
		/// <summary>
		/// Reads data from an ARC file and puts it into a Byte array (or NULL if not found)
		/// </summary>
		/// <param name="dataId">The string ID for the data which we are retieving.</param>
		/// <returns>Returns byte array of the data corresponding to the string ID.</returns>
		byte[] GetData(ArcFile file, string dataId);
		/// <summary>
		/// Read the table of contents of the ARC file
		/// </summary>
		void ReadARCToC(ArcFile file);
		/// <summary>
		/// Gets the sorted list of directoryEntries.
		/// </summary>
		/// <returns>string array holding the sorted list</returns>
		string[] GetKeyTable(ArcFile file);
		/// <summary>
		/// Reads the ARC file table of contents to determine if the file is readable.
		/// </summary>
		/// <returns>True if able to read the ToC</returns>
		bool Read(ArcFile file);
		/// <summary>
		/// Writes a record to a file.
		/// </summary>
		/// <param name="baseFolder">string holding the base folder path</param>
		/// <param name="record">Record we are writing</param>
		/// <param name="destinationFileName">Filename for the new file.</param>
		void Write(ArcFile file, string baseFolder, string record, string destinationFileName);
	}
}