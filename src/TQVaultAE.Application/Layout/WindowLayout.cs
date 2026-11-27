using System.Drawing;

namespace TQVaultAE.Application.Layout;

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

	/// <summary>
	/// Where a new window of <paramref name="size"/> goes so that it does not sit on one of the same kind already open:
	/// <paramref name="preferred"/> when no window of <paramref name="taken"/> is there, else the first free place one
	/// <paramref name="step"/> further down and right, then from the top left of <paramref name="workingArea"/> once the
	/// window would leave it. A place is taken when an open window's top left is less than half a step away. With every
	/// place taken, <paramref name="preferred"/>.
	/// </summary>
	/// <param name="preferred">The place the window would take alone (centered on its owner, for example).</param>
	/// <param name="size">The window's size.</param>
	/// <param name="workingArea">The working area of the window's screen.</param>
	/// <param name="taken">The top left corners of the windows of the same kind already open.</param>
	/// <param name="step">The offset between two windows, on each axis; at least 1.</param>
	public static Point Cascade(Point preferred, Size size, Rectangle workingArea, IReadOnlyCollection<Point> taken, int step)
	{
		step = Math.Max(1, step);
		bool Free(Point p) => !taken.Any(t => Math.Abs(t.X - p.X) * 2 < step && Math.Abs(t.Y - p.Y) * 2 < step);
		bool Fits(Point p) => p.X + size.Width <= workingArea.Right && p.Y + size.Height <= workingArea.Bottom;

		foreach (var origin in new[] { preferred, workingArea.Location })
		{
			for (int i = 0; ; i++)
			{
				var candidate = new Point(origin.X + (i * step), origin.Y + (i * step));
				// The preferred place itself, even when it does not fit (a window larger than the area).
				if (i > 0 && !Fits(candidate))
					break;
				if (Free(candidate))
					return candidate;
			}
		}

		return preferred;
	}

	/// <summary>
	/// Where a window saved at <paramref name="saved"/> reopens: there, kept within the working area of the screen it
	/// overlaps most; on the first area when it overlaps none (a monitor unplugged since). A window larger than the area
	/// keeps its top left corner on it.
	/// </summary>
	/// <param name="saved">The window's saved top left corner.</param>
	/// <param name="size">The window's size.</param>
	/// <param name="workingAreas">The working areas of the screens, the primary first.</param>
	public static Point KeepOnScreen(Point saved, Size size, IReadOnlyList<Rectangle> workingAreas)
	{
		if (workingAreas.Count == 0)
			return saved;

		var window = new Rectangle(saved, size);
		static long Overlap(Rectangle a, Rectangle b)
		{
			var both = Rectangle.Intersect(a, b);
			return (long)both.Width * both.Height;
		}

		var area = workingAreas.OrderByDescending(a => Overlap(window, a)).First();
		if (Overlap(window, area) == 0)
			area = workingAreas[0];

		return new Point(
			Math.Clamp(saved.X, area.Left, Math.Max(area.Left, area.Right - size.Width)),
			Math.Clamp(saved.Y, area.Top, Math.Max(area.Top, area.Bottom - size.Height)));
	}
}
