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

			#region Apply custom font

			this.itemSeedBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.randomButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.ok.Font = Program.GetFontAlbertusMTLight(12F);
			this.cancel.Font = Program.GetFontAlbertusMTLight(12F);
			this.label1.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.Font = Program.GetFontAlbertusMTLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

			#endregion

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