using System.Drawing.Imaging;

namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// Draws a framed picture at any size in nine pieces: the corners at their size, the edges stretched along their length,
/// the inside stretched to fill. Used by the game's gold boxes (drop-downs, the side panel, the caravan frame).
/// </summary>
internal static class NineSlice
{
	/// <summary>Draws the part <paramref name="from"/> of <paramref name="source"/> in nine pieces into <paramref name="to"/>.</summary>
	/// <param name="g">The graphics, with the interpolation the caller wants.</param>
	/// <param name="source">The picture.</param>
	/// <param name="from">The part of the picture holding the frame.</param>
	/// <param name="sourceCorner">The size of a corner in the picture, in pixels.</param>
	/// <param name="to">Where to draw it.</param>
	/// <param name="corner">The size of a corner on screen.</param>
	/// <param name="inside">Whether the inside is drawn too, or left as it is.</param>
	/// <param name="attributes">The attributes of every piece (a wrap mode keeps a filter from blending in the pixels around it).</param>
	public static void Draw(Graphics g, Bitmap source, Rectangle from, int sourceCorner, Rectangle to, int corner, bool inside = true, ImageAttributes? attributes = null)
	{
		int[] sx = [from.Left, from.Left + sourceCorner, from.Right - sourceCorner];
		int[] sy = [from.Top, from.Top + sourceCorner, from.Bottom - sourceCorner];
		int[] sw = [sourceCorner, from.Width - (2 * sourceCorner), sourceCorner];
		int[] sh = [sourceCorner, from.Height - (2 * sourceCorner), sourceCorner];
		int[] dx = [to.Left, to.Left + corner, to.Right - corner];
		int[] dy = [to.Top, to.Top + corner, to.Bottom - corner];
		int[] dw = [corner, Math.Max(0, to.Width - (2 * corner)), corner];
		int[] dh = [corner, Math.Max(0, to.Height - (2 * corner)), corner];

		for (int row = 0; row < 3; row++)
		{
			for (int column = 0; column < 3; column++)
			{
				if ((row == 1 && column == 1 && !inside) || dw[column] <= 0 || dh[row] <= 0)
					continue;

				g.DrawImage(source, new Rectangle(dx[column], dy[row], dw[column], dh[row]), sx[column], sy[row], sw[column], sh[row], GraphicsUnit.Pixel, attributes);
			}
		}
	}
}
