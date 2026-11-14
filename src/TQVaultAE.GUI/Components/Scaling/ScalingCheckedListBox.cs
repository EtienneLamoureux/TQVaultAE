namespace TQVaultAE.GUI.Components.Scaling;

using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// ComboBox class to support scaling of the fonts.
/// </summary>
public class ScalingCheckedListBox : System.Windows.Forms.CheckedListBox, IScalingControl, IZoomedFont
{
	public ScalingCheckedListBox() 
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		//this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
	}


	/// <inheritdoc/>
	Font IZoomedFont.ZoomedFont
	{
		get => this.Font;
		set => this.Font = value;
	}

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