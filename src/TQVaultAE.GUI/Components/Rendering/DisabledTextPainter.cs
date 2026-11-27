namespace TQVaultAE.GUI.Components.Rendering;

using System.Drawing;

/// <summary>
/// Colours for the text of disabled controls on the dark UI. The system disabled colour is a dark gray that is
/// not readable on the brown background, so light text is dimmed instead.
/// </summary>
internal static class DisabledTextPainter
{
	private const float DimFactor = 0.65f;

	/// <summary>
	/// <c>true</c> when the text is light enough to be dimmed; dark text keeps the system rendering.
	/// </summary>
	public static bool Applies(Color foreColor)
		=> foreColor.A > 0 && foreColor.GetBrightness() >= 0.5f;

	/// <summary>
	/// Dimmed version of <paramref name="foreColor"/> used while the control is disabled.
	/// </summary>
	public static Color ColorFor(Color foreColor)
		=> Color.FromArgb(
			(int)(foreColor.R * DimFactor),
			(int)(foreColor.G * DimFactor),
			(int)(foreColor.B * DimFactor));
}
