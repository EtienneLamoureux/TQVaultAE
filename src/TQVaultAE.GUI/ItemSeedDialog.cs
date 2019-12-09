//-----------------------------------------------------------------------
// <copyright file="ItemSeedDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Globalization;
	using System.Windows.Forms;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Presentation;

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
		public ItemSeedDialog(MainForm instance) : base(instance.ServiceProvider)
		{
			this.Owner = instance;

			this.InitializeComponent();

			#region Apply custom font

			this.itemSeedBox.Font = FontService.GetFontAlbertusMTLight(11.25F);
			this.ButtonRandom.Font = FontService.GetFontAlbertusMTLight(12F);
			this.ButtonOk.Font = FontService.GetFontAlbertusMTLight(12F);
			this.ButtonCancel.Font = FontService.GetFontAlbertusMTLight(12F);
			this.labelInfos.Font = FontService.GetFontAlbertusMTLight(11.25F);
			this.Font = FontService.GetFontAlbertusMTLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

			#endregion

			this.NormalizeBox = false;
			this.DrawCustomBorder = true;

			this.Text = Resources.SeedText;
			this.labelInfos.Text = Resources.SeedLabel1;
			this.ButtonCancel.Text = Resources.GlobalCancel;
			this.ButtonRandom.Text = Resources.SeedBtnRandom;
			this.ButtonOk.Text = Resources.GlobalOK;

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
				this.selectedItem.IsModified = true;
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