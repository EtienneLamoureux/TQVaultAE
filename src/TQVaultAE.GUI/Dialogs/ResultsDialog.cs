//-----------------------------------------------------------------------
// <copyright file="ResultsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using TQVaultAE.Application.Layout;
using TQVaultAE.Application.Search;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.GUI.Models;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components.Scaling;

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
	/// The main content: the targets of <i>Move To</i>, and its panels shown again after a move. Set by the main content
	/// before the window is shown.
	/// </summary>
	internal Views.MainFormUserControl MoveHost { get; set; } = null!;

	/// <summary>
	/// The moves of this window, the last one last: <i>Undo</i> puts the items of the last one back. Lost when the window
	/// closes.
	/// </summary>
	private readonly List<ResultsMoveBatch> undoHistory = [];

	/// <summary>The row under the mouse, drawn lighter; -1 for none.</summary>
	private int hoveredRow = -1;

	/// <summary>The row under the mouse: a dark gold, the selected rows being gold.</summary>
	private static readonly Color HoverBackColor = Color.FromArgb(92, 72, 24);

	private ScalingButton selectAllButton = null!;
	private ScalingButton moveButton = null!;
	private ScalingButton undoButton = null!;
	private FlowLayoutPanel moveButtons = null!;
	private TableLayoutPanel movePanel = null!;
	private Label reportLabel = null!;
	private ContextMenuStrip resultsMenu = null!;
	private ContextMenuStrip moveMenu = null!;

	/// <summary>
	/// Initializes a new instance of the ResultsDialog class.
	/// </summary>
	public ResultsDialog(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		// The commands under the list are made in code: inside the designer's layout suspension, so that Windows Forms
		// scales them to the monitor's DPI with the rest of the dialog when it resumes (it scales only the controls
		// present then: the buttons kept their 96 dpi size, their text cut at a higher DPI).
		this.SuspendLayout();
		CaravanDialog.SetUpWindow(this, nameof(ResultsDialog));
		this.InitializeComponent();
		// A modeless window: it keeps its taskbar button.
		this.ShowInTaskbar = true;

		// Several rows (Ctrl or Shift click, Ctrl+A): Move To and Undo act on the selection.
		this.resultsDataGridView.MultiSelect = true;
		this.resultsDataGridView.CellMouseDown += this.ResultsDataGridViewCellMouseDown;
		this.BuildMoveControls();
		// Room for the marble around the frame, as in Item Properties; under the last row, black like its lists.
		this.Padding = new Padding(this.Padding.Left + 10, this.Padding.Top + 10, this.Padding.Right + 10, this.Padding.Bottom + 6);
		this.ResumeLayout(false);

		// The look of Advanced Search and Item Properties: the main window's marble, the list in a caravan frame, its
		// headers in gold on black (the system's light headers otherwise).
		CaravanLook.Apply(this, MarbleBackground.Marble.MainWindow, this.movePanel, this.moveButtons);
		this.resultsDataGridView.BackgroundColor = Color.Black;
		this.resultsDataGridView.BorderStyle = BorderStyle.None;
		CaravanFrame.FrameAround(this.resultsDataGridView);
		var headers = this.resultsDataGridView.ColumnHeadersDefaultCellStyle;
		headers.ForeColor = Color.Gold;
		headers.SelectionBackColor = headers.BackColor;
		headers.SelectionForeColor = headers.ForeColor;
		this.resultsDataGridView.EnableHeadersVisualStyles = false;

		this.Font = FontService.GetFontLight(CaravanLook.TextSize);
		this.ApplyListFonts();
		// The header row as high as its text, on one line: a fixed height did not follow the font, which grows with the DPI
		// and the zoom.
		headers.WrapMode = DataGridViewTriState.False;
		this.resultsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

		this.resultsList = [];

		this.columnItem.HeaderText = Resources.ResultsItem;
		this.columnContainerName.HeaderText = Resources.ResultsContainer;
		this.columnContainerType.HeaderText = Resources.ResultsContainerType;
		this.columnQuality.HeaderText = Resources.ResultsQuality;
		this.columnSack.HeaderText = Resources.ResultsSack;
		this.columnLevel.HeaderText = Resources.ResultsLevel;

		this.DrawCustomBorder = true;

		this.LastFormSize = this.Size;
	}

	/// <summary>Designed at zoom 1: scaled to the user's zoom when made (<see cref="VaultForm.ApplyUserZoom"/>).</summary>
	protected internal override bool FollowsUserZoom => true;

	/// <summary>The list's fonts are its own (not the dialog's, which the zoom grows): made again at the zoom.</summary>
	protected override void OnUserZoomApplied()
	{
		base.OnUserZoomApplied();
		this.ApplyListFonts();
	}

	/// <summary>
	/// The fonts of the list and of the report under it, at the dialog's zoom (<see cref="VaultForm.ScaledZoom"/>): the
	/// marble dialogs' text size, the item name in italics, the other columns regular.
	/// </summary>
	private void ApplyListFonts()
	{
		var regular = FontService.GetFontLight(CaravanLook.TextSize, this.ScaledZoom);
		this.resultsDataGridView.ColumnHeadersDefaultCellStyle.Font = regular;
		this.columnItem.DefaultCellStyle.Font = FontService.GetFontLight(CaravanLook.TextSize, FontStyle.Italic, this.ScaledZoom);
		foreach (var column in new DataGridViewColumn[] { this.columnQuality, this.columnContainerName, this.columnContainerType, this.columnSack, this.columnLevel })
			column.DefaultCellStyle.Font = regular;
		this.reportLabel.Font = regular;
	}

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
	/// The window's title in place of the one made from <see cref="SearchString"/>, e.g. the item menu's search
	/// ("All copies of Ring (2 found)").
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// The result searched from (the item menu's <i>Search</i>): its row is marked as the source.
	/// </summary>
	public SearchResult? SourceResult { get; set; }

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

		int totalHeight = this.resultsDataGridView.ColumnHeadersHeight;
		foreach (DataGridViewRow row in this.resultsDataGridView.Rows)
			totalHeight += row.Height;

		totalHeight += this.Padding.Vertical + this.movePanel.PreferredSize.Height;
		totalWidth = Math.Max(totalWidth, this.moveButtons.PreferredSize.Width);

		if (totalHeight > this.Height)
			totalWidth += this.VerticalScrollBarWidth();

		// The monitor the dialog opened on, which is not always the primary one.
		var area = Screen.FromControl(this).WorkingArea;
		int scrollBarHeight = this.HorizontalScrollBarHeight();
		this.Width = Math.Min(area.Width, totalWidth + this.Padding.Horizontal);
		this.Height = Math.Max(this.Height, Math.Min(area.Height - scrollBarHeight, totalHeight - scrollBarHeight));
		this.Location = WindowLayout.KeepOnScreen(new Point(this.Location.X, area.Top + ((area.Height - this.Height) / 2)), this.Size, [area]);

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
		this.Text = !string.IsNullOrWhiteSpace(this.Title)
			? this.Title
			: string.IsNullOrWhiteSpace(this.searchString)
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
			bool isSource = ReferenceEquals(result, this.SourceResult);
			var (container, containerType, bag) = LocationTexts(result);
			// Add the result to the DataGridView
			int currentRow = this.resultsDataGridView.Rows.Add(
				isSource ? $"{result.ItemName} {Resources.ResultsSourceMark}" : result.ItemName
				, result.ItemStyle
				, container
				, containerType
				, bag
				, result.RequiredLevel
			);

			// Change the text color of the item string and style to match the style color.
			if (currentRow > -1)
			{
				this.resultsDataGridView.Rows[currentRow].Cells[0].Style.ForeColor = result.TQColor.Color();
				this.resultsDataGridView.Rows[currentRow].Cells[1].Style.ForeColor = result.TQColor.Color();
				this.resultsDataGridView.Rows[currentRow].Tag = i;

				// The item searched from, in bold.
				if (isSource)
					this.resultsDataGridView.Rows[currentRow].Cells[0].Style.Font = FontService.GetFontLight(CaravanLook.TextSize, FontStyle.Bold | FontStyle.Italic, this.ScaledZoom);
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
	/// The result shown on row <paramref name="rowIndex"/>; none for the header (a negative index) or a row without one.
	/// </summary>
	private bool TryGetResult(int rowIndex, [NotNullWhen(true)] out SearchResult? result)
	{
		result = rowIndex >= 0 && this.resultsDataGridView.Rows[rowIndex].Tag is int index ? this.resultsList[index] : null;
		return result is not null;
	}

	/// <summary>
	/// Handler for the focus changing to a different row.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DataGridViewCellEventArgs data</param>
	private void ResultsDataGridViewRowEnter(object? sender, DataGridViewCellEventArgs e)
	{
		if (!this.TryGetResult(e.RowIndex, out var result))
			return;

		this.selectedResult = result;
	}

	/// <summary>
	/// Handler for single click on a cell.
	/// Used to bring to front the selected item.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ResultsDataGridViewCellClick(object? sender, DataGridViewCellEventArgs e)
	{
		if (!this.TryGetResult(e.RowIndex, out var result))
			return;

		this.selectedResult = result;

		// A Ctrl or Shift click picks rows to move: the main window stays where it is.
		if (Control.ModifierKeys != Keys.None)
			return;

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
	private void ResultsDataGridViewMouseLeave(object? sender, EventArgs e)
	{
		this.HoverRow(-1);
		ItemTooltip.HideTooltip();
	}

	/// <summary>
	/// Handler for entering a cell in the DataGridView.
	/// Used to highlight the row under the mouse and updating the tool tip text.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DataGridViewCellEventArgs data</param>
	private void ResultsDataGridViewCellMouseEnter(object? sender, DataGridViewCellEventArgs e)
	{
		if (!this.TryGetResult(e.RowIndex, out var result))
			return;

		this.selectedResult = result;
		// The row under the mouse drawn lighter; the current cell stays, or the selection would follow the mouse.
		this.HoverRow(e.RowIndex);
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


	#region Move To and Undo

	/// <summary>
	/// The commands under the list: <i>Select All</i>, <i>Move To</i> (its menu opens under the button) and <i>Undo</i>,
	/// with the report of the last move below them; the right-click menu of the list has the same commands.
	/// </summary>
	private void BuildMoveControls()
	{
		// At zoom 1, like the designer's controls: the dialog is scaled to the user's zoom once made (FollowsUserZoom).
		const float zoom = 1.0F;
		this.selectAllButton = CaravanDialog.Button(this.FontService, zoom, "selectAllButton", Resources.ResultsMenuSelectAll);
		this.selectAllButton.Click += (_, _) => this.SelectAllRows();
		this.moveButton = CaravanDialog.Button(this.FontService, zoom, "moveButton", Resources.ResultsMenuMoveTo);
		this.moveButton.Click += (_, _) => this.ShowMoveMenu();
		this.undoButton = CaravanDialog.Button(this.FontService, zoom, "undoButton", Resources.ResultsMenuUndo);
		this.undoButton.Click += (_, _) => this.UndoLastMove();

		this.moveButtons = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Margin = Padding.Empty,
			Padding = Padding.Empty,
			Name = "moveButtons",
		};
		this.moveButtons.Controls.AddRange([this.selectAllButton, this.moveButton, this.undoButton]);

		this.reportLabel = new Label()
		{
			AutoSize = true,
			BackColor = Color.Transparent,
			ForeColor = Color.Gold,
			Margin = new Padding(3, (int)Math.Round(4 * zoom), 3, 0),
			Name = "moveReport",
		};

		this.movePanel = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			RowCount = 2,
			Dock = DockStyle.Bottom,
			Margin = Padding.Empty,
			// Under the list's frame, drawn outside the list.
			Padding = new Padding(0, CaravanFrame.Border + (int)Math.Round(6 * zoom), 0, 0),
			Name = "movePanel",
		};
		this.movePanel.Controls.Add(this.moveButtons, 0, 0);
		this.movePanel.Controls.Add(this.reportLabel, 0, 1);
		// The report wraps at the window's width.
		this.movePanel.Resize += (_, _) => this.reportLabel.MaximumSize = new Size(Math.Max(1, this.movePanel.ClientSize.Width - 6), 0);
		this.Controls.Add(this.movePanel);
		// The list fills what the commands leave.
		this.Controls.SetChildIndex(this.resultsDataGridView, 0);

		this.resultsMenu = new ContextMenuStrip() { Name = "resultsMenu", ShowImageMargin = false };
		MenuStyle.Apply(this.resultsMenu, this.FontService, this.UIService.FontZoom);
		this.resultsMenu.FillOnOpening(() => this.FillMenu(this.resultsMenu, full: true));
		this.resultsDataGridView.ContextMenuStrip = this.resultsMenu;

		this.moveMenu = new ContextMenuStrip() { Name = "moveMenu", ShowImageMargin = false };
		MenuStyle.Apply(this.moveMenu, this.FontService, this.UIService.FontZoom);
		this.moveMenu.FillOnOpening(() => this.FillMenu(this.moveMenu, full: false));

		// Made in code, outside the designer's components: disposed with the dialog, their entries with them.
		this.Disposed += (_, _) =>
		{
			this.resultsMenu.Dispose();
			this.moveMenu.Dispose();
		};

		this.resultsDataGridView.SelectionChanged += (_, _) => this.UpdateMoveButtons();
		this.UpdateMoveButtons();
	}

	/// <summary>The results of the selected rows, in the order of the list.</summary>
	internal List<SearchResult> SelectedResults()
		=> [.. this.resultsDataGridView.SelectedRows.Cast<DataGridViewRow>()
			.OrderBy(row => row.Index)
			.Select(row => this.TryGetResult(row.Index, out var result) ? result : null)
			.OfType<SearchResult>()];

	/// <summary>The commands follow the selection and the moves kept.</summary>
	private void UpdateMoveButtons()
	{
		this.selectAllButton.Enabled = this.resultsDataGridView.Rows.Count > 0;
		this.moveButton.Enabled = this.resultsDataGridView.SelectedRows.Count > 0;
		this.undoButton.Enabled = this.undoHistory.Count > 0;
	}

	/// <summary>Selects every row, as Ctrl+A does.</summary>
	internal void SelectAllRows()
	{
		this.resultsDataGridView.SelectAll();
		this.UpdateMoveButtons();
	}

	/// <summary>A right click on a row not selected selects it alone, as in a file list: the menu acts on it.</summary>
	private void ResultsDataGridViewCellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right || e.RowIndex < 0)
			return;

		var row = this.resultsDataGridView.Rows[e.RowIndex];
		if (row.Selected)
			return;

		this.resultsDataGridView.ClearSelection();
		row.Selected = true;
	}

	/// <summary>Draws the row under the mouse lighter, and the one it left as before.</summary>
	private void HoverRow(int rowIndex)
	{
		var rows = this.resultsDataGridView.Rows;
		if (this.hoveredRow >= 0 && this.hoveredRow < rows.Count)
			rows[this.hoveredRow].DefaultCellStyle.BackColor = Color.Empty;

		this.hoveredRow = rowIndex;
		if (rowIndex >= 0 && rowIndex < rows.Count)
			rows[rowIndex].DefaultCellStyle.BackColor = HoverBackColor;
	}

	/// <summary>The <i>Move To</i> menu under its button.</summary>
	private void ShowMoveMenu()
		=> this.moveMenu.Show(this.moveButton, new Point(0, this.moveButton.Height));

	/// <summary>
	/// Fills a menu when it opens: the right-click menu with <i>Select All</i>, <i>Move To</i> and <i>Undo</i>, the
	/// button's menu with the targets of <i>Move To</i>.
	/// </summary>
	internal void FillMenu(ContextMenuStrip menu, bool full)
	{
		menu.Items.Clear();
		var targets = this.MoveTargetEntries(menu);
		if (!full)
		{
			menu.Items.AddRange(targets);
			return;
		}

		menu.Items.Add(Entry(menu, "selectAll", Resources.ResultsMenuSelectAll, this.SelectAllRows, this.resultsDataGridView.Rows.Count > 0));
		var move = new ToolStripMenuItem(Resources.ResultsMenuMoveTo)
		{
			Name = "moveTo",
			DisplayStyle = ToolStripItemDisplayStyle.Text,
			Enabled = this.resultsDataGridView.SelectedRows.Count > 0,
		}.StyledLike(menu);
		move.DropDownItems.AddRange(targets);
		WithoutImageMargin(move);
		menu.Items.Add(move);
		menu.Items.Add(Entry(menu, "undo", Resources.ResultsMenuUndo, this.UndoLastMove, this.undoHistory.Count > 0));
	}

	/// <summary>A sub menu drawn like the menu it opens from: text only, no room for pictures.</summary>
	private static void WithoutImageMargin(ToolStripMenuItem item)
	{
		if (item.DropDown is ToolStripDropDownMenu dropDown)
			dropDown.ShowImageMargin = false;
	}

	private static ToolStripMenuItem Entry(ToolStrip owner, string name, string text, Action click, bool enabled = true)
		=> new ToolStripMenuItem(text, null, (_, _) => click()) { Name = name, Enabled = enabled }.StyledLike(owner);

	/// <summary>
	/// The targets of <i>Move To</i>: the stashes shown with the selected character (the relic vault when it takes every
	/// selected item), then the vaults, each with its bags (the vault is read when its entry opens).
	/// </summary>
	private ToolStripItem[] MoveTargetEntries(ToolStrip owner)
	{
		var host = this.MoveHost;
		var items = this.SelectedResults().Select(r => r.Item).ToList();
		bool any = items.Count > 0;

		ToolStripItem Stash(string name, string text, StashType type, bool allowed)
		{
			var target = host.ResultsMoveStashTarget(type);
			return Entry(owner, name, text, () => this.MoveSelection(() => host.ResultsMoveStashTarget(type), text)
				, any && allowed && target is not null && !target.Sack.IsReadOnly);
		}

		var vaults = new ToolStripMenuItem(Resources.ResultsMenuVaults)
		{
			Name = "moveVaults",
			DisplayStyle = ToolStripItemDisplayStyle.Text,
			Enabled = any,
		}.StyledLike(owner);
		foreach (var name in host.ResultsMoveVaults())
		{
			var vault = new ToolStripMenuItem(name) { Name = name, DisplayStyle = ToolStripItemDisplayStyle.Text }.StyledLike(owner);
			// Filled when it opens: reading every vault to list its bags would be slow.
			vault.DropDownItems.Add(new ToolStripMenuItem("…").StyledLike(owner));
			vault.DropDownOpening += (_, _) => this.FillVaultBags(vault, name, owner);
			WithoutImageMargin(vault);
			vaults.DropDownItems.Add(vault);
		}

		WithoutImageMargin(vaults);

		return
		[
			Stash("moveTransfer", Resources.GlobalTransferStash, StashType.TransferStash, true),
			Stash("movePlayer", Resources.ResultsMenuPlayerStash, StashType.PlayerStash, true),
			Stash("moveRelic", Resources.GlobalRelicVaultStash, StashType.RelicVaultStash, any && this.Services.ResultsMove.Accepts(items, StashType.RelicVaultStash)),
			new ToolStripSeparator(),
			vaults,
		];
	}

	/// <summary>The bags of a vault, read now when no panel shows it.</summary>
	private void FillVaultBags(ToolStripMenuItem entry, string name, ToolStrip owner)
	{
		entry.DropDownItems.Clear();
		var host = this.MoveHost;
		if (host.ResultsMoveVault(name) is not { } vault)
			return;

		for (int bag = 0; bag < vault.NumberOfSacks; bag++)
		{
			int number = bag;
			string text = string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.GlobalMenuBag, number + 1);
			entry.DropDownItems.Add(Entry(owner, $"bag{number + 1}", text
				, () => this.MoveSelection(() => host.ResultsMoveVaultTarget(vault, number), ResultsMoveText.VaultBag(vault.PlayerName, number))));
		}
	}

	/// <summary>
	/// Moves the items of the selected rows to a target, keeps the move for <i>Undo</i>, and shows the new places and the
	/// report.
	/// </summary>
	private void MoveSelection(Func<ResultsMoveTarget?> target, string targetName)
	{
		var host = this.MoveHost;
		var items = this.SelectedResults().Select(r => r.Item).ToList();
		if (items.Count == 0 || !host.ReadyForResultsMove() || target() is not { } bag)
			return;

		var outcome = this.Services.ResultsMove.Move(items, bag);
		if (outcome.Batch is not null)
			this.undoHistory.Add(outcome.Batch);

		this.AfterMove(outcome, targetName);
	}

	/// <summary>Puts the items of the last move back where they were.</summary>
	internal void UndoLastMove()
	{
		var host = this.MoveHost;
		if (this.undoHistory.Count == 0 || !host.ReadyForResultsMove())
			return;

		var batch = this.undoHistory[^1];
		this.undoHistory.RemoveAt(this.undoHistory.Count - 1);
		this.AfterMove(this.Services.ResultsMove.Undo(batch), null);
	}

	private void AfterMove(ResultsMoveOutcome outcome, string? targetName)
	{
		this.MoveHost.ResultsMoved(outcome);
		this.RefreshLocations();
		this.reportLabel.Text = ResultsMoveText.Report(outcome, targetName, this.NameOf);
		this.UpdateMoveButtons();
	}

	/// <summary>The name the list shows for an item.</summary>
	private string NameOf(Item item)
		=> this.resultsList.FirstOrDefault(r => ReferenceEquals(r.Item, item))?.ItemName is { Length: > 0 } name
			? name
			: this.Services.ItemText.GetFriendlyNames(item).FullNameClean;

	/// <summary>The rows show where their items are now, the selection kept.</summary>
	private void RefreshLocations()
	{
		foreach (DataGridViewRow row in this.resultsDataGridView.Rows)
		{
			if (!this.TryGetResult(row.Index, out var result))
				continue;

			var (container, containerType, bag) = LocationTexts(result);
			row.Cells[this.columnContainerName.Index].Value = container;
			row.Cells[this.columnContainerType.Index].Value = containerType;
			row.Cells[this.columnSack.Index].Value = bag;
		}
	}

	/// <summary>The container, container type and bag columns of a result, read from where its item is now.</summary>
	private static (string Container, string ContainerType, string Bag) LocationTexts(SearchResult result)
		=> (SharedStashText.Localize(result.ContainerName, result.StashType)
			, ItemSummaryText.ContainerTypeLabel(result.SackType, result.StashType)
			, ItemSummaryText.PlaceLabel(result.LoadoutName, SearchResultNavigation.Resolve(result).BagNumber));

	/// <summary>The report of the last move or undo, for the automation.</summary>
	internal string MoveReport => this.reportLabel.Text;

	/// <summary>The moves an undo can still put back, for the automation.</summary>
	internal int UndoCount => this.undoHistory.Count;

	/// <summary>The right-click menu of the list, for the automation.</summary>
	internal ContextMenuStrip ResultsMenu => this.resultsMenu;

	/// <summary>The menu of the <i>Move To</i> button, for the automation.</summary>
	internal ContextMenuStrip MoveMenu => this.moveMenu;

	/// <summary>The list, for the automation.</summary>
	internal DataGridView ResultsGrid => this.resultsDataGridView;

	#endregion
}