//-----------------------------------------------------------------------
// <copyright file="BitmapService.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using ImageMagick;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GameFiles;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Presentation.Win32;

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
	/// Loads the first frame of a .tex from memory and converts it to a bitmap.
	/// </summary>
	/// <param name="data">raw tex data array</param>
	/// <param name="offset">offset into the array</param>
	/// <param name="count">number of bytes</param>
	/// <returns>bitmap of the first frame, <see langword="null"/> when the data is not a texture.</returns>
	/// <exception cref="InvalidDataException">The texture is cut or damaged.</exception>
	public Bitmap? LoadFromTexMemory(byte[] data, int offset, int count)
	{
		ArgumentNullException.ThrowIfNull(data);

		if (offset < 0 || offset > data.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));

		if (count < 0 || (data.Length - offset) < count)
			throw new ArgumentOutOfRangeException(nameof(count));

		var tex = new ReadOnlySpan<byte>(data, offset, count);
		if (tex.Length < 12)
		{
			Log.LogError("TEX is not long enough to be valid.");
			return null;
		}

		if (!TexConverter.IsTex(tex))
		{
			Log.LogError("Unexpected TEX magic found in game files, ignoring.");
			return null;
		}

		return LoadFromMemory(TexConverter.ToDds(tex));
	}

	/// <summary>
	/// Loads the raw data and try to convert it to a bitmap
	/// </summary>
	/// <param name="data">raw image data</param>
	/// <returns>converted bitmap</returns>
	private Bitmap LoadFromMemory(byte[] data)
	{
		ArgumentNullException.ThrowIfNull(data);

		try
		{
			using var image = new MagickImage(data, MagickFormat.Dds);
			using var ms = new MemoryStream();
			image.Write(ms, MagickFormat.Png);
			ms.Position = 0;
			return new Bitmap(ms);
		}
		catch (Exception ex)
		{
			// You need to install ImageMagick on your computer and restart Visual Studio. Command "winget install ImageMagick.Q8"
			Log.LogError(ex, "Failed to load a DDS image with Magick.NET");
			throw new InvalidOperationException("Unable to load image data", ex);
		}
	}

	public Image ResizeImage(Image image, int newWidth, int newHeight
		, InterpolationMode mode = InterpolationMode.Default
		, bool maintainAspectRatio = false
	)
	{
		Bitmap output = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);

		using (Graphics gfx = Graphics.FromImage(output))
		{
			gfx.Clear(Color.FromArgb(0, 0, 0, 0));
			gfx.InterpolationMode = mode;
			if (mode == InterpolationMode.NearestNeighbor)
			{
				gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
				gfx.SmoothingMode = SmoothingMode.HighQuality;
			}

			// Default = Stretch
			Rectangle drawArea = new Rectangle(
				0
				, 0
				, newWidth
				, newHeight
			);

			if (maintainAspectRatio)
			{
				double ratioW = (double)newWidth / (double)image.Width;
				double ratioH = (double)newHeight / (double)image.Height;
				double ratio = ratioW < ratioH ? ratioW : ratioH;
				int insideWidth = (int)(image.Width * ratio);
				int insideHeight = (int)(image.Height * ratio);

				drawArea = new Rectangle(
					(newWidth / 2) - (insideWidth / 2)
					, (newHeight / 2) - (insideHeight / 2)
					, insideWidth
					, insideHeight
				);
			}

			gfx.DrawImage(image, drawArea);
		}

		return output;
	}
}