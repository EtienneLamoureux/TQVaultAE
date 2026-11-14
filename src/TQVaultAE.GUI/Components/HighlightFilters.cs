﻿using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

public partial class HighlightFilters : UserControl, IZoomFit
{
	// Set by MainForm once the control is created by the designer.
	private LinkLabel _link = null!;
	public SessionContext UserContext { get; internal set; } = null!;
	public ITranslationService TranslationService { get; internal set; } = null!;
	public IHighlightService HighlightService { get; internal set; } = null!;

	public HighlightFilters()
	{
		InitializeComponent();

		DoubleBuffered = true;
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

		// The frame drawn once at the panel's size (again when it changes), not the picture stretched at each paint.
		CaravanFrame.ApplyTo(this, this.Padding);
	}

	/// <summary>
	/// The panel and its many transparent controls are painted in one pass (<c>WS_EX_COMPOSITED</c>), as the settings
	/// view: open during a resize of the window, they were drawn again control by control at each zoom step.
	/// </summary>
	protected override CreateParams CreateParams
	{
		get
		{
			const int WS_EX_COMPOSITED = 0x02000000;
			var parameters = base.CreateParams;
			parameters.ExStyle |= WS_EX_COMPOSITED;
			return parameters;
		}
	}

	/// <summary>
	/// Gives every control of the panel the game font at <paramref name="fontZoom"/>. Called with the window's other fonts
	/// before the zoom 1 layout is kept, then after each zoom and DPI change: set only at the first opening, the fonts
	/// were missing from the kept layout and the next zoom (a resize of the window) gave the designer's fonts back.
	/// </summary>
	internal void ApplyFonts(IFontService fontService, float fontZoom)
	{
		var font = fontService.GetFontLight(10F, fontZoom);
		this.ProcessAllControls(c => c.Font = font);
	}

	internal void InitializeFilters()
	{
		toolTip.SetToolTip(numericUpDownMinDex, Resources.HighlightFiltersZeroToIgnore);
		toolTip.SetToolTip(numericUpDownMinInt, Resources.HighlightFiltersZeroToIgnore);
		toolTip.SetToolTip(numericUpDownMinLvl, Resources.HighlightFiltersZeroToIgnore);
		toolTip.SetToolTip(numericUpDownMinStr, Resources.HighlightFiltersZeroToIgnore);
		toolTip.SetToolTip(numericUpDownMaxDex, Resources.HighlightFiltersZeroToIgnore);
		toolTip.SetToolTip(numericUpDownMaxInt, Resources.HighlightFiltersZeroToIgnore);
		toolTip.SetToolTip(numericUpDownMaxLvl, Resources.HighlightFiltersZeroToIgnore);
		toolTip.SetToolTip(numericUpDownMaxStr, Resources.HighlightFiltersZeroToIgnore);

		InitRarity();

		InitType();

		InitOrigin();

		// Translate
		scalingCheckBoxMin.Text = $"{Resources.GlobalMin} {Resources.GlobalRequirements} :";
		scalingCheckBoxMax.Text = $"{Resources.GlobalMax} {Resources.GlobalRequirements} :";
		scalingLabelMaxLvl.Text =
		scalingLabelMinLvl.Text = TranslationService.TranslateXTag("tagMenuImport05");
		scalingLabelMaxStr.Text =
		scalingLabelMinStr.Text = TranslationService.TranslateXTag("Strength");
		scalingLabelMaxDex.Text =
		scalingLabelMinDex.Text = TranslationService.TranslateXTag("Dexterity");
		scalingLabelMaxInt.Text =
		scalingLabelMinInt.Text = TranslationService.TranslateXTag("Intelligence");
		scalingCheckBoxHavingCharm.Text = Resources.SearchHavingCharm;
		scalingCheckBoxHavingRelic.Text = Resources.SearchHavingRelic;
		scalingCheckBoxHavingPrefix.Text = Resources.SearchHavingPrefix;
		scalingCheckBoxHavingSuffix.Text = Resources.SearchHavingSuffix;
		scalingCheckBoxSetItem.Text = Resources.SearchSetItem;
		scalingCheckBoxHavingSkill.Text = Resources.SearchHavingSkill;
		buttonReset.Text = TranslationService.TranslateXTag("tagSkillReset");
		buttonApply.Text = TranslationService.TranslateXTag("tagMenuButton07");

	}

