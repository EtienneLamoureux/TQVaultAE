namespace TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// Shrinks a text to fit its control: the one search the drawn buttons share (<see cref="ScalingButton"/>, the bag
/// buttons).
/// </summary>
internal static class TextFit
{
	/// <summary>
	/// The largest font size from <paramref name="start"/> down, by <paramref name="step"/>, at which
	/// <paramref name="fits"/> is true; <paramref name="min"/> or just above when none fits, never below it (a font of size 0
	/// throws).
	/// </summary>
	public static float LargestSize(float start, float step, float min, Func<float, bool> fits)
	{
		float size = start;
		while (size - step >= min && !fits(size))
			size -= step;

		return size;
	}

	/// <summary>
	/// <paramref name="font"/> at the largest size, by steps of <paramref name="step"/> down to <paramref name="min"/>, at
	/// which <paramref name="fits"/> is true: <paramref name="font"/> itself when it fits, otherwise a new font the caller
	/// owns. The fonts tried on the way are disposed.
	/// </summary>
	public static Font Shrink(Font font, float step, float min, Func<Font, bool> fits)
	{
		float size = LargestSize(font.Size, step, min, s =>
		{
			if (s == font.Size)
				return fits(font);

			using var trial = SizedLike(font, s);
			return fits(trial);
		});

		return size == font.Size ? font : SizedLike(font, size);
	}

	/// <summary>
	/// <paramref name="font"/> at <paramref name="size"/>, made from its family: the game fonts are private, a font made
	/// from their name was Microsoft Sans Serif.
	/// </summary>
	private static Font SizedLike(Font font, float size) => new(font.FontFamily, size, font.Style, font.Unit);
}
