//-----------------------------------------------------------------------
// <copyright file="StashButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Properties;
using TQVaultAE.GUI.Tooltip;

namespace TQVaultAE.GUI.Components
{
	/// <summary>
	/// Extends the BagButton functionality to the stash panel.
	/// </summary>
	public class StashButton : BagButtonBase
	{
		/// <summary>
		/// Initializes a new instance of the StashButton class.
		/// </summary>
		/// <param name="bagNumber">number of the bag for display</param>
		/// <param name="getToolTip">Tooltip delegate</param>
		/// <param name="tooltip">Tooltip instance</param>
		public StashButton(int bagNumber, GetToolTip getToolTip, TTLib tooltip) : base(bagNumber, getToolTip, tooltip)
		{
			// Override the default background graphics.
			this.CreateBackgroundGraphics();
		}

		/// <summary>
		/// Updates the background graphics for the stash buttons.
		/// </summary>
		public override void CreateBackgroundGraphics()
		{
			this.OnBitmap = Resources.StashTabUp;
			this.OffBitmap = Resources.StashTabDown;
			this.OverBitmap = Resources.StashTabOver;
		}
	}
}