	private void InitOrigin()
	{
		scalingCheckedListBoxOrigin.Items.Clear();// Remove design mode items
		var list = new ItemOrigin[] {
			new ItemOrigin{
				Value = GameDlc.TitanQuest,
				DisplayName = TranslationService.Translate(GameDlc.TitanQuest)
			},
			new ItemOrigin{
				Value = GameDlc.ImmortalThrone,
				DisplayName = TranslationService.Translate(GameDlc.ImmortalThrone)
			},
			new ItemOrigin{
				Value = GameDlc.Ragnarok,
				DisplayName = TranslationService.Translate(GameDlc.Ragnarok)
			},
			new ItemOrigin{
				Value = GameDlc.Atlantis,
				DisplayName = TranslationService.Translate(GameDlc.Atlantis)
			},
			new ItemOrigin{
				Value = GameDlc.EternalEmbers,
				DisplayName = TranslationService.Translate(GameDlc.EternalEmbers)
			},
		};

		scalingCheckedListBoxOrigin.Items.AddRange(list);
		FitColumns(scalingCheckedListBoxOrigin);
	}

	private void InitType()
	{
		scalingCheckedListBoxTypes.Items.Clear();// Remove design mode items
		var list = HighlightService.GetTypeOptions();

		scalingCheckedListBoxTypes.Items.AddRange([.. list]);
		FitColumns(scalingCheckedListBoxTypes);
	}

	private void InitRarity()
	{
		scalingCheckedListBoxRarity.Items.Clear();// Remove design mode items
		var listRarity = new ItemRarity[] {
			new ItemRarity(){
				Value = Rarity.Broken,
				DisplayName = TranslationService.Translate(Rarity.Broken)
			},
			new ItemRarity(){
				Value = Rarity.Mundane,
				DisplayName = TranslationService.Translate(Rarity.Mundane)
			},
			new ItemRarity(){
				Value = Rarity.Common,
				DisplayName = TranslationService.Translate(Rarity.Common)
			},
			new ItemRarity(){
				Value = Rarity.Rare,
				DisplayName = TranslationService.Translate(Rarity.Rare)
			},
			new ItemRarity(){
				Value = Rarity.Epic,
				DisplayName = TranslationService.Translate(Rarity.Epic)
			},
			new ItemRarity(){
				Value = Rarity.Legendary,
				DisplayName = TranslationService.Translate(Rarity.Legendary)
			},
		};

		scalingCheckedListBoxRarity.Items.AddRange(listRarity);
		FitColumns(scalingCheckedListBoxRarity);
	}

	/// <summary>
	/// Gives <paramref name="list"/> columns as wide as its longest entry in its current font, plus the check box: called
	/// when it is filled and again at each placement, since a zoom or a DPI change gives it another font (columns kept
	/// at the width of an earlier font overflowed the list, whose horizontal scroll bar then left room for one row).
	/// </summary>
	private static void FitColumns(ScalingCheckedListBox list)
	{
		if (list.Items.Count == 0)
			return;

		int width = list.Items.Cast<object>().Max(item => TextRenderer.MeasureText(
			list.GetItemText(item), list.Font, list.Size, TextFormatFlags.SingleLine).Width);
		width += list.VerticalScrollBarWidth();// Room for the check box in front of the text
		if (list.ColumnWidth != width)
			list.ColumnWidth = width;

		// High enough for the rows its columns need: the zoom scales its height and the font sets its item height, which
		// can leave room for one row only, the entries then in a single row with a horizontal scroll bar.
		int columns = Math.Max(1, list.ClientSize.Width / Math.Max(1, list.ColumnWidth));
		int rows = (list.Items.Count + columns - 1) / columns;
		int height = (rows * list.ItemHeight) + (list.Height - list.ClientSize.Height);
		if (list.Height < height)
			list.Height = height;
	}

