using TQVaultAE.GUI.Forms;

//-----------------------------------------------------------------------
// <copyright file="ItemSeedDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Dialogs;

using System;
using System.Globalization;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

/// <summary>
/// Dialog box class for the Item Seed Dialog
/// </summary>
internal sealed partial class ItemSeedDialog : VaultForm
{
	/// <summary>
	/// Selected Item
	/// </summary>
	// Set before the dialog is shown.
	private Item selectedItem = null!;

	/// <summary>
	/// Initializes a new instance of the ItemSeedDialog class.
	/// </summary>
	public ItemSeedDialog(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		this.InitializeComponent();

		#region Apply custom font

		this.itemSeedBox.Font = FontService.GetFontLight(11.25F);
		this.ButtonRandom.Font = FontService.GetFontLight(12F);
		this.ButtonOk.Font = FontService.GetFontLight(12F);
		this.ButtonCancel.Font = FontService.GetFontLight(12F);
		this.labelInfos.Font = FontService.GetFontLight(11.25F);
		this.Font = FontService.GetFontLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

		#endregion

		this.NormalizeBox = false;
		this.DrawCustomBorder = true;

		this.Text = Resources.SeedText;
		this.labelInfos.Text = Resources.SeedLabel1;
		this.ButtonCancel.Text = Resources.GlobalCancel;
		this.ButtonRandom.Text = Resources.SeedBtnRandom;
		this.ButtonOk.Text = Resources.GlobalOK;

		// The English text has its own line breaks; longer translations wrap within the dialog instead of being cut.
		this.labelInfos.MaximumSize = new System.Drawing.Size(this.ClientSize.Width - (2 * this.labelInfos.Left), 0);
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
	/// Gets the valid seed accepted with OK, or <see langword="null"/> when the dialog was cancelled.
	/// </summary>
	public int? NewSeed { get; private set; }

	/// <summary>
	/// Handler for the random button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void RandomButtonClicked(object? sender, EventArgs e)
	{
		int newSeed = Item.GenerateSeed();
		this.itemSeedBox.Text = newSeed.ToString(CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Cancel button handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButton_Click(object? sender, EventArgs e)
	{
		this.Close();
	}

	/// <summary>
	/// OK button handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OKButton_Click(object? sender, EventArgs e)
	{
		// Get the seed value in the textbox
		if (int.TryParse(this.itemSeedBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out int newSeed) && Item.IsValidSeed(newSeed))
		{
			this.NewSeed = newSeed;
			this.Close();
		}
		else
		{
			this.Services.MessageBoxes.Inform(Resources.SeedRange, Resources.GlobalError, owner: this);
		}
	}

	/// <summary>
	/// Dialog load handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ItemSeedDlg_Load(object? sender, EventArgs e)
	{
		this.itemSeedBox.Text = this.selectedItem.Seed.ToString(CultureInfo.CurrentCulture);
		this.itemSeedBox.SelectAll();
		this.itemSeedBox.Focus();
	}
}