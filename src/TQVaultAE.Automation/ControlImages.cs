using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TQVaultAE.Automation;

/// <summary>
/// Pictures of controls for the automation tools of TQVaultAE and ARZExplorer.
/// </summary>
public static class ControlImages
{
	/// <summary>Encodes <paramref name="image"/> as PNG.</summary>
	public static byte[] Png(Image image)
	{
		ArgumentNullException.ThrowIfNull(image);
		using var stream = new MemoryStream();
		image.Save(stream, ImageFormat.Png);
		return stream.ToArray();
	}

	/// <summary>Draws <paramref name="control"/> to a bitmap of <paramref name="size"/> (its own size by default) with <see cref="Control.DrawToBitmap"/>, which works while it is hidden.</summary>
	public static Bitmap Render(Control control, Size? size = null)
	{
		ArgumentNullException.ThrowIfNull(control);
		var area = size ?? control.Size;
		var bitmap = new Bitmap(area.Width, area.Height);
		control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, area));
		return bitmap;
	}

	/// <summary>PNG of <see cref="Render"/>.</summary>
	public static byte[] RenderPng(Control control)
	{
		using var bitmap = Render(control);
		return Png(bitmap);
	}
}
