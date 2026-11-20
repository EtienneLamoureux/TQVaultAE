namespace TQVaultAE.GUI.Components.Scaling;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// A standard button (no picture, may be auto-sized) whose font follows the zoom, like the other scaling controls.
/// <see cref="ScalingButton"/> fits its text to its width instead, which an auto-sized button shrinks with it.
/// </summary>
public class ScalingTextButton : Button, IScalingControl, IZoomedFont
{
	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		ZoomScope.Grow(this, factor.Height);

		base.ScaleControl(factor, specified);
	}
}
