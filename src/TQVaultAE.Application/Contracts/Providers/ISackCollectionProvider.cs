using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Providers;

public interface ISackCollectionProvider
{
	/// <summary>
	/// Encodes the sack into binary form
	/// </summary>
	/// <param name="writer">BinaryWriter instance</param>
	void Encode(SackCollection sc, BinaryWriter writer);
	/// <summary>
	/// Parses the binary sack data to internal data
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	void Parse(SackCollection sc, BinaryReader reader);

	/// <summary>
	/// Parses the header portion of sack data using ReadOnlySpan for zero-copy parsing.
	/// Enables bounds-check elimination in high-frequency parsing paths.
	/// </summary>
	/// <param name="sc">SackCollection to populate</param>
	/// <param name="data">ReadOnlySpan of binary data</param>
	/// <param name="offset">Offset that will be advanced by the method</param>
	/// <returns>Number of bytes consumed for header</returns>
	int ParseHeader(SackCollection sc, ReadOnlySpan<byte> data, ref int offset);
}