	internal void ResetAll()
	{
		ResetNumeric();
		scalingCheckBoxMax.Checked =
		scalingCheckBoxMin.Checked =
		scalingCheckBoxSetItem.Checked =
		scalingCheckBoxHavingSkill.Checked =
		scalingCheckBoxHavingCharm.Checked =
		scalingCheckBoxHavingRelic.Checked =
		scalingCheckBoxHavingPrefix.Checked =
		scalingCheckBoxHavingSuffix.Checked = false;

		ClearSelected(scalingCheckedListBoxTypes);
		ClearSelected(scalingCheckedListBoxRarity);
		ClearSelected(scalingCheckedListBoxOrigin);
	}

	private static void ClearSelected(ScalingCheckedListBox listbox)
	{
		listbox.ClearSelected();
		for (int i = 0; i < listbox.Items.Count; i++)
			listbox.SetItemChecked(i, false);
	}

	internal void ResetNumeric()
	{
		numericUpDownMaxDex.Value =
		numericUpDownMaxInt.Value =
		numericUpDownMaxLvl.Value =
		numericUpDownMaxStr.Value =
		numericUpDownMinDex.Value =
		numericUpDownMinInt.Value =
		numericUpDownMinLvl.Value =
		numericUpDownMinStr.Value = 0;
	}

	/// <summary>
	/// Anchor link
	/// </summary>
	internal LinkLabel Link
	{
		set
		{
			_link = value;

			// The link moves without a zoom (a resize that keeps the zoom step) and after the panel is shown (the view laid
			// out again for a DPI changed while the panel was hidden): the open panel follows it after each layout of the
			// view holding both, which also covers the containers between the view and the link.
			if (this.Parent is { } view)
				view.Layout += LinkMoved;

			AdjustLocation();
		}
	}

	/// <summary>Places the open panel over its link again; its rows are laid out again only when its width changes.</summary>
	private void LinkMoved(object? sender, EventArgs e)
	{
		if (this.Visible)
			this.AdjustLocation(relayout: false);
	}

