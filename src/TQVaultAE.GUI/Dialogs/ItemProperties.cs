using TQVaultAE.GUI.Forms;

//-----------------------------------------------------------------------
// <copyright file="ItemProperties.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Dialogs;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;
using TQVaultAE.Domain.Helpers;

/// <summary>
/// Form for the item properties display
/// </summary>
internal sealed partial class ItemProperties : VaultForm
{
	/// <summary>
	/// Item instance of the item we are displaying
	/// </summary>
	/// <summary>Item to show, set before the dialog is shown.</summary>
	public Item Item { get; set; } = null!;

	/// <summary>
	/// Item human readable data
	/// </summary>
	// Read from Item when the dialog loads.
	private ToFriendlyNameResult Data = null!;

	/// <summary>
	/// Initializes a new instance of the ItemProperties class.
	/// </summary>
	public ItemProperties(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		this.InitializeComponent();

		#region Apply custom font

		this.ButtonOK.Font = FontService.GetFontLight(12F);
		this.labelPrefixProperties.Font = FontService.GetFontLight(11.25F);
		this.labelBaseItemProperties.Font = FontService.GetFontLight(11.25F);
		this.checkBoxFilterExtraInfo.Font = FontService.GetFontLight(11.25F);
		this.labelSuffixProperties.Font = FontService.GetFontLight(11.25F);
		this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

		// Apply font to DataGridViews
		var gridFont = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.columnBaseItemProperty.DefaultCellStyle.Font = gridFont;
		this.columnPrefixProperty.DefaultCellStyle.Font = gridFont;
		this.columnSuffixProperty.DefaultCellStyle.Font = gridFont;

		// Set row height
		var rowHeight = (int)(gridFont.Height + 6);
		this.dataGridViewBaseItemProperties.RowTemplate.Height = rowHeight;
		this.dataGridViewPrefixProperties.RowTemplate.Height = rowHeight;
		this.dataGridViewSuffixProperties.RowTemplate.Height = rowHeight;

		#endregion

		this.Text = Resources.ItemPropertiesText;
		this.ButtonOK.Text = Resources.GlobalOK;
		this.labelPrefixProperties.Text = Resources.ItemPropertiesLabelPrefixProperties;
		this.labelBaseItemProperties.Text = Resources.ItemPropertiesLabelBaseItemProperties;
		this.labelSuffixProperties.Text = Resources.ItemPropertiesLabelSuffixProperties;
		this.checkBoxFilterExtraInfo.Text = Resources.ItemPropertiesCheckBoxLabelFilterExtraInfo;

		this.NormalizeBox = false;
		this.DrawCustomBorder = true;
	}

	/// <summary>
	/// Dialog load methond
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ItemProperties_Load(object? sender, EventArgs e)
	{
		this.AlignFilterCheckBox();
		this.LoadProperties();
	}

	/// <summary>
	/// Right-aligns the filter check box with the suffix grid, and keeps the item name left of it: a translated label
	/// longer than "Filter Extra Info" ran past the right edge of the dialog.
	/// </summary>
	private void AlignFilterCheckBox()
	{
		var box = this.checkBoxFilterExtraInfo;
		box.Left = Math.Max(this.labelItemName.Left, this.dataGridViewSuffixProperties.Right - box.PreferredSize.Width);
		if (this.labelItemName.Right > box.Left - box.Margin.Left)
			this.labelItemName.Width = Math.Max(0, box.Left - box.Margin.Left - this.labelItemName.Left);
	}

