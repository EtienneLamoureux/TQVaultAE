using System.IO;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
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
	}
}