//-----------------------------------------------------------------------
// <copyright file="BagButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI.Components
{
	using Properties;
	using Tooltip;

	/// <summary>
	/// Used to create sack bag buttons.
	/// </summary>
	public class BagButton : BagButtonBase
	{
		/// <summary>
		/// Initializes a new instance of the BagButton class.
		/// </summary>
		/// <param name="bagNumber">number of the bag for display</param>
		/// <param name="getToolTip">Tooltip delegate</param>
		/// <param name="tooltip">Tooltip instance</param>
		public BagButton(int bagNumber, GetToolTip getToolTip, TTLib tooltip) : base(bagNumber, getToolTip, tooltip)
		{
		}

		/// <summary>
		/// Sets the background bitmaps for the BagButton
		/// </summary>
		public override void CreateBackgroundGraphics()
		{
			this.OnBitmap = Resources.inventorybagup01;
			this.OffBitmap = Resources.inventorybagdown01;
			this.OverBitmap = Resources.inventorybagover01;
		}
	}
}