using System.Drawing.Drawing2D;

namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// The custom border's close button, drawn at the size it is shown instead of a 26 pixel picture stretched by the zoom
/// and the screen DPI: a gold frame, a red square and a white cross, in the look of the game's close buttons.
/// </summary>
internal static class CloseButtonArt
{
	/// <summary>The state the button is drawn in.</summary>
	public enum State
	{
		Up,
		Over,
		Down,
	}

	/// <summary>The colours of one state: gold frame, red square and cross.</summary>
	private readonly record struct Palette(
		Color GoldLight, Color Gold, Color GoldDark, Color Gap,
		Color RedTop, Color RedBottom, Color RedLight, Color RedShadow,
		Color Cross, Color CrossHalo);

	private static Palette PaletteOf(State state) => state switch
	{
		State.Over => new(
			Color.FromArgb(0xE6, 0xC2, 0x5E), Color.FromArgb(0xD8, 0xB0, 0x47), Color.FromArgb(0xB0, 0x8D, 0x36), Color.FromArgb(0x5E, 0x47, 0x1D),
			Color.FromArgb(0xD3, 0x0C, 0x0C), Color.FromArgb(0xE8, 0x40, 0x3C), Color.FromArgb(0xF7, 0xB7, 0xB7), Color.FromArgb(0x8A, 0x10, 0x10),
			Color.FromArgb(0xFE, 0xFE, 0xFE), Color.FromArgb(0xF8, 0xC2, 0xC2)),
		State.Down => new(
			Color.FromArgb(0xC4, 0x99, 0x3E), Color.FromArgb(0xB8, 0x92, 0x38), Color.FromArgb(0x8E, 0x70, 0x2A), Color.FromArgb(0x1F, 0x16, 0x0A),
			Color.FromArgb(0x46, 0x04, 0x04), Color.FromArgb(0x6A, 0x17, 0x17), Color.FromArgb(0x84, 0x69, 0x69), Color.FromArgb(0x2A, 0x02, 0x02),
			Color.FromArgb(0x8E, 0x8D, 0x8D), Color.FromArgb(0x70, 0x42, 0x42)),
		_ => new(
			Color.FromArgb(0xDB, 0xB4, 0x50), Color.FromArgb(0xD2, 0xA6, 0x3A), Color.FromArgb(0xA5, 0x82, 0x31), Color.FromArgb(0x3A, 0x2C, 0x10),
			Color.FromArgb(0x76, 0x07, 0x07), Color.FromArgb(0xB1, 0x26, 0x25), Color.FromArgb(0xB9, 0x6F, 0x6F), Color.FromArgb(0x4A, 0x04, 0x04),
			Color.FromArgb(0xED, 0xED, 0xED), Color.FromArgb(0xD3, 0xBD, 0xBD)),
	};

	/// <summary>
	/// Draws the button in a square bitmap of <paramref name="size"/> pixels. Proportions are those of the 26 pixel
	/// design: a gold frame of 3, a dark gap of 2, then the red square with its cross.
	/// </summary>
	public static Bitmap Render(int size, State state)
	{
		size = Math.Max(size, 8);
		var p = PaletteOf(state);
		var bitmap = new Bitmap(size, size);
		using var g = Graphics.FromImage(bitmap);
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;

		float unit = size / 26F;
		int frame = Math.Max(2, (int)Math.Round(3 * unit));
		int gap = Math.Max(1, (int)Math.Round(2 * unit));
		int bevel = Math.Max(1, (int)Math.Round(unit));

		// Gold frame: light on its outer top and left edges, darker on its inner edge, as a bevel.
		var outer = new Rectangle(0, 0, size, size);
		using (var gold = new LinearGradientBrush(outer, p.GoldLight, p.GoldDark, LinearGradientMode.ForwardDiagonal))
		{
			gold.SetBlendTriangularShape(0.5F, 0.35F);
			g.FillRectangle(gold, outer);
		}
		using (var edge = new SolidBrush(p.GoldDark))
		{
			var inner = Rectangle.Inflate(outer, -(frame - bevel), -(frame - bevel));
			g.FillRectangle(edge, inner);
		}

		// Dark gap between the frame and the red square.
		var gapRect = Rectangle.Inflate(outer, -frame, -frame);
		using (var dark = new SolidBrush(p.Gap))
			g.FillRectangle(dark, gapRect);

		// Red square: darker at the top, lighter at the bottom, with a light bevel on the top and left edges and a
		// shadow on the others.
		var red = Rectangle.Inflate(gapRect, -gap, -gap);
		if (red.Width <= 0 || red.Height <= 0)
			return bitmap;

		using (var light = new SolidBrush(p.RedLight))
			g.FillRectangle(light, red);
		using (var shadow = new SolidBrush(p.RedShadow))
			g.FillRectangle(shadow, red.X + bevel, red.Y + bevel, red.Width - bevel, red.Height - bevel);
		var body = new Rectangle(red.X + bevel, red.Y + bevel, red.Width - 2 * bevel, red.Height - 2 * bevel);
		if (body.Width > 0 && body.Height > 0)
		{
			using var fill = new LinearGradientBrush(body, p.RedTop, p.RedBottom, LinearGradientMode.Vertical);
			g.FillRectangle(fill, body);
		}

		// The cross, a soft halo under a white stroke, inset from the red square.
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float inset = red.Width * 0.22F;
		var a = new PointF(red.Left + inset, red.Top + inset);
		var b = new PointF(red.Right - inset, red.Bottom - inset);
		var c = new PointF(red.Right - inset, red.Top + inset);
		var d = new PointF(red.Left + inset, red.Bottom - inset);
		float stroke = Math.Max(1.5F, 2.6F * unit);
		using (var halo = new Pen(p.CrossHalo, stroke + Math.Max(1F, unit)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
		{
			g.DrawLine(halo, a, b);
			g.DrawLine(halo, c, d);
		}
		using (var cross = new Pen(p.Cross, stroke) { StartCap = LineCap.Round, EndCap = LineCap.Round })
		{
			g.DrawLine(cross, a, b);
			g.DrawLine(cross, c, d);
		}

		return bitmap;
	}
}