	/// <summary>
	/// Placed again once shown: the first show after a move to a monitor of another DPI scales the bounds the hidden panel
	/// got for the previous DPI, which would move it away from the link.
	/// </summary>
	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (this.Visible && this._link is not null)
			this.AdjustLocation();
	}

	/// <summary>Nothing to give back: the panel is only placed.</summary>
	void IZoomFit.UndoFit()
	{
	}

	/// <summary>An open panel is placed over its link again: the zoom gave it back the bounds it had when the layout was kept.</summary>
	void IZoomFit.Fit()
	{
		if (this.Visible && this._link is not null)
			this.AdjustLocation();
	}

	/// <summary>
	/// Places the panel over its link, no wider than the link's row; with <paramref name="relayout"/> (a zoom, a DPI
	/// change, an opening) or when that width changed, its rows and lists are laid out again first.
	/// </summary>
	private void AdjustLocation(bool relayout = true)
	{
		// Adjust location: over the link, in the view holding both.
		var parent = Parent;
		if (parent is null)
			return;

		// No wider than the link's row (the vault column) instead of running over the panels on the right.
		if (this._link.Parent is { } row && this.MaximumSize.Width != row.Width)
		{
			this.MaximumSize = new Size(row.Width, 0);
			this.tableLayoutPanelHighlight.MaximumSize = new Size(Math.Max(1, row.Width - this.Padding.Horizontal), 0);
			relayout = true;
		}

		if (!relayout)
		{
			this.PlaceOverLink(parent);
			return;
		}

		// Drawn once, at its place, after the rows are broken (its drawing frozen, its layout not).
		using (new LayoutBatch(this))
		{
			FitColumns(this.scalingCheckedListBoxTypes);
			FitColumns(this.scalingCheckedListBoxRarity);
			FitColumns(this.scalingCheckedListBoxOrigin);
			this.SpreadRequirementRows();
			this.BreakHavingRow();
			this.PlaceOverLink(parent);
		}
	}

	/// <summary>Above the link, but never above the top of the view: a panel taller than the room covers the link instead.</summary>
	private void PlaceOverLink(Control parent)
	{
		var linklocation = parent.PointToClient(_link.PointToScreen(Point.Empty));
		var location = new Point(linklocation.X, Math.Max(0, linklocation.Y - Height));
		if (Location != location)
			Location = location;
	}

	/// <summary>
	/// Keeps each requirement row (Level, Strength, Dexterity, Intelligence) on one line, from the left of its cell to
	/// its right edge: the space left is shared between the four label and box pairs; when the line is too long, the
	/// boxes give the room, down to four digits and their arrows.
	/// </summary>
	private void SpreadRequirementRows()
	{
		// Each label close to its box, the first one at the left of the row: the gaps between the pairs take the rest.
		var rows = new[] { this.flowLayoutPanelMin, this.flowLayoutPanelMax };
		foreach (var row in rows)
		{
			foreach (var pair in Pairs(row))
			{
				var close = pair[0].Margin with { Left = 0, Right = pair[0].LogicalToDeviceUnits(3) };
				if (pair[0].Margin != close)
					pair[0].Margin = close;
			}
		}

		this.PerformLayout();

		foreach (var row in rows)
		{
			var pairs = Pairs(row);
			var items = pairs.SelectMany(p => p).ToList();
			int width = row.ClientSize.Width - row.Padding.Horizontal;
			int Needed() => items.Sum(c => c.Width + c.Margin.Horizontal);

			// The boxes give the room the line lacks, shared between those that can still shrink.
			var boxes = items.OfType<NumericUpDown>().ToList();
			for (int excess = Needed() - width; excess > 0;)
			{
				var shrinkable = boxes.Where(b => b.Width > SmallestWidth(b)).ToList();
				if (shrinkable.Count == 0)
					break;

				int each = (excess + shrinkable.Count - 1) / shrinkable.Count;
				foreach (var box in shrinkable)
				{
					int before = box.Width;
					box.Width = Math.Max(SmallestWidth(box), box.Width - Math.Min(each, excess));
					excess -= before - box.Width;
				}
			}

			int gap = pairs.Count > 1 ? Math.Max(0, width - Needed()) / (pairs.Count - 1) : 0;
			for (int k = 1; k < pairs.Count; k++)
				SetLeftMargin(pairs[k][0], gap);
		}

		this.PerformLayout();

		static List<Control[]> Pairs(FlowLayoutPanel row) => row.Controls.Cast<Control>().Where(c => c.Visible).Chunk(2).ToList();

		// The width that still shows the box's largest value (4 digits for 9000, 3 for a level) and its arrows.
		static int SmallestWidth(NumericUpDown box)
		{
			var digits = new string('0', ((long)box.Maximum).ToString(System.Globalization.CultureInfo.InvariantCulture).Length);
			int arrows = box.Controls.Cast<Control>().FirstOrDefault(c => c is not TextBoxBase)?.Width ?? box.LogicalToDeviceUnits(16);
			return TextRenderer.MeasureText(digits, box.Font, Size.Empty, TextFormatFlags.NoPadding).Width + arrows + box.LogicalToDeviceUnits(6);
		}
	}

	/// <summary>
	/// Lays the Having check boxes out in even columns across the panel: six on one line, or three on two lines when they
	/// do not fit. They wrap first with the designer's spacing to find how many lines they need, then each moves to the
	/// start of its column, the columns lined up from line to line.
	/// </summary>
	private void BreakHavingRow()
	{
		var row = this.bufferedFlowLayoutPanelPrefixSuffix;
		var items = row.Controls.Cast<Control>().Where(c => c.Visible).ToList();
		if (items.Count == 0)
			return;

		foreach (var item in items)
		{
			row.SetFlowBreak(item, false);
			SetLeftMargin(item, item.Margin.Right);
		}

		this.PerformLayout();

		int lines = items.Select(c => c.Top).Distinct().Count();
		int perLine = (items.Count + lines - 1) / lines;
		int room = row.ClientSize.Width - row.Padding.Horizontal;
		int column = room / perLine;
		for (int first = 0; first < items.Count; first += perLine)
		{
			var line = items.Skip(first).Take(perLine).ToList();
			int x = 0;
			var lefts = new int[line.Count];
			for (int k = 0; k < line.Count; k++)
			{
				int start = Math.Max(x, k * column);
				lefts[k] = start - x;
				x = start + line[k].Width + line[k].Margin.Horizontal;
			}

			// A line the columns would push past the edge keeps the designer's spacing.
			if (x <= room)
			{
				for (int k = 0; k < line.Count; k++)
					SetLeftMargin(line[k], line[k].Margin.Right + lefts[k]);
			}

			if (first + perLine < items.Count)
				row.SetFlowBreak(line[^1], true);
		}

		this.PerformLayout();
	}

	/// <summary>Sets the left margin of <paramref name="control"/>, the space before it in its row.</summary>
	private static void SetLeftMargin(Control control, int left)
	{
		if (control.Margin.Left != left)
			control.Margin = control.Margin with { Left = left };
	}

	private void buttonReset_Click(object? sender, EventArgs e)
	{
		var frm = FindForm();
		ResetAll();
		_link.LinkVisited = HighlightService.ApplyFilter(null);
		Visible = false;
		frm?.Refresh();
	}

	private void buttonSave_Click(object? sender, EventArgs e)
	{
		var frm = FindForm();
		var filter = new HighlightFilterValues
		{
			MaxLvl = (int)numericUpDownMaxLvl.Value,
			MaxDex = (int)numericUpDownMaxDex.Value,
			MaxInt = (int)numericUpDownMaxInt.Value,
			MaxStr = (int)numericUpDownMaxStr.Value,
			MinLvl = (int)numericUpDownMinLvl.Value,
			MinDex = (int)numericUpDownMinDex.Value,
			MinInt = (int)numericUpDownMinInt.Value,
			MinStr = (int)numericUpDownMinStr.Value,
			MaxRequierement = scalingCheckBoxMax.Checked,
			MinRequierement = scalingCheckBoxMin.Checked,
			ClassItem = scalingCheckedListBoxTypes.CheckedItems.OfType<HighlightTypeOption>().SelectMany(x => x.ItemClasses).ToList(),
			Rarity = scalingCheckedListBoxRarity.CheckedItems.OfType<ItemRarity>().Select(x => x.Value).ToList(),
			Origin = scalingCheckedListBoxOrigin.CheckedItems.OfType<ItemOrigin>().Select(x => x.Value).ToList(),
			HavingPrefix = scalingCheckBoxHavingPrefix.Checked,
			HavingSuffix = scalingCheckBoxHavingSuffix.Checked,
			HavingRelic = scalingCheckBoxHavingRelic.Checked,
			HavingCharm = scalingCheckBoxHavingCharm.Checked,
			IsSetItem = scalingCheckBoxSetItem.Checked,
			HavingSkill = scalingCheckBoxHavingSkill.Checked,
		};

		// An empty filter clears the highlight filter.
		_link.LinkVisited = HighlightService.ApplyFilter(filter);
		Visible = false;
		frm?.Refresh();
	}

	private void numericUpDownMinLvl_ValueChanged(object? sender, EventArgs e)
	{
		scalingCheckBoxMin.Checked = true;
	}
	private void numericUpDownMaxLvl_ValueChanged(object? sender, EventArgs e)
	{
		scalingCheckBoxMax.Checked = true;
	}
}
