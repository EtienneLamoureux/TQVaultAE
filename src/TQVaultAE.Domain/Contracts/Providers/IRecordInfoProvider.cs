using System.IO;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IRecordInfoProvider
	{
		/// <summary>
		/// Decodes the ARZ file.
		/// </summary>
		/// <param name="inReader">input BinaryReader</param>
		/// <param name="baseOffset">Offset in the file.</param>
		/// <param name="arzFile">ArzFile instance which we are operating.</param>
		void Decode(RecordInfo info, BinaryReader inReader, int baseOffset, ArzFile arzFile);
		/// <summary>
		/// Decompresses an individual record.
		/// </summary>
		/// <param name="arzFile">ARZ file which we are decompressing.</param>
		/// <returns>decompressed DBRecord.</returns>
		DBRecordCollection Decompress(ArzFile arzFile, RecordInfo info);
	}
}