namespace TQVaultAE.GUI.Components.Scaling;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// CheckedListBox class to support scaling of the fonts.
/// </summary>
public class ScalingCheckedListBox : System.Windows.Forms.CheckedListBox, IScalingControl, IZoomedFont
{
	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
	}
}