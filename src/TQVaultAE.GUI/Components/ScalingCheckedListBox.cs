namespace TQVaultAE.GUI.Components
{
	using System;
	using System.Linq;
	using System.Drawing;
	using System.Windows.Forms;
	using TQVaultAE.Presentation;

	/// <summary>
	/// ComboBox class to support scaling of the fonts.
	/// </summary>
	public class ScalingCheckedListBox : System.Windows.Forms.CheckedListBox, IScalingControl
	{
		public ScalingCheckedListBox() 
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			//this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		}


		/// <summary>
		/// Override of ScaleControl which supports font scaling.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * factor.Height, this.Font.Style);
			base.ScaleControl(factor, specified);
		}
	}
}