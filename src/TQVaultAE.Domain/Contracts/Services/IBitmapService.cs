using System.Drawing;

namespace TQVaultAE.Domain.Contracts.Services
{

	/// <summary>
	/// Loads Titan Quest textures and converts them into bitmaps.
	/// </summary>
	/// <remarks>
	/// Heavily modified by Da_FileServer to improve sanity, performance, and add transparent DDS support.
	/// </remarks>
	public interface IBitmapService
	{
		/// <summary>
		/// Loads a .tex from memory and converts to bitmap
		/// </summary>
		/// <param name="data">raw tex data array</param>
		/// <param name="offset">offset into the array</param>
		/// <param name="count">number of bytes</param>
		/// <returns>bitmap of tex file.</returns>
		Bitmap LoadFromTexMemory(byte[] data, int offset, int count);
	}
}