using System.Drawing;
using System.Drawing.Drawing2D;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Loads Titan Quest textures and converts them into bitmaps.
/// </summary>
/// <remarks>
/// Heavily modified by Da_FileServer to improve sanity, performance, and add transparent DDS support.
/// </remarks>
public interface IBitmapService
{
	/// <summary>
	/// Loads a .tex from memory and convert it to bitmap
	/// </summary>
	/// <param name="data">raw tex data array</param>
	/// <param name="offset">offset into the array</param>
	/// <param name="count">number of bytes</param>
	/// <returns>bitmap of tex file.</returns>
	Bitmap LoadFromTexMemory(byte[] data, int offset, int count);

	/// <summary>Resizes an image to a new width and height value.</summary>
	/// <param name="image">The image to resize.</param>
	/// <param name="newWidth">The width of the new image.</param>
	/// <param name="newHeight">The height of the new image.</param>
	/// <param name="mode">Interpolation mode.</param>
	/// <param name="maintainAspectRatio">If true, the image is centered in the middle of the returned image, maintaining the aspect ratio of the original image.</param>
	/// <returns>The new image. The old image is unaffected.</returns>
	/// <remarks>
	/// From https://stackoverflow.com/questions/1940581/c-sharp-image-resizing-to-different-size-while-preserving-aspect-ratio
	/// </remarks>
	Image ResizeImage(Image image, int newWidth, int newHeight
		, InterpolationMode mode = InterpolationMode.Default
		, bool maintainAspectRatio = false
	);
}