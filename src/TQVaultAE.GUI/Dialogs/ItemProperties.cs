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
	/// The label and grid of each property section, by <see cref="ItemPropertySectionKind"/>.
	/// </summary>
	private readonly (Label Label, DataGridView Grid, DataGridViewColumn Column)[] sectionControls;

	/// <summary>
	/// Initializes a new instance of the ItemProperties class.
	/// </summary>
	public ItemProperties(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		this.InitializeComponent();

		this.sectionControls =
		[
			(this.labelBaseItemProperties, this.dataGridViewBaseItemProperties, this.columnBaseItemProperty),
			(this.labelPrefixProperties, this.dataGridViewPrefixProperties, this.columnPrefixProperty),
			(this.labelSuffixProperties, this.dataGridViewSuffixProperties, this.columnSuffixProperty),
		];

		#region Apply custom font

		this.ButtonOK.Font = FontService.GetFontLight(12F);
		this.checkBoxFilterExtraInfo.Font = FontService.GetFontLight(11.25F);
		this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

		// Apply font to DataGridViews
		var gridFont = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		var rowHeight = (int)(gridFont.Height + 6);
		foreach (var (label, grid, column) in this.sectionControls)
		{
			label.Font = FontService.GetFontLight(11.25F);
			column.DefaultCellStyle.Font = gridFont;
			grid.RowTemplate.Height = rowHeight;
		}

		#endregion

		this.Text = Resources.ItemPropertiesText;
		this.ButtonOK.Text = Resources.GlobalOK;
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
		var filterExtra = this.checkBoxFilterExtraInfo.Checked;
		var content = ItemPropertiesContent.Build(
			ItemProvider.GetFriendlyNames(this.Item, FriendlyNamesExtraScopes.ItemFullDisplay, filterExtra), filterExtra);

		this.labelItemName.ForeColor = content.NameColor;
		this.labelItemName.Text = content.Name;

		foreach (var section in content.Sections)
		{
			var (label, grid, _) = this.sectionControls[(int)section.Kind];
			label.Text = section.Title;

			if (!section.Visible)
			{
				grid.Hide();
				label.Hide();
				continue;
			}

			grid.Rows.Clear();
			foreach (var row in section.Rows)
				AddGridRow(grid, row);

			grid.Show();
			label.Show();
		}
	}

	/// <summary>
	/// Adds <paramref name="row"/> to <paramref name="gridView"/> in its color.
	/// </summary>
	private static void AddGridRow(DataGridView gridView, ItemPropertyRow row)
	{
		// The column's DefaultCellStyle provides the base styling
		int rowIndex = gridView.Rows.Add(row.Text);

		var cell = gridView.Rows[rowIndex].Cells[0];
		cell.Style.ForeColor = row.Color;
		cell.Style.SelectionForeColor = row.Color;
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
