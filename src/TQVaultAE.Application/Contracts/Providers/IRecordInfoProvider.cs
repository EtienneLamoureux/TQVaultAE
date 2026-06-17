using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Providers;

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
	/// Decodes the ARZ file using ReadOnlySpan for zero-copy parsing.
	/// </summary>
	/// <param name="info">RecordInfo to populate</param>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced by the method</param>
	/// <param name="baseOffset">Base offset to add to the record offset</param>
	/// <param name="arzFile">ArzFile instance which we are operating.</param>
	void Decode(RecordInfo info, ReadOnlySpan<byte> data, ref int offset, int baseOffset, ArzFile arzFile);

	/// <summary>
	/// Decompresses an individual record.
	/// </summary>
	/// <param name="arzFile">ARZ file which we are decompressing.</param>
	/// <returns>decompressed DBRecord.</returns>
	DBRecordCollection Decompress(ArzFile arzFile, RecordInfo info);
}