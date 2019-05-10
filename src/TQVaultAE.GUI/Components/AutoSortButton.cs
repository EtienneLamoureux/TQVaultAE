//-----------------------------------------------------------------------
// <copyright file="AutoSortButton.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using Properties;
	using System;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Displays and handles the auto sort button.
	/// </summary>
	public class AutoSortButton : BagButtonBase
	{
		/// <summary>
		/// Initializes a new instance of the AutoSortButton class.
		/// </summary>
		/// <param name="buttonNumber">Number for this button</param>
		/// <param name="rotateGraphic">bool to signal the button is using a rotated background graphic</param>
		public AutoSortButton(int buttonNumber, bool rotateGraphic) : base(buttonNumber, null, null)
		{
			this.IsVault = rotateGraphic;
			this.CreateBackgroundGraphics();

			// Add handlers for clicking the button since we will be changing the graphic based on the mouse click.
			MouseUp += new MouseEventHandler(this.MouseUpCallback);
			MouseDown += new MouseEventHandler(this.MouseDownCallback);
		}

		/// <summary>
		/// Gets or sets a value indicating whether this button is attached to a vault panel.
		/// </summary>
		public bool IsVault { get; set; }

		/// <summary>
		/// Updates the background graphics for the sort buttons.
		/// </summary>
		public override void CreateBackgroundGraphics()
		{
			this.OffBitmap = Resources.autosortup01;
			this.OnBitmap = Resources.autosortdown01;
			this.OverBitmap = Resources.autosortover01;

			if (this.IsVault)
			{
				this.OffBitmap = Resources.rotatedautosortup01;
				this.OnBitmap = Resources.rotatedautosortdown01;
				this.OverBitmap = Resources.rotatedautosortover01;
			}

			// Scale the button to the size of the graphic.
			this.Height = Convert.ToInt32((float)this.OffBitmap.Height * Database.DB.Scale);
			this.Width = Convert.ToInt32((float)this.OffBitmap.Width * Database.DB.Scale);
		}

		/// <summary>
		/// Handler for clicking the mouse button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		private void MouseDownCallback(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.IsOn = true;
			}
		}

		/// <summary>
		/// Handler for releasing the mouse button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		private void MouseUpCallback(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.IsOn = false;
			}
		}
	}
}