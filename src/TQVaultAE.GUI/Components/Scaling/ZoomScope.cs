using System.Runtime.CompilerServices;

namespace TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// Marks a <see cref="Control.Scale(SizeF)"/> made for the user's zoom, as opposed to the DPI scaling Windows Forms does
/// by itself when a form loads.
/// </summary>
/// <remarks>
/// <see cref="Control.Scale(SizeF)"/> scales bounds but not fonts, so the scaling controls grow their fonts in
/// <c>ScaleControl</c>. That is right for a zoom, but fonts sized in points already follow the DPI: growing them during
/// the DPI scaling too would apply it twice.
/// </remarks>
internal static class ZoomScope
{
	[ThreadStatic]
	private static int depth;

	/// <summary>Gets a value indicating whether a zoom is being applied on this thread.</summary>
	public static bool IsZooming => depth > 0;

	/// <summary>Starts a zoom, until the result is disposed.</summary>
	public static Scope Enter()
	{
		depth++;
		return default;
	}

	/// <summary>
	/// <paramref name="font"/> grown by <paramref name="factor"/> during a zoom, given to <paramref name="owner"/> (see
	/// <see cref="Give"/>); <paramref name="font"/> itself otherwise.
	/// </summary>
	public static Font ScaleFont(Control owner, Font font, float factor)
		=> IsZooming && factor != 1.0F ? Give(owner, new Font(font.FontFamily, font.SizeInPoints * factor, font.Style)) : font;

	/// <summary>Grows the font of <paramref name="control"/> by <paramref name="factor"/> during a zoom.</summary>
	public static void Grow(Control control, float factor) => control.Font = ScaleFont(control, control.Font, factor);

	/// <summary>
	/// Records <paramref name="font"/>, made by the zoom, as the font of <paramref name="owner"/>: the font the zoom gave it
	/// before is disposed when the outermost zoom ends, unless the control still shows it. Each zoom made a new font per
	/// scaling control and dropped the previous one undisposed (about 600 per zoom step in the main window).
	/// </summary>
	public static Font Give(Control owner, Font font)
	{
		if (Given.TryGetValue(owner, out var previous) && !ReferenceEquals(previous, font))
			Replaced.Add((owner, previous));

		// Released at the end of the zoom, the snapshot giving the fonts back just before it, with no zoom started yet.
		Given.AddOrUpdate(owner, font);
		return font;
	}

	/// <summary>The font each control was last given by the zoom.</summary>
	private static readonly ConditionalWeakTable<Control, Font> Given = new();

	/// <summary>The fonts the zoom gave and then replaced, disposed once unused.</summary>
	private static readonly List<(Control Owner, Font Font)> Replaced = [];

	private static void ReleaseReplaced()
	{
		foreach (var (owner, font) in Replaced)
		{
			bool shown = ReferenceEquals(owner.Font, font)
				|| (owner is IZoomedFont zoomed && ReferenceEquals(zoomed.ZoomedFont, font))
				|| (Given.TryGetValue(owner, out var current) && ReferenceEquals(current, font));
			if (!shown)
				font.Dispose();
		}

		Replaced.Clear();
	}

	/// <summary>Ends the zoom started by <see cref="Enter"/>.</summary>
	public readonly struct Scope : IDisposable
	{
		public void Dispose()
		{
			if (--depth == 0)
				ReleaseReplaced();
		}
	}
}
