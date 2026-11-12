using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The black box with a thin gold frame of <see cref="Images.caravan_bg"/>, at the size of any control: drawn in nine
/// parts, so that the frame keeps its thickness and only the black inside stretches.
/// </summary>
internal static class CaravanFrame
{
	/// <summary>The thickness of the frame in the picture, in pixels (7, and one of black inside).</summary>
	public const int Border = 8;

	private static Bitmap? source;

	private static Bitmap Source => source ??= Images.caravan_bg;

	/// <summary>The frame at <paramref name="size"/>.</summary>
	public static Bitmap Render(Size size)
	{
		var result = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
		using var g = Graphics.FromImage(result);
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
		g.Clear(Color.Black);

		var src = Source;
		int b = Math.Min(Border, Math.Min(result.Width, result.Height) / 2);
		int sw = src.Width, sh = src.Height, w = result.Width, h = result.Height;
		void Part(int dx, int dy, int dw, int dh, int sx, int sy, int sW, int sH)
		{
			if (dw > 0 && dh > 0)
				g.DrawImage(src, new Rectangle(dx, dy, dw, dh), new Rectangle(sx, sy, sW, sH), GraphicsUnit.Pixel);
		}

		// Corners, then the edges stretched along their length.
		Part(0, 0, b, b, 0, 0, b, b);
		Part(w - b, 0, b, b, sw - b, 0, b, b);
		Part(0, h - b, b, b, 0, sh - b, b, b);
		Part(w - b, h - b, b, b, sw - b, sh - b, b, b);
		Part(b, 0, w - 2 * b, b, b, 0, sw - 2 * b, b);
		Part(b, h - b, w - 2 * b, b, b, sh - b, sw - 2 * b, b);
		Part(0, b, b, h - 2 * b, 0, b, b, sh - 2 * b);
		Part(w - b, b, b, h - 2 * b, sw - b, b, b, sh - 2 * b);
		return result;
	}

	/// <summary>
	/// Gives <paramref name="control"/> the frame as its background, made again when its size changes (zoom, DPI), with
	/// a padding that keeps its text inside the frame.
	/// </summary>
	/// <param name="control">The control.</param>
	/// <param name="padding">The padding of its text; by default the frame's thickness on every side.</param>
	public static void ApplyTo(Control control, Padding? padding = null)
	{
		control.BackgroundImageLayout = ImageLayout.None;
		control.Padding = padding ?? new Padding(Border);
		void Refresh()
		{
			var old = control.BackgroundImage;
			control.BackgroundImage = Render(control.Size);
			old?.Dispose();
		}

		Refresh();
		control.SizeChanged += (_, _) => Refresh();
		control.Disposed += (_, _) => control.BackgroundImage?.Dispose();
	}

	/// <summary>
	/// Draws <paramref name="box"/> as the frame instead of the group box's own border, without its caption. Its
	/// controls are moved off the frame's left edge and centered in its height (the caption's room is no longer needed),
	/// and made transparent so that the black shows behind them.
	/// </summary>
	/// <param name="box">The group box.</param>
	/// <param name="inset">The gap between the frame and its controls on the left; 4 pixels by default.</param>
	public static void ApplyTo(GroupBox box, int inset = 4)
	{
		Bitmap? frame = null;
		var children = box.Controls.Cast<Control>().ToList();
		if (children.Count > 0)
		{
			int dy = (box.Height - children.Max(c => c.Bottom) - children.Min(c => c.Top)) / 2;
			foreach (var child in children)
			{
				child.BackColor = Color.Transparent;
				child.Top += dy;
				if (child.Left < Border + inset)
					child.Left = Border + inset;
			}
		}

		box.Paint += (_, e) =>
		{
			if (frame is null || frame.Size != box.Size)
			{
				frame?.Dispose();
				frame = Render(box.Size);
			}

			e.Graphics.DrawImageUnscaled(frame, 0, 0);
		};
		box.Disposed += (_, _) => frame?.Dispose();
	}
}
