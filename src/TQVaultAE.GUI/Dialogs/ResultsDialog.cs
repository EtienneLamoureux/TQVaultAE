//-----------------------------------------------------------------------
// <copyright file="ResultsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.GUI.Models;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Results dialog form class
/// </summary>
public partial class ResultsDialog : VaultForm
{
	/// <summary>
	/// List of all results
	/// </summary>
	private List<SearchResult> resultsList;

	/// <summary>
	/// User selected result from the list
	/// </summary>
	private SearchResult? selectedResult;

	/// <summary>
	/// Search string passed from user
	/// </summary>
	private string searchString = string.Empty;

	/// <summary>
	/// Initializes a new instance of the ResultsDialog class.
	/// </summary>
	public ResultsDialog(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		this.InitializeComponent();

		// The look of Advanced Search and Item Properties: the main window's marble, the list in a caravan frame, its
		// headers in gold on black (the system's light headers otherwise).
		CaravanLook.Apply(this, MarbleBackground.Marble.MainWindow);
		// Room for the marble around the frame, as in Item Properties; under the last row, black like its lists.
		this.Padding = new Padding(this.Padding.Left + 10, this.Padding.Top + 10, this.Padding.Right + 10, this.Padding.Bottom + 6);
		this.resultsDataGridView.BackgroundColor = Color.Black;
		this.resultsDataGridView.BorderStyle = BorderStyle.None;
		CaravanFrame.FrameAround(this.resultsDataGridView);
		var headers = this.resultsDataGridView.ColumnHeadersDefaultCellStyle;
		headers.ForeColor = Color.Gold;
		headers.SelectionBackColor = headers.BackColor;
		headers.SelectionForeColor = headers.ForeColor;
		this.resultsDataGridView.EnableHeadersVisualStyles = false;

		#region Apply custom font

		this.resultsDataGridView.ColumnHeadersDefaultCellStyle.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.columnItem.DefaultCellStyle.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.columnQuality.DefaultCellStyle.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.columnContainerName.DefaultCellStyle.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.columnContainerType.DefaultCellStyle.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.columnSack.DefaultCellStyle.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.columnLevel.DefaultCellStyle.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

		#endregion

		this.resultsList = [];

		this.columnItem.HeaderText = Resources.ResultsItem;
		this.columnContainerName.HeaderText = Resources.ResultsContainer;
		this.columnContainerType.HeaderText = Resources.ResultsContainerType;
		this.columnQuality.HeaderText = Resources.ResultsQuality;
		this.columnSack.HeaderText = Resources.ResultsSack;
		this.columnLevel.HeaderText = Resources.ResultsLevel;

		this.NormalizeBox = false;
		this.DrawCustomBorder = true;

		this.FormDesignRatio = 0.0F;

		this.OriginalFormSize = this.Size;
		this.OriginalFormScale = 1.0F;
		this.LastFormSize = this.Size;
	}

	/// <summary>
	/// Event Handler for the ResultChanged event.
	/// </summary>
	/// <typeparam name="ResultChangedEventArgs">ResultChangedEventArgs type</typeparam>
	/// <param name="sender">sender object</param>
	/// <param name="e">ResultChangedEventArgs data</param>
	public delegate void EventHandler<ResultChangedEventArgs>(object? sender, ResultChangedEventArgs e);

	/// <summary>
	/// Event for changing to a different highlighted result.
	/// </summary>
	public event EventHandler<ResultChangedEventArgs>? ResultChanged;

	/// <summary>
	/// Gets the list of results collection
	/// </summary>
	public List<SearchResult> ResultsList => this.resultsList;

	/// <summary>
	/// Sets the user search string
	/// </summary>
	public string SearchString
	{
		set => this.searchString = value;
	}

	/// <summary>
	/// Handler for showing the results dialog.
	/// Populates the data grid view.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ResultsDialogShown(object? sender, EventArgs e)
	{
		this.PopulateDataGridView();

		int totalWidth = 0;
		foreach (DataGridViewColumn column in this.resultsDataGridView.Columns)
			totalWidth += column.Width;

		totalWidth += this.resultsDataGridView.Margin.Horizontal;

		int totalHeight = 0;
		foreach (DataGridViewRow row in this.resultsDataGridView.Rows)
			totalHeight += row.Height;

		totalHeight += this.Padding.Vertical;

		if (totalHeight > this.Height)
			totalWidth += this.VerticalScrollBarWidth();

		// The monitor the dialog opened on, which is not always the primary one.
		var area = Screen.FromControl(this).WorkingArea;
		int scrollBarHeight = this.HorizontalScrollBarHeight();
		this.Width = Math.Min(area.Width, totalWidth + this.Padding.Horizontal);
		this.Height = Math.Max(this.Height, Math.Min(area.Height - scrollBarHeight, totalHeight - scrollBarHeight));
		this.Location = new Point(
			Math.Clamp(this.Location.X, area.Left, Math.Max(area.Left, area.Right - this.Width)),
			area.Top + (area.Height - this.Height) / 2);

		this.selectedResult = null;

	}

	/// <summary>
	/// Returns a string containing the tool tip text for the highlighted result.
	/// </summary>
	/// <param name="selectedResult">Currently selected Result</param>
	/// <returns>String containing the tool tip for the Result.</returns>
	private void GetToolTip(SearchResult selectedResult)
	{
		if (selectedResult is null || selectedResult.FriendlyNames is null)
			ItemTooltip.HideTooltip();
		else
			ItemTooltip.ShowTooltip(this.Services, selectedResult.FriendlyNames.Item, this);
	}