	/// <summary>
	/// Loads the item properties
	/// </summary>
	private void LoadProperties()
	{
		this.Data = ItemProvider.GetFriendlyNames(this.Item, FriendlyNamesExtraScopes.ItemFullDisplay, this.checkBoxFilterExtraInfo.Checked);

		// ItemName
		this.labelItemName.ForeColor = this.Data.Item.ExtractTextColorOrItemColor(Data.BaseItemInfoDescription);
		this.labelItemName.Text = this.Data.FullNameClean;

		// Base Item Attributes
		if (this.Data.BaseAttributes.Length != 0)
		{
			this.dataGridViewBaseItemProperties.Rows.Clear();

			if (!this.checkBoxFilterExtraInfo.Checked)
				this.AddGridRow(this.dataGridViewBaseItemProperties, this.Data.BaseItemId.Normalized, ItemStyle.Relic.Color());

			foreach (var prop in this.Data.BaseAttributes)
				this.AddGridRow(this.dataGridViewBaseItemProperties, prop);

			this.dataGridViewBaseItemProperties.Show();
			this.labelBaseItemProperties.Show();
		}
		else
		{
			this.dataGridViewBaseItemProperties.Hide();
			this.labelBaseItemProperties.Hide();
		}

		// Prefix Attributes
		if (this.Data.PrefixAttributes.Length != 0)
		{
			this.dataGridViewPrefixProperties.Rows.Clear();

			if (!this.checkBoxFilterExtraInfo.Checked)
				this.AddGridRow(this.dataGridViewPrefixProperties, (this.Data.PrefixInfoRecords?.Id.Normalized ?? string.Empty), ItemStyle.Relic.Color());

			foreach (var prop in this.Data.PrefixAttributes)
				this.AddGridRow(this.dataGridViewPrefixProperties, prop);

			this.dataGridViewPrefixProperties.Show();
			this.labelPrefixProperties.Show();
		}
		else
		{
			this.dataGridViewPrefixProperties.Hide();
			this.labelPrefixProperties.Hide();
		}

		// Suffix Attributes
		if (this.Data.SuffixAttributes.Length != 0)
		{
			this.dataGridViewSuffixProperties.Rows.Clear();

			if (!this.checkBoxFilterExtraInfo.Checked)
				this.AddGridRow(this.dataGridViewSuffixProperties, (this.Data.SuffixInfoRecords?.Id.Normalized ?? string.Empty), ItemStyle.Relic.Color());

			foreach (var prop in this.Data.SuffixAttributes)
				this.AddGridRow(this.dataGridViewSuffixProperties, prop);

			this.dataGridViewSuffixProperties.Show();
			this.labelSuffixProperties.Show();
		}
		else
		{
			this.dataGridViewSuffixProperties.Hide();
			this.labelSuffixProperties.Hide();
		}
	}

	/// <summary>
	/// Adds a row to the specified DataGridView with the given property text.
	/// Handles multi-colored text by extracting the color from tags.
	/// </summary>
	/// <param name="gridView">The DataGridView to add the row to.</param>
	/// <param name="propertyText">The property text, possibly containing color tags.</param>
	/// <param name="defaultColor">The default foreground color to use if no color tags are present.</param>
	private void AddGridRow(DataGridView gridView, string propertyText, Color? defaultColor = null)
	{
		// Replace TQNewLine with regular newline for multi-line support
		propertyText = propertyText.Replace(StringHelper.TQNewLineTag, "\n");

		// Extract color from tag if present, and remove the tag from the text
		var (tqColor, untaggedText) = propertyText.SplitLeadingColorTag();
		Color fgColor = tqColor?.Color() ?? defaultColor ?? Color.White;
		propertyText = untaggedText;

		// Add row with the text value - the column's DefaultCellStyle provides the base styling
		int rowIndex = gridView.Rows.Add(propertyText);

		// Apply the foreground color to the cell
		var cell = gridView.Rows[rowIndex].Cells[0];
		cell.Style.ForeColor = fgColor;
		cell.Style.SelectionForeColor = fgColor;
		cell.Style.SelectionBackColor = Color.FromArgb(60, 45, 30);
	}

	/// <summary>
	/// Handler for the OK button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ButtonOK_Button_Click(object? sender, EventArgs e) => this.Close();

	/// <summary>
	/// Handler for clicking the check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CheckBox1_CheckedChanged(object? sender, EventArgs e)
	{
		System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
		this.LoadProperties();
		System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
	}
}
