namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// Draws a picture of the game's interface (a button, an arrow) stretched to a rectangle: greyed when disabled, flipped
/// or mirrored for the pictures the game draws the other way round. A missing picture is drawn as a grey frame.
/// </summary>
internal static class GamePicture
{
	/// <summary>The frame drawn in place of a missing picture.</summary>
	internal static readonly Color MissingFrame = Color.FromArgb(190, 190, 190);

	/// <summary>Draws <paramref name="picture"/> in <paramref name="to"/>.</summary>
	/// <param name="g">The graphics.</param>
	/// <param name="picture">The picture, null when it could not be loaded.</param>
	/// <param name="to">Where to draw it.</param>
	/// <param name="greyed">Drawn faded, for a disabled button.</param>
	/// <param name="flip">Upside down.</param>
	internal static void Draw(Graphics g, Bitmap? picture, Rectangle to, bool greyed = false, bool flip = false)
	{
		if (picture is null)
		{
			using var pen = new Pen(MissingFrame);
			g.DrawRectangle(pen, to.X, to.Y, to.Width - 1, to.Height - 1);
			return;
		}

		if (flip)
		{
			// The upper left, upper right and lower left corners of the picture, where they land.
			g.DrawImage(picture, [new Point(to.Left, to.Bottom), new Point(to.Right, to.Bottom), new Point(to.Left, to.Top)]);
			return;
		}

		if (!greyed)
		{
			g.DrawImage(picture, to);
			return;
		}

		using var attributes = new System.Drawing.Imaging.ImageAttributes();
		var dim = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.4F };
		attributes.SetColorMatrix(dim);
		g.DrawImage(picture, to, 0, 0, picture.Width, picture.Height, GraphicsUnit.Pixel, attributes);
	}
}