	/// <summary>
	/// Populates the data grid view from the results list.
	/// </summary>
	private void PopulateDataGridView()
	{
		if (this.resultsList is null || this.resultsList.Count < 1)
			return;

		// Update the dialog text.
		this.Text = string.IsNullOrWhiteSpace(this.searchString)
			? Resources.SearchDialogCaption
			: string.Format(Resources.ResultsText, this.resultsList.Count, this.searchString);

		// Size the columns and rows once, after the rows are added: with AllCells, every row added resized them all
		// (minutes for a few thousand results).
		var grid = this.resultsDataGridView;
		var columnModes = grid.Columns.Cast<DataGridViewColumn>().Select(column => column.AutoSizeMode).ToArray();
		var gridColumnsMode = grid.AutoSizeColumnsMode;
		var gridRowsMode = grid.AutoSizeRowsMode;
		grid.SuspendLayout();
		grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
		grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
		foreach (DataGridViewColumn column in grid.Columns)
			column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
		try
		{
			this.AddResultRows();
		}
		finally
		{
			grid.AutoSizeColumnsMode = gridColumnsMode;
			for (int c = 0; c < columnModes.Length; c++)
				grid.Columns[c].AutoSizeMode = columnModes[c];
			grid.AutoSizeRowsMode = gridRowsMode;
			grid.ResumeLayout();
		}
	}

	/// <summary>
	/// Adds a row per result, colored like the item.
	/// </summary>
	private void AddResultRows()
	{
		for (int i = 0; i < this.resultsList.Count; i++)
		{
			SearchResult result = this.resultsList[i];
			// Add the result to the DataGridView
			int currentRow = this.resultsDataGridView.Rows.Add(
				result.ItemName
				, result.ItemStyle
				, result.ContainerName
				, ItemSummaryText.ContainerTypeLabel(result.SackType, result.StashType)
				, ItemSummaryText.PlaceLabel(result.LoadoutName, SearchResultNavigation.Resolve(result).BagNumber)
				, result.RequiredLevel
			);

			// Change the text color of the item string and style to match the style color.
			if (currentRow > -1)
			{
				this.resultsDataGridView.Rows[currentRow].Cells[0].Style.ForeColor = result.TQColor.Color();
				this.resultsDataGridView.Rows[currentRow].Cells[1].Style.ForeColor = result.TQColor.Color();
				this.resultsDataGridView.Rows[currentRow].Tag = i;
			}
		}
	}

	/// <summary>
	/// Handler for user clicking on one of the result cells
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DataGridViewCellEventArgs data</param>
	private void ResultsDataGridViewCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
	{
		// Ignore double click on the header.
		if (e.RowIndex < 0)
			return;

		this.Close();
	}

	/// <summary>
	/// Handler for pressing a key
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyPressEventArgs data</param>
	private void ResultsDataGridViewKeyPress(object? sender, KeyPressEventArgs e)
	{
		// Escape key
		if (e.KeyChar == 27)
			this.Close();
	}

	/// <summary>
	/// Handler for the focus changing to a different row.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DataGridViewCellEventArgs data</param>
	private void ResultsDataGridViewRowEnter(object? sender, DataGridViewCellEventArgs e)
	{
		// Ignore click on the header.
		if (e.RowIndex < 0)
			return;

		var currentRow = this.resultsDataGridView.Rows[e.RowIndex];
		if (currentRow.Tag is null)
			return;

		this.selectedResult = this.resultsList[(int)currentRow.Tag];
	}

	/// <summary>
	/// Handler for single click on a cell.
	/// Used to bring to front the selected item.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ResultsDataGridViewCellClick(object? sender, DataGridViewCellEventArgs e)
	{
		// Ignore click on the header.
		if (e.RowIndex < 0)
			return;

		var currentRow = this.resultsDataGridView.Rows[e.RowIndex];
		if (currentRow.Tag is null)
			return;

		this.selectedResult = this.resultsList[(int)currentRow.Tag];

		if (this.selectedResult.FriendlyNames is not null && this.ResultChanged is not null)
			this.ResultChanged(this, new ResultChangedEventArgs(this.selectedResult));
	}

	/// <summary>
	/// Handler for leaving a DataGridView row.
	/// Used to clear the tool tip and the selected result.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DataGridViewCellEventArgs data</param>
	private void ResultsDataGridViewRowLeave(object? sender, DataGridViewCellEventArgs e)
	{
		this.selectedResult = null;
		ItemTooltip.HideTooltip();
	}

	/// <summary>
	/// Handler for the mouse leaving the DataGridView.  Used to clear the tooltip.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ResultsDataGridViewMouseLeave(object? sender, EventArgs e) => ItemTooltip.HideTooltip();

	/// <summary>
	/// Handler for entering a cell in the DataGridView.
	/// Used to highlight the row under the mouse and updating the tool tip text.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DataGridViewCellEventArgs data</param>
	private void ResultsDataGridViewCellMouseEnter(object? sender, DataGridViewCellEventArgs e)
	{
		// Ignore click on the header.
		if (e.RowIndex < 0)
			return;

		var currentRow = this.resultsDataGridView.Rows[e.RowIndex];
		if (currentRow.Tag is null)
			return;

		this.selectedResult = this.resultsList[(int)currentRow.Tag];
		this.resultsDataGridView.CurrentCell = currentRow.Cells[e.ColumnIndex];
		this.GetToolTip(this.selectedResult);
	}

	/// <summary>
	/// Handler for the mouse leaving a DataGridView cell.
	/// Used to clear the tool tip and the selected result.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DataGridViewCellEventArgs data</param>
	private void ResultsDataGridViewCellMouseLeave(object? sender, DataGridViewCellEventArgs e)
	{
		this.selectedResult = null;
		ItemTooltip.HideTooltip();
	}
}