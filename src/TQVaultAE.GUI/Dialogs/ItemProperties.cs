using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;

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
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Presentation;

/// <summary>
/// Form for the item properties display
/// </summary>
internal sealed partial class ItemProperties : VaultForm
{
	/// <summary>
	/// How a property line is drawn and measured: wrapped at the column's width, a word longer than the width (a record
	/// path) broken where it reaches the edge, as a multiline text box does, so that no line is cut.
	/// </summary>
	private const TextFormatFlags RowTextFlags = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak
		| TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix;

	/// <summary>Item to show, set before the dialog is shown.</summary>
	public Item Item { get; set; } = null!;

	/// <summary>
	/// The group (its caption the section's title) and grid of each property section, by <see cref="ItemPropertySectionKind"/>.
	/// </summary>
	private readonly (GroupBox Caption, DataGridView Grid, DataGridViewColumn Column)[] sectionControls;

	/// <summary>The grids whose row heights are fitted again once their column has finished changing width.</summary>
	private readonly HashSet<DataGridView> rowFitsPending = [];

	/// <summary>
	/// Initializes a new instance of the ItemProperties class.
	/// </summary>
	public ItemProperties(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		this.InitializeComponent();

		this.sectionControls =
		[
			(this.groupBoxBaseItemProperties, this.dataGridViewBaseItemProperties, this.columnBaseItemProperty),
			(this.groupBoxPrefixProperties, this.dataGridViewPrefixProperties, this.columnPrefixProperty),
			(this.groupBoxSuffixProperties, this.dataGridViewSuffixProperties, this.columnSuffixProperty),
		];

		#region Apply custom font

		// One rule, as in the settings dialog: the button in the larger size, the item name as the title, the rest in the
		// base size. The scaling controls hold their font (they grow it with the zoom); the group boxes and the grids'
		// lines take the dialog's.
		var baseFont = FontService.GetFontLight(CaravanLook.TextSize);
		this.Font = baseFont;
		this.ProcessAllControls(c =>
		{
			if (c is Button)
				c.Font = FontService.GetFontLight(CaravanLook.ButtonTextSize);
			else if (c is IScalingControl)
				c.Font = baseFont;
		});
		this.labelItemName.Font = FontService.GetFontLight(18F);

		#endregion

		this.Text = Resources.ItemPropertiesText;
		this.ButtonOK.Text = Resources.GlobalOK;
		this.checkBoxFilterExtraInfo.Text = Resources.ItemPropertiesCheckBoxLabelFilterExtraInfo;

		// The item name in a caravan frame, one character of space from its edge; each section a framed group with its
		// title over the frame's top edge, its lines in their game colours on the black.
		int space = CaravanFrame.Border + CaravanFrame.CharacterWidth(this.labelItemName.Font);
		CaravanFrame.ApplyTo(this.labelItemName, new Padding(space, 0, space, 0));
		CaravanLook.Apply(this);

		foreach (var (_, grid, column) in this.sectionControls)
		{
			grid.CellPainting += GridCellPainting;
			grid.ColumnWidthChanged += (_, _) => this.FitRowHeightsLater(grid, column);
			grid.FontChanged += (_, _) => this.FitRowHeightsLater(grid, column);
		}

		this.NormalizeBox = false;
		this.DrawCustomBorder = true;

		// The user's zoom, as the other dialogs get it; Windows Forms adds the DPI when the dialog loads.
		float zoom = this.UIService.Zoom;
		using (ZoomScope.Enter())
			this.Scale(new SizeF(zoom, zoom));
	}

	/// <summary>
	/// Override of ScaleControl which grows the dialog's font with the zoom: the group captions and the property lines
	/// use it.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
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
	/// Right-aligns the filter check box with the suffix group, centred on the item name's frame, and keeps the item name
	/// left of it: a translated label longer than "Filter Extra Info" ran past the right edge of the dialog.
	/// </summary>
	private void AlignFilterCheckBox()
	{
		var box = this.checkBoxFilterExtraInfo;
		var name = this.labelItemName;
		box.Left = Math.Max(name.Left, this.groupBoxSuffixProperties.Right - box.PreferredSize.Width);
		box.Top = name.Top + ((name.Height - box.Height) / 2);
		int right = box.Left - box.Margin.Left - name.Margin.Right;
		if (name.Right > right)
			name.Width = Math.Max(0, right - name.Left);
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
			var (caption, grid, column) = this.sectionControls[(int)section.Kind];
			caption.Text = section.Title;

			if (!section.Visible)
			{
				grid.Hide();
				caption.Hide();
				continue;
			}

			grid.Rows.Clear();
			foreach (var row in section.Rows)
				AddGridRow(grid, row);

			grid.Show();
			caption.Show();
			this.FitRowHeights(grid, column);
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

	/// <summary>The space around a property line in its row, from the line's font.</summary>
	private static Padding RowPadding(Font font)
		=> new(Math.Max(1, font.Height / 8), Math.Max(1, font.Height / 6), Math.Max(1, font.Height / 8), Math.Max(1, font.Height / 6));

	/// <summary>
	/// Gives each row of <paramref name="grid"/> the height of its line wrapped at the column's width (one line at
	/// least). The new heights may show or hide the scroll bar, which changes that width: the heights are then fitted
	/// again (<see cref="FitRowHeightsLater"/>). They only grow as the column narrows, so this settles.
	/// </summary>
	private void FitRowHeights(DataGridView grid, DataGridViewColumn column)
	{
		var font = grid.Font;
		var padding = RowPadding(font);
		int lineWidth = Math.Max(1, column.Width - padding.Horizontal);
		int minimum = font.Height + padding.Vertical;
		foreach (DataGridViewRow row in grid.Rows)
		{
			var text = row.Cells[0].Value as string ?? string.Empty;
			int height = TextRenderer.MeasureText(text, font, new Size(lineWidth, int.MaxValue), RowTextFlags).Height + padding.Vertical;
			row.Height = Math.Max(minimum, height);
		}
	}

	/// <summary>
	/// Fits the row heights of <paramref name="grid"/> once its column has finished changing width: the grid refuses a
	/// row height change while it resizes its filling column.
	/// </summary>
	private void FitRowHeightsLater(DataGridView grid, DataGridViewColumn column)
	{
		if (!grid.IsHandleCreated || !this.rowFitsPending.Add(grid))
			return;

		grid.BeginInvoke(() =>
		{
			this.rowFitsPending.Remove(grid);
			if (!grid.IsDisposed)
				this.FitRowHeights(grid, column);
		});
	}

	/// <summary>
	/// Draws a property line wrapped at its cell's width (<see cref="RowTextFlags"/>), in its colour, over the cell's
	/// background or selection.
	/// </summary>
	private static void GridCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
	{
		if (e.RowIndex < 0 || e.Graphics is null || e.CellStyle is null)
			return;

		bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
		e.PaintBackground(e.ClipBounds, selected);
		var font = e.CellStyle.Font ?? ((Control)sender!).Font;
		var padding = RowPadding(font);
		var bounds = new Rectangle(
			e.CellBounds.Left + padding.Left,
			e.CellBounds.Top + padding.Top,
			Math.Max(0, e.CellBounds.Width - padding.Horizontal),
			Math.Max(0, e.CellBounds.Height - padding.Vertical));
		TextRenderer.DrawText(e.Graphics, e.FormattedValue as string, font, bounds,
			selected ? e.CellStyle.SelectionForeColor : e.CellStyle.ForeColor, RowTextFlags);
		e.Handled = true;
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
