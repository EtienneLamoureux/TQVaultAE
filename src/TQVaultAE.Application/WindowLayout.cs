using System.Drawing;

namespace TQVaultAE.Application;

/// <summary>
/// Where the content of a window drawn with the TQ border may go: inside the border pictures, and clear of the resize
/// grips along the sides and the bottom, so no child control hides the border or takes the mouse from a grip.
/// </summary>
public static class WindowLayout
{
	/// <summary>
	/// The part of the client area the content may use.
	/// </summary>
	/// <param name="client">The window's client size.</param>
	/// <param name="side">Width of the side border picture.</param>
	/// <param name="top">Height of the title bar picture.</param>
	/// <param name="bottom">Height of the bottom border picture.</param>
	/// <param name="grip">How far the resize grips reach past the side and bottom pictures.</param>
	public static Rectangle ContentArea(Size client, int side, int top, int bottom, int grip)
	{
		int left = side + grip;
		int right = client.Width - side - grip;
		int bottomEdge = client.Height - bottom - grip;
		return new Rectangle(left, top, Math.Max(0, right - left), Math.Max(0, bottomEdge - top));
	}

	/// <summary>
	/// The size that makes a control placed at <paramref name="location"/> reach the right and bottom edges of
	/// <paramref name="area"/>; never negative.
	/// </summary>
	public static Size FillTo(Rectangle area, Point location)
		=> new(Math.Max(0, area.Right - location.X), Math.Max(0, area.Bottom - location.Y));

	/// <summary>
	/// <paramref name="design"/> (a size in design pixels) at <paramref name="scale"/> (zoom times DPI factor).
	/// </summary>
	public static Size Scaled(Size design, float scale)
		=> new((int)Math.Round(design.Width * scale), (int)Math.Round(design.Height * scale));

	/// <summary>
	/// <paramref name="client"/>, grown so that <paramref name="content"/> placed at <paramref name="location"/> fits
	/// inside <paramref name="area"/>, the content area of that client size; never smaller than <paramref name="client"/>.
	/// </summary>
	public static Size GrowToFit(Size client, Rectangle area, Point location, Size content)
		=> new(
			client.Width + Math.Max(0, location.X + content.Width - area.Right),
			client.Height + Math.Max(0, location.Y + content.Height - area.Bottom));

	/// <summary>
	/// The largest zoom at which <paramref name="design"/> (a size in design pixels) at <paramref name="dpiScale"/> times
	/// the zoom still fits inside <paramref name="limit"/>.
	/// </summary>
	public static float LargestZoom(Size design, float dpiScale, Size limit)
		=> Math.Min(limit.Width / (design.Width * dpiScale), limit.Height / (design.Height * dpiScale));

	/// <summary>
	/// <paramref name="size"/>, cut to <paramref name="limit"/> on each side that exceeds it.
	/// </summary>
	public static Size Within(Size size, Size limit)
		=> new(Math.Min(size.Width, limit.Width), Math.Min(size.Height, limit.Height));

	/// <summary>
	/// Where a window of <paramref name="size"/> is centered in <paramref name="workingArea"/>, but never above nor left
	/// of it: a window larger than the area keeps its caption on screen.
	/// </summary>
	public static Point CenteredIn(Rectangle workingArea, Size size)
		=> new(
			workingArea.Left + Math.Max(0, (workingArea.Width - size.Width) / 2),
			workingArea.Top + Math.Max(0, (workingArea.Height - size.Height) / 2));
}
