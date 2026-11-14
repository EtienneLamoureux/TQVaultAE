using System.Drawing;

namespace TQVaultAE.Application;

/// <summary>
/// The zoom steps of the main window: 0.4 to 2.0 by 0.1 (nominal zooms), each made exact by a whole item cell size. At a
/// step the scale (pixels per design pixel) is the cell size over the design cell size, so everything sized from the
/// scale agrees with the cells: 96, 144 or 168 dpi round the cell as 120 dpi does not (32 x 0.7 = 22.4), and the panels
/// left uneven gaps around their bags.
/// </summary>
public static class ZoomSteps
{
	/// <summary>The item cell size in design pixels (96 dpi, zoom 1).</summary>
	public const int DesignCell = 32;

	/// <summary>The number of steps: 0.4 to 2.0 by 0.1.</summary>
	public const int Count = 17;

	/// <summary>The step of zoom 1.</summary>
	public const int Reset = 6;

	/// <summary>The nominal zoom of <paramref name="step"/> (0.4 for step 0, 1 for <see cref="Reset"/>): the zoom saved.</summary>
	public static float Nominal(int step) => (4 + Clamp(step)) / 10F;

	/// <summary>
	/// The item cell size in pixels at <paramref name="step"/> on a monitor at <paramref name="dpiScale"/> times the design
	/// DPI: the nominal zoom's cell, rounded as the cells always were.
	/// </summary>
	public static int Cell(int step, float dpiScale) => Convert.ToInt32(DesignCell * (Nominal(step) * dpiScale));

	/// <summary>The scale at <paramref name="step"/>: the cell over the design cell, exact in binary.</summary>
	public static float Scale(int step, float dpiScale) => Cell(step, dpiScale) / (float)DesignCell;

	/// <summary>The step nearest to <paramref name="zoom"/> (a saved zoom, or the zoom a window size gives).</summary>
	public static int Nearest(float zoom)
		=> Clamp((int)Math.Round((zoom - Nominal(0)) * 10F, MidpointRounding.AwayFromZero));

	/// <summary>
	/// The largest step at which <paramref name="design"/> (a size in design pixels) at the step's scale fits inside
	/// <paramref name="limit"/>; the first step when none does.
	/// </summary>
	public static int LargestFitting(Size design, float dpiScale, Size limit)
	{
		for (int step = Count - 1; step > 0; step--)
		{
			var size = WindowLayout.Scaled(design, Scale(step, dpiScale));
			if (size.Width <= limit.Width && size.Height <= limit.Height)
				return step;
		}

		return 0;
	}

	private static int Clamp(int step) => Math.Clamp(step, 0, Count - 1);
}
