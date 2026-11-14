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
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

/// <summary>
/// Changes the seed of an item. A TQ window like Vault Maintenance: marble, the explanation in a caravan frame, gold
/// buttons; laid out in code by a table, at the user's zoom, and following the DPI.
/// </summary>
internal sealed class ItemSeedDialog : VaultForm
{
	/// <summary>Largest width of the explanation, frame included, at zoom 1: a longer line wraps.</summary>
	private const int ContentWidth = 600;

	private readonly ScalingLabel labelInfos;
	private readonly ScalingTextBox itemSeedBox;
	private readonly ScalingButton ButtonRandom;
	private readonly ScalingButton ButtonOk;
	private readonly ScalingButton ButtonCancel;

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

		float zoom = this.UIService.Zoom;
		int Zoomed(int value) => (int)Math.Round(value * zoom);
		var font = this.FontService.GetFontLight(CaravanLook.TextSize, zoom);

		// The explanation in a caravan frame, one character of space inside it on the left and right, as the instructions
		// of Vault Maintenance. The English text has its own line breaks; a longer translation wraps within the frame.
		this.labelInfos = new ScalingLabel()
		{
			Name = nameof(this.labelInfos),
			AutoSize = true,
			Dock = DockStyle.Fill,
			Font = font,
			Margin = new Padding(0, 0, 0, Zoomed(10)),
			MaximumSize = new Size(Zoomed(ContentWidth), 0),
			Text = Resources.SeedLabel1,
			UseMnemonic = false,
		};
		int space = CaravanFrame.Border + CaravanFrame.CharacterWidth(font);
		CaravanFrame.ApplyTo(this.labelInfos, new Padding(space, CaravanFrame.Border, space, CaravanFrame.Border));

		this.itemSeedBox = new ScalingTextBox()
		{
			Name = nameof(this.itemSeedBox),
			Anchor = AnchorStyles.None,
			Font = font,
			Margin = new Padding(Zoomed(3), 0, Zoomed(3), 0),
			MaxLength = 5,
		};

		this.ButtonRandom = CaravanDialog.Button(this.FontService, zoom, nameof(this.ButtonRandom), Resources.SeedBtnRandom);
		this.ButtonRandom.Click += this.RandomButtonClicked;
		this.ButtonOk = CaravanDialog.Button(this.FontService, zoom, nameof(this.ButtonOk), Resources.GlobalOK);
		this.ButtonOk.Click += this.OKButton_Click;
		this.ButtonCancel = CaravanDialog.Button(this.FontService, zoom, nameof(this.ButtonCancel), Resources.GlobalCancel);
		this.ButtonCancel.DialogResult = DialogResult.Cancel;
		// Also closes the dialog shown modeless (automation).
		this.ButtonCancel.Click += (_, _) => this.Close();

		// The seed box as wide as a button, beside Random; OK and Cancel under them.
		var seedRow = NewRow(0);
		seedRow.Controls.AddRange([this.itemSeedBox, this.ButtonRandom]);
		var buttons = NewRow(Zoomed(14));
		buttons.Controls.AddRange([this.ButtonOk, this.ButtonCancel]);
		this.itemSeedBox.Width = this.ButtonRandom.Width;

		var layout = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			Margin = new Padding(0),
		};
		layout.Controls.AddRange([this.labelInfos, seedRow, buttons]);

		this.SuspendLayout();
		this.AutoScaleDimensions = new SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.BackColor = Color.FromArgb(46, 31, 21);
		this.AcceptButton = this.ButtonOk;
		this.CancelButton = this.ButtonCancel;
		this.Font = font;
		this.ForeColor = Color.White;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.NormalizeBox = false;
		this.Name = nameof(ItemSeedDialog);
		this.ShowIcon = false;
		this.ShowInTaskbar = false;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = Resources.SeedText;
		this.TopMost = true;
		this.Padding = CaravanDialog.ContentPadding(zoom);
		layout.Location = new Point(this.Padding.Left, this.Padding.Top);
		this.Controls.Add(layout);
		this.DrawCustomBorder = true;
		CaravanLook.Apply(this, layout, seedRow, buttons);
		this.ResumeLayout(false);
		this.PerformLayout();

		this.FitAround(layout);
		this.Load += this.ItemSeedDlg_Load;

		static FlowLayoutPanel NewRow(int top) => new()
		{
			Anchor = AnchorStyles.None,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			Margin = new Padding(0, top, 0, 0),
			WrapContents = false,
		};
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
		// The keyboard starts in the seed box, not on the close button of the border.
		this.ActiveControl = this.itemSeedBox;
	}
}
