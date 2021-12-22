//-----------------------------------------------------------------------
// <copyright file="BitmapCode.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Presentation
{
	using Microsoft.Extensions.Logging;
	using System;
	using System.Drawing;
	using System.IO;
	using TQVaultAE.Domain.Contracts.Services;

	/// <summary>
	/// Loads Titan Quest textures and converts them into bitmaps.
	/// </summary>
	/// <remarks>
	/// Heavily modified by Da_FileServer to improve sanity, performance, and add transparent DDS support.
	/// </remarks>
	public class BitmapService : IBitmapService
	{
		private readonly ILogger Log;

		public BitmapService(ILogger<BitmapService> log)
		{
			this.Log = log;
		}

		/// <summary>
		/// Loads a .tex from memory and converts to bitmap
		/// </summary>
		/// <param name="data">raw tex data array</param>
		/// <param name="offset">offset into the array</param>
		/// <param name="count">number of bytes</param>
		/// <returns>bitmap of tex file.</returns>
		public Bitmap LoadFromTexMemory(byte[] data, int offset, int count)
		{
			// AMS: Yet another hack to new offset needed for Atlantis and Eternal Ember Images...
			int newTextureOffsetAdd = int.MinValue;

			if (data == null)
				throw new ArgumentNullException("data");

			if (offset < 0 || offset > data.Length)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0 || (data.Length - offset) < count)
				throw new ArgumentOutOfRangeException("count");

			if (data.Length < 12)
			{
				Log.LogError("TEX is not long enough to be valid.");
				return null;
			}

			if (BitConverter.ToUInt32(data, offset) == 0x01584554)
			{
				newTextureOffsetAdd = 0;
			}
			else if (BitConverter.ToUInt32(data, offset) == 39339348)
			{
				newTextureOffsetAdd = 1;
			}
			else
			{
				Log.LogError("Unexpected TEX magic found in game files, ignoring.");
				return null;
			}

			// We need to convert from TEX to DDS format as follows:
			// The 1st 12 bytes of a TEX file is crap.  We want to throw it away
			// We need to change bytes 13,14,15,16 to: 0x44 0x44 0x53 0x20
			// We then create it from DDS memory starting at offset 12

			// I assume this is the texture offset. (Just add 12 to make it a file offset.)
			int textureOffset = BitConverter.ToInt32(data, offset + 4);
			System.Diagnostics.Debug.Assert(textureOffset == 0, "Texture Offset == 0");

			if (textureOffset < 0 || textureOffset > (count - offset))
				throw new InvalidDataException("TEX texture offset is invalid.");

			int textureLength = BitConverter.ToInt32(data, offset + 8 + newTextureOffsetAdd);
			if (textureLength < 0 || textureLength > (count - offset - textureOffset))
				throw new InvalidDataException("TEX texture length is invalid.");

			if (textureLength < 4)
				throw new InvalidDataException("Cannot read TEX texture image magic.");

			int realOffset = offset + textureOffset + 12 + newTextureOffsetAdd;

			// realOffset + 0           = DDSmagic "DDS " or "DDSR"
			//                            Following DDSmagic we have the DDS_HEADER structure.
			// realOffset + 4 + 0       = DDS_HEADER Structure size.  It should be 124 bytes unless its a DXT10 texture.
			//                            at offset 72 within DDS_HEADER we have the DDS_PIXELFORMAT structure.
			// realOffset + 8 + 0       = DDS_HEADER flags.
			// realOffset + 12 + 0      = DDS_HEADER Surface height (in pixels).
			// realOffset + 16 + 0      = DDS_HEADER Surface width (in pixels).
			// realOffset + 20 + 0      = DDS_HEADER Pitch Or LinearSize.
			// realOffset + 24 + 0      = DDS_HEADER Depth of a volume texture (in pixels).
			// realOffset + 28 + 0      = DDS_HEADER Number of mipmap levels
			// realOffset + 4 + 72 + 0  = DDS_PIXELFORMAT Structure size.  Should be 32 bytes.
			// realOffset + 4 + 72 + 4  = DDS_PIXELFORMAT Flags / 0x1 = DDPF_ALPHAPIXELS flag
			// realOffset + 4 + 72 + 8  = DDS_PIXELFORMAT FOURCC - Four-character codes for specifying compressed or custom formats.
			// realOffset + 4 + 72 + 12 = DDS_PIXELFORMAT RGBBitCount.  Make sure it's 32 bit since we have an alpha channel.
			// realOffset + 4 + 72 + 16 = Red Bitmask for A8R8G8B8 the red mask would be 0x00ff0000
			// realOffset + 4 + 72 + 20 = Green Bitmask for A8R8G8B8 the green mask would be 0x0000ff00.
			// realOffset + 4 + 72 + 24 = Blue Bitmask for A8R8G8B8 the blue mask would be 0x000000ff.
			// realOffset + 4 + 72 + 28 = Alpha Bitmask for A8R8G8B8 the alpha mask would be 0xff000000.
			// realOffset + 4 + 104     = DDS_HEADER Complexity of the surfaces stored.  Should always set at least 0x1000.
			uint textureMagic = BitConverter.ToUInt32(data, realOffset);

			// Check for both "DDS " and "DDSR".
			if ((textureMagic == 0x52534444 || textureMagic == 0x20534444) && textureLength >= 128)
			{
				// Make sure the DDS header is "valid".
				if (BitConverter.ToInt32(data, realOffset + 4) == 124 &&
					BitConverter.ToInt32(data, realOffset + 76) == 32)
				{
					// Copy the texture data to a new buffer and fix the magic so it is a valid DDS texture.
					byte[] ddsData = new byte[textureLength];
					Buffer.BlockCopy(data, realOffset, ddsData, 0, textureLength);

					// Change "DDSR" to "DDS "
					ddsData[3] = 0x20;

					int bitDepth = BitConverter.ToInt32(ddsData, 88);
					if (bitDepth >= 24)
					{
						// Set the Red pixel mask
						ddsData[92] = 0;
						ddsData[93] = 0;
						ddsData[94] = 0xff;
						ddsData[95] = 0;

						// Set the Green pixel mask
						ddsData[96] = 0;
						ddsData[97] = 0xff;
						ddsData[98] = 0;
						ddsData[99] = 0;

						// Set the Blue pixel mask
						ddsData[100] = 0xff;
						ddsData[101] = 0;
						ddsData[102] = 0;
						ddsData[103] = 0;

						// HACK: Fix to make 32-bit DDS files use transparency.
						if (bitDepth == 32)
						{
							// Add the alpha bit to the pixel format.
							ddsData[80] |= 1;

							// Set the Alpha pixel mask
							ddsData[104] = 0;
							ddsData[105] = 0;
							ddsData[106] = 0;
							ddsData[107] = 0xff;
						}
					}

					// Set the DDS caps flag
					ddsData[109] |= 0x10;

					// now create it from this memory buffer
					return LoadFromMemory(ddsData, 0, ddsData.Length);
				}
				else
					throw new InvalidDataException("Invalid Header format.");
			}
			else
				throw new InvalidDataException("Unknown texture format.");
		}

		/// <summary>
		/// Loads the raw data and trys to convert it to a bitmap
		/// </summary>
		/// <param name="data">raw image data</param>
		/// <param name="offset">offset into the raw data</param>
		/// <param name="count">number of bytes to read</param>
		/// <returns>converted bitmap</returns>
		private Bitmap LoadFromMemory(byte[] data, int offset, int count)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			if (offset < 0 || offset > data.Length)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0 || (data.Length - offset) < count)
				throw new ArgumentOutOfRangeException("count");

			byte[] buffer = new byte[count];
			Buffer.BlockCopy(data, offset, buffer, 0, count);

			Bitmap bitmap = DDSReader.DDSImage.ConvertDDSToPng(buffer);// Png for transparency

			return bitmap;
		}
	}
}