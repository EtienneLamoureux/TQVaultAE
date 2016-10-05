//-----------------------------------------------------------------------
// <copyright file="ItemSeedDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Dialog box class for the Item Seed Dialog
	/// </summary>
	internal partial class ItemSeedDialog : VaultForm
	{
		/// <summary>
		/// MessageBoxOptions for right to left reading.
		/// </summary>
		private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

		/// <summary>
		/// Selected Item
		/// </summary>
		private Item selectedItem;

		/// <summary>
		/// Initializes a new instance of the ItemSeedDialog class.
		/// </summary>
		public ItemSeedDialog()
		{
			this.InitializeComponent();

			this.DrawCustomBorder = true;

			this.Text = Resources.SeedText;
			this.label1.Text = Resources.SeedLabel1;
			this.cancel.Text = Resources.GlobalCancel;
			this.randomButton.Text = Resources.SeedBtnRandom;
			this.ok.Text = Resources.GlobalOK;

			// Set options for Right to Left reading.
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}
		}

		/// <summary>
		/// Gets or sets the selected item
		/// </summary>
		public Item SelectedItem
		{
			get
			{
				return this.selectedItem;
			}

			set
			{
				this.selectedItem = value;
			}
		}

		/// <summary>
		/// Reverts the form back to the original size and UI style.
		/// </summary>
		/// <param name="originalSize">Original size of the form.</param>
		protected override void Revert(Size originalSize)
		{
			this.DrawCustomBorder = false;
			this.ClientSize = originalSize;
			Font originalFont = new Font("Albertus MT", 9.0F);
			this.Font = originalFont;

			this.itemSeedBox.Font = originalFont;
			this.itemSeedBox.Location = new Point(81, 153);
			this.itemSeedBox.Size = new Size(71, 21);

			this.label1.Font = originalFont;
			this.label1.Location = new Point(20, 9);
			this.label1.Size = new Size(300, 98);

			this.randomButton.Revert(new Point(184, 152), new Size(75, 23));
			this.ok.Revert(new Point(53, 211), new Size(75, 23));
			this.cancel.Revert(new Point(213, 211), new Size(75, 23));
		}

		/// <summary>
		/// Handler for the random button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void RandomButtonClicked(object sender, EventArgs e)
		{
			int newSeed = Item.GenerateSeed();
			this.itemSeedBox.Text = newSeed.ToString(CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Cancel button handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// OK button handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OKButton_Click(object sender, EventArgs e)
		{
			// Get the seed value in the textbox
			int newSeed;
			if (int.TryParse(this.itemSeedBox.Text, out newSeed) && newSeed > 0 && newSeed < 0x7fff)
			{
				this.selectedItem.Seed = newSeed;
				this.selectedItem.MarkModified();
				this.Close();
			}
			else
			{
				MessageBox.Show(this, Resources.SeedRange, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
			}
		}

		/// <summary>
		/// Dialog load handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ItemSeedDlg_Load(object sender, EventArgs e)
		{
			this.itemSeedBox.Text = this.selectedItem.Seed.ToString(CultureInfo.CurrentCulture);
			this.itemSeedBox.SelectAll();
			this.itemSeedBox.Focus();
		}
	}
}