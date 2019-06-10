//-----------------------------------------------------------------------
// <copyright file="BitmapCode.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Note: FreeImage is technically thread-safe. This is because internally, the
// objects (FIBITMAP, FIMEMORY, etc.) are actually allocated via malloc, which
// is thread-safe. The only thing which may not be thread-safe are plugins. But
// since those probably are not being modified, it should be safe.

namespace TQVaultAE.DAL
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Security.Permissions;
	using Tao.DevIl;

	/// <summary>
	/// Loads Titan Quest textures and converts them into bitmaps.
	/// </summary>
	/// <remarks>
	/// Heavily modified by Da_FileServer to improve sanity, performance, and add transparent DDS support.
	/// </remarks>
	public static class BitmapCode
	{
		/// <summary>
		/// Loads a DDS file into memory
		/// </summary>
		/// <param name="fileName">Name of the DDS file.</param>
		/// <returns>bitmap of DDS</returns>
		public static Bitmap LoadFromDdsFile(string fileName)
		{
			// Load the file into memory.
			byte[] data;
			using (FileStream inStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				int offset = 0;
				int bytesRead;

				data = new byte[Convert.ToInt32(inStream.Length)];
				while ((bytesRead = inStream.Read(data, offset, data.Length - offset)) > 0)
				{
					offset += bytesRead;
					System.Diagnostics.Debug.Assert(offset <= data.Length, "Offset it too large! Read too many bytes.");
				}
			}

			if (data.Length >= 128)
			{
				uint ddsMagic = BitConverter.ToUInt32(data, 0);
				if (ddsMagic == 0x52534444 || ddsMagic == 0x20534444)
				{
					// Fixup DDS magic for Titan Quest textures.
					if (ddsMagic == 0x52534444)
					{
						data[3] = 0x20;
					}

					// HACK: Fix to make 32-bit DDS files use transparency.
					// (Additionally make sure the DDS header is "valid".)
					if (BitConverter.ToInt32(data, 4) == 124 &&
						BitConverter.ToInt32(data, 76) == 32 &&
						BitConverter.ToInt32(data, 88) == 32)
					{
						// Add the alpha bit to the pixel format.
						data[80] |= 1;
					}
				}
			}

			return LoadFromDdsMemory(data, 0, data.Length);
		}

		/// <summary>
		/// Loads a DDS from memory and converts to bitmap
		/// </summary>
		/// <param name="data">raw data array</param>
		/// <param name="offset">offset into array</param>
		/// <param name="count">number of bytes</param>
		/// <returns>bitmap of dds file</returns>
		public static Bitmap LoadFromDdsMemory(byte[] data, int offset, int count)
		{
			Bitmap bitmap = LoadFromMemory(data, offset, count);
			return bitmap;
		}

		/// <summary>
		/// Loads a .tex file
		/// </summary>
		/// <param name="fileName">Name of the .tex file</param>
		/// <returns>Bitmap of the .tex file</returns>
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public static Bitmap LoadFromTexFile(string fileName)
		{
			// Load the file into memory.
			byte[] data;
			using (FileStream inStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				int offset = 0;
				int bytesRead;

				data = new byte[Convert.ToInt32(inStream.Length)];
				while ((bytesRead = inStream.Read(data, offset, data.Length - offset)) > 0)
				{
					offset += bytesRead;
					System.Diagnostics.Debug.Assert(offset <= data.Length, "Offset it too large! Read too many bytes.");
				}
			}

			return LoadFromTexMemory(data, 0, data.Length);
		}

		/// <summary>
		/// Loads a .tex from memory and converts to bitmap
		/// </summary>
		/// <param name="data">raw tex data array</param>
		/// <param name="offset">offset into the array</param>
		/// <param name="count">number of bytes</param>
		/// <returns>bitmap of tex file.</returns>
		public static Bitmap LoadFromTexMemory(byte[] data, int offset, int count)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			if (offset < 0 || offset > data.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}

			if (count < 0 || (data.Length - offset) < count)
			{
				throw new ArgumentOutOfRangeException("count");
			}

			if (data.Length < 12)
			{
				throw new InvalidDataException("TEX is not long enough to be valid.");
			}

			if (BitConverter.ToUInt32(data, offset) != 0x01584554)
			{
				throw new InvalidDataException("TEX magic is invalid.");
			}

			// We need to convert from TEX to DDS format as follows:
			// The 1st 12 bytes of a TEX file is crap.  We want to throw it away
			// We need to change bytes 13,14,15,16 to: 0x44 0x44 0x53 0x20
			// We then create it from DDS memory starting at offset 12

			// I assume this is the texture offset. (Just add 12 to make it a file offset.)
			int textureOffset = BitConverter.ToInt32(data, offset + 4);
			System.Diagnostics.Debug.Assert(textureOffset == 0, "Texture Offset == 0");

			if (textureOffset < 0 || textureOffset > (count - offset))
			{
				throw new InvalidDataException("TEX texture offset is invalid.");
			}

			int textureLength = BitConverter.ToInt32(data, offset + 8);
			if (textureLength < 0 || textureLength > (count - offset - textureOffset))
			{
				throw new InvalidDataException("TEX texture length is invalid.");
			}

			if (textureLength < 4)
			{
				throw new InvalidDataException("Cannot read TEX texture image magic.");
			}

			int realOffset = offset + textureOffset + 12;

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
					return LoadFromDdsMemory(ddsData, 0, ddsData.Length);
				}
				else
				{
					throw new InvalidDataException("Invalid Header format.");
				}
			}
			else
			{
				throw new InvalidDataException("Unknown texture format.");
			}
		}

		/// <summary>
		/// Converts an in-memory image in DDS format to a System.Drawing.Bitmap
		/// object for easy display in Windows forms.
		/// </summary>
		/// <remarks>
		/// From Tony V's post at http://forums.create.msdn.com/forums/p/12527/66109.aspx
		/// </remarks>
		/// <param name="ddsData">Byte array containing DDS image data</param>
		/// <returns>A Bitmap object that can be displayed</returns>
		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		private static Bitmap DDSDataToBMP(byte[] ddsData)
		{
			// Create a DevIL image "name" (which is actually a number)
			int imageName;
			Il.ilGenImages(1, out imageName);
			Il.ilBindImage(imageName);

			// Load the DDS file into the bound DevIL image
			Il.ilLoadL(Il.IL_DDS, ddsData, ddsData.Length);

			// Set a few size variables that will simplify later code
			int imageWidth = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
			int imageHeight = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
			Rectangle rect = new Rectangle(0, 0, imageWidth, imageHeight);

			// Convert the DevIL image to a pixel byte array to copy into Bitmap
			Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);

			// Create a Bitmap to copy the image into, and prepare it to get data
			Bitmap bitmap = new Bitmap(imageWidth, imageHeight);
			BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			// Copy the pixel byte array from the DevIL image to the Bitmap
			int result = Il.ilCopyPixels(0, 0, 0, Il.ilGetInteger(Il.IL_IMAGE_WIDTH), Il.ilGetInteger(Il.IL_IMAGE_HEIGHT), 1, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE, bitmapData.Scan0);

			if (result == Il.IL_INVALID_CONVERSION || result == Il.IL_ILLEGAL_OPERATION)
			{
				throw new InvalidDataException("Cannot Convert Bitmap.");
			}

			// Clean up and return Bitmap
			Il.ilDeleteImages(1, ref imageName);
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		/// <summary>
		/// Loads the raw data and trys to convert it to a bitmap
		/// </summary>
		/// <param name="data">raw image data</param>
		/// <param name="offset">offset into the raw data</param>
		/// <param name="count">number of bytes to read</param>
		/// <returns>converted bitmap</returns>
		private static Bitmap LoadFromMemory(byte[] data, int offset, int count)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			if (offset < 0 || offset > data.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}

			if (count < 0 || (data.Length - offset) < count)
			{
				throw new ArgumentOutOfRangeException("count");
			}

			byte[] buffer = new byte[count];
			Buffer.BlockCopy(data, offset, buffer, 0, count);

			Bitmap bitmap = DDSDataToBMP(buffer);

			if (bitmap == null)
			{
				// bummer
				return null;
			}

			return bitmap;
		}
	}
}