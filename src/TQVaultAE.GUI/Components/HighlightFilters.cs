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
			AdjustLocation();
		}
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

	private void AdjustLocation()
	{
		// Adjust location: over the link, in the view holding both.
		var parent = Parent;
		if (parent is null)
			return;

		// No wider than the link's row (the vault column): the Min and Max rows wrap instead of running over the panels
		// on the right.
		if (this._link.Parent is { } row && this.MaximumSize.Width != row.Width)
		{
			this.MaximumSize = new Size(row.Width, 0);
			this.tableLayoutPanelHighlight.MaximumSize = new Size(Math.Max(1, row.Width - this.Padding.Horizontal), 0);
		}

		// Drawn once, at its place, after the rows are broken (its drawing frozen, its layout not).
		using (new LayoutBatch(this))
		{
			FitColumns(this.scalingCheckedListBoxTypes);
			FitColumns(this.scalingCheckedListBoxRarity);
			FitColumns(this.scalingCheckedListBoxOrigin);
			this.BreakRowsEvenly();
			var linklocation = parent.PointToClient(_link.PointToScreen(Point.Empty));
			Location = new Point(linklocation.X, linklocation.Y - Height);
		}
	}

	/// <summary>
	/// Lays the rows of the panel's top out in even columns across its width: a requirement's label stays with its box
	/// (four columns, or Level and Strength then Dexterity and Intelligence when they do not fit) and the Having check
	/// boxes take six columns, or three on two lines. The rows wrap first with the designer's spacing to find how many
	/// lines they need, then each group moves to the start of its column, the columns lined up from line to line.
	/// </summary>
	private void BreakRowsEvenly()
	{
		var rows = new (FlowLayoutPanel Row, int Group)[]
		{
			(this.flowLayoutPanelMin, 2),
			(this.flowLayoutPanelMax, 2),
			(this.bufferedFlowLayoutPanelPrefixSuffix, 1),
		};

		var groupsOf = rows.Select(r => r.Row.Controls.Cast<Control>().Where(c => c.Visible).Chunk(r.Group).ToList()).ToList();
		for (int r = 0; r < rows.Length; r++)
		{
			foreach (Control item in rows[r].Row.Controls)
				rows[r].Row.SetFlowBreak(item, false);
			foreach (var group in groupsOf[r])
				SetLeftMargin(group[0], group[0].Margin.Right);
		}

		this.PerformLayout();

		for (int r = 0; r < rows.Length; r++)
		{
			var row = rows[r].Row;
			var groups = groupsOf[r];
			if (groups.Count == 0)
				continue;

			int lines = groups.SelectMany(g => g).Select(c => c.Top).Distinct().Count();
			int perLine = (groups.Count + lines - 1) / lines;
			int column = (row.ClientSize.Width - row.Padding.Horizontal) / perLine;
			for (int first = 0; first < groups.Count; first += perLine)
			{
				var line = groups.Skip(first).Take(perLine).ToList();
				int x = 0;
				var lefts = new int[line.Count];
				for (int k = 0; k < line.Count; k++)
				{
					int start = Math.Max(x, k * column);
					lefts[k] = start - x;
					x = start + line[k].Sum(c => c.Width + c.Margin.Horizontal);
				}

				// A line the columns would push past the edge keeps the designer's spacing.
				if (x <= row.ClientSize.Width - row.Padding.Horizontal)
				{
					for (int k = 0; k < line.Count; k++)
						SetLeftMargin(line[k][0], line[k][0].Margin.Right + lefts[k]);
				}

				if (first + perLine < groups.Count)
					row.SetFlowBreak(line[^1][^1], true);
			}
		}

		this.PerformLayout();

		static void SetLeftMargin(Control control, int left)
		{
			if (control.Margin.Left != left)
				control.Margin = control.Margin with { Left = left };
		}
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
