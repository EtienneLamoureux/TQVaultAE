using System.Drawing;

namespace TQVaultAE.Application.Layout;

/// <summary>
/// The floating character window's content in one row: the equipment slots, then the four statistics tabs side by
/// side (I, II, III, Buffs), each part vertically centred on the tallest. Tab I's gold bar, wider than its panel, goes
/// on under tab II. Sizes are given at scale 1 (design pixels).
/// </summary>
public static class CharacterWindowLayout
{
	/// <summary>The statistics tabs shown at once.</summary>
	public const int StatsPanels = 4;

	/// <summary>The gap between two parts, in design pixels.</summary>
	public const int Gap = 6;

	/// <summary>The parts' places in the content area at <paramref name="scale"/>, and the content's size.</summary>
	/// <param name="slots">The equipment slots view at scale 1.</param>
	/// <param name="first">Tab I's panel at scale 1, its gold bar under it (wider than the panel).</param>
	/// <param name="stats">A tab's panel at scale 1 without the gold bar: the step between two tabs.</param>
	/// <param name="scale">The window's scale: the user's zoom times its monitor's DPI factor, fitted to the screen.</param>
	public static (Rectangle Slots, IReadOnlyList<Rectangle> Stats, Size Content) Arrange(Size slots, Size first, Size stats, float scale)
	{
		var slotsSize = WindowLayout.Scaled(slots, scale);
		var firstSize = WindowLayout.Scaled(first, scale);
		var statsSize = WindowLayout.Scaled(stats, scale);
		int gap = (int)Math.Round(Gap * scale);
		int height = Math.Max(slotsSize.Height, Math.Max(firstSize.Height, statsSize.Height));

		var slotsBounds = new Rectangle(new Point(0, (height - slotsSize.Height) / 2), slotsSize);
		var panels = new List<Rectangle>(StatsPanels);
		int x = slotsBounds.Right;
		for (int i = 0; i < StatsPanels; i++)
		{
			var size = i == 0 ? firstSize : statsSize;
			x += gap;
			panels.Add(new Rectangle(new Point(x, (height - size.Height) / 2), size));
			x += statsSize.Width;
		}

		return (slotsBounds, panels, new Size(Math.Max(x, panels[0].Right), height));
	}

	/// <summary>
	/// <paramref name="scale"/>, made smaller when the content would not fit in <paramref name="limit"/> (the screen's
	/// working area less the window's border): the window never grows past its screen.
	/// </summary>
	public static float FitScale(float scale, Size slots, Size first, Size stats, Size limit)
	{
		var natural = Arrange(slots, first, stats, 1F).Content;
		if (natural.Width <= 0 || natural.Height <= 0 || limit.Width <= 0 || limit.Height <= 0)
			return scale;

		return Math.Min(scale, Math.Min(limit.Width / (float)natural.Width, limit.Height / (float)natural.Height));
	}
}
