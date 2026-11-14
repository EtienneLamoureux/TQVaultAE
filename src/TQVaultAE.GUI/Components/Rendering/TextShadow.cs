namespace TQVaultAE.GUI.Components.Rendering;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// The dark copies drawn under light text to keep it readable over a textured background: a drop shadow for the labels
/// and check boxes over the marble, an outline for the captions over a frame's gold border.
/// </summary>
internal static class TextShadow
{
	/// <summary>The color of the shadow and of the outline.</summary>
	public static readonly Color Color = Color.FromArgb(20, 12, 6);

	/// <summary>Draws the drop shadow of <paramref name="text"/>: a dark copy one pixel lower right.</summary>
	public static void DrawShadow(Graphics g, string text, Font font, Rectangle bounds, TextFormatFlags flags)
		=> DrawCopy(g, text, font, bounds, flags, 1, 1);

	/// <summary>
	/// Draws a dark outline one pixel around <paramref name="text"/> (its copies in the eight directions), plus a drop
	/// shadow one pixel further lower right: the light text drawn over it then stays readable on gold, black and marble.
	/// </summary>
	public static void DrawOutline(Graphics g, string text, Font font, Rectangle bounds, TextFormatFlags flags)
	{
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				if (dx != 0 || dy != 0)
					DrawCopy(g, text, font, bounds, flags, dx, dy);
			}
		}

		DrawCopy(g, text, font, bounds, flags, 2, 2);
	}

	/// <summary>
	/// Draws the outline of <paramref name="text"/> (<see cref="DrawOutline"/>) when <paramref name="outline"/> is set,
	/// else its drop shadow (<see cref="DrawShadow"/>).
	/// </summary>
	public static void Draw(Graphics g, string text, Font font, Rectangle bounds, TextFormatFlags flags, bool outline)
	{
		if (outline)
			DrawOutline(g, text, font, bounds, flags);
		else
			DrawShadow(g, text, font, bounds, flags);
	}

	private static void DrawCopy(Graphics g, string text, Font font, Rectangle bounds, TextFormatFlags flags, int dx, int dy)
	{
		bounds.Offset(dx, dy);
		TextRenderer.DrawText(g, text, font, bounds, Color, flags | TextFormatFlags.PreserveGraphicsClipping);
	}
}
