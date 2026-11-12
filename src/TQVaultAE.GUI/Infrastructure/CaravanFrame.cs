using System.Windows.Forms.VisualStyles;
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

	/// <summary>The color of the outline around a caption drawn over the frame, as the labels' drop shadow.</summary>
	private static readonly Color ShadowColor = Color.FromArgb(20, 12, 6);

	/// <summary>The gap between the glyph and the text of a check box, as the system draws it.</summary>
	private const int CheckBoxTextGap = 3;

	/// <summary>
	/// Draws <paramref name="box"/> as the frame, keeping its caption across the top edge as a group box has it: its text,
	/// or the check box of a <see cref="Components.UIToolbox.CheckGroupBox"/>, on black or straight over the frame (see
	/// <see cref="CaptionBackground"/>). Its content gets a padding off the frame and is made transparent so that the
	/// black shows behind it.
	/// </summary>
	/// <param name="box">The group box.</param>
	/// <param name="background">What lies behind the caption; black by default.</param>
	public static void ApplyWithCaption(GroupBox box, CaptionBackground background = CaptionBackground.Black)
	{
		Bitmap? frame = null;
		bool overBorder = background == CaptionBackground.None;
		box.BackColor = Color.Transparent;
		box.Padding = new Padding(Border + 2, box.Padding.Top, Border + 2, Border);
		foreach (Control child in box.Controls)
		{
			// The check box of a CheckGroupBox is its caption, across the frame's top edge.
			child.BackColor = child is CheckBox && !overBorder ? Color.Black : Color.Transparent;
		}

		box.Paint += (_, e) =>
		{
			var check = box.Controls.OfType<CheckBox>().FirstOrDefault();
			var caption = check is not null
				? check.Bounds
				: new Rectangle(new Point(Border + 2, 0), TextRenderer.MeasureText(e.Graphics, box.Text, box.Font));
			// The frame's gold line runs through the middle of the caption.
			int top = Math.Max(0, caption.Top + (caption.Height - Border) / 2);
			var size = new Size(box.Width, box.Height - top);
			if (frame is null || frame.Size != size)
			{
				frame?.Dispose();
				frame = Render(size);
			}

			if (!overBorder)
			{
				e.Graphics.DrawImageUnscaled(frame, 0, top);
				if (check is not null)
					return;

				e.Graphics.FillRectangle(Brushes.Black, caption);
				TextRenderer.DrawText(e.Graphics, box.Text, box.Font, caption, box.ForeColor);
				return;
			}

			// The group box drew its own caption and border line before this handler: above the frame they would show
			// next to the text drawn here, so that strip is painted again with the background. The frame is then drawn
			// whole, and the caption over it.
			if (top > 0)
				ButtonRenderer.DrawParentBackground(e.Graphics, new Rectangle(0, 0, box.Width, top), box);
			e.Graphics.DrawImageUnscaled(frame, 0, top);
			if (check is null)
			{
				DrawOutline(e.Graphics, box.Text, box.Font, caption, TextFormatFlags.Default);
				TextRenderer.DrawText(e.Graphics, box.Text, box.Font, caption, box.ForeColor);
				return;
			}

			// The check box is transparent: it paints this group behind itself, so the outline drawn here lies under the
			// text it draws, and its glyph, its clicks and its focus rectangle stay the system's.
			int left = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.UncheckedNormal).Width + CheckBoxTextGap;
			var text = new Rectangle(
				check.Left + check.Padding.Left + left,
				check.Top,
				Math.Max(0, check.ClientSize.Width - check.Padding.Left - left),
				check.ClientSize.Height);
			var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
			if (!check.UseMnemonic)
				flags |= TextFormatFlags.NoPrefix;
			DrawOutline(e.Graphics, check.Text, check.Font, text, flags);
		};
		box.Disposed += (_, _) => frame?.Dispose();
	}

	/// <summary>
	/// Draws a dark outline one pixel around <paramref name="text"/> (its copies in the eight directions), plus a drop
	/// shadow one pixel further lower right: the gold text drawn over it then stays readable on the gold frame pieces,
	/// the black inside and the marble alike.
	/// </summary>
	private static void DrawOutline(Graphics g, string text, Font font, Rectangle bounds, TextFormatFlags flags)
	{
		flags |= TextFormatFlags.PreserveGraphicsClipping;
		for (int dx = -1; dx <= 2; dx++)
		{
			for (int dy = -1; dy <= 2; dy++)
			{
				bool ring = dx <= 1 && dy <= 1 && (dx != 0 || dy != 0);
				if (ring || (dx == 2 && dy == 2))
				{
					var copy = bounds;
					copy.Offset(dx, dy);
					TextRenderer.DrawText(g, text, font, copy, ShadowColor, flags);
				}
			}
		}
	}

	/// <summary>
	/// Draws the frame around <paramref name="control"/>, on its parent: the control gets a margin of the frame's
	/// thickness, in which the parent paints the frame. For a control that draws its own content from its corner (a list).
	/// </summary>
	public static void FrameAround(Control control)
	{
		if (control.Parent is not { } parent)
			return;

		control.Margin = new Padding(Border);
		Bitmap? frame = null;
		parent.Paint += (_, e) =>
		{
			if (!control.Visible)
				return;

			var bounds = Rectangle.Inflate(control.Bounds, Border, Border);
			if (frame is null || frame.Size != bounds.Size)
			{
				frame?.Dispose();
				frame = Render(bounds.Size);
			}

			e.Graphics.DrawImageUnscaled(frame, bounds.Location);
		};
		void Repaint(object? sender, EventArgs e) => parent.Invalidate();
		control.LocationChanged += Repaint;
		control.SizeChanged += Repaint;
		control.VisibleChanged += Repaint;
		control.Disposed += (_, _) => frame?.Dispose();
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
