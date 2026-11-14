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

public partial class HighlightFilters : UserControl
{
	// Set by MainForm once the control is created by the designer.
	private LinkLabel _link = null!;
	public SessionContext UserContext { get; internal set; } = null!;
	public ITranslationService TranslationService { get; internal set; } = null!;
	public IFontService FontService { get; internal set; } = null!;
	public IHighlightService HighlightService { get; internal set; } = null!;

	public HighlightFilters()
	{
		InitializeComponent();

		DoubleBuffered = true;
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

		BackgroundImageLayout = ImageLayout.Stretch;
		BackgroundImage = Images.caravan_bg;
	}

	internal void InitializeFilters()
	{
		this.ProcessAllControls(c => c.Font = FontService.GetFontLight(10F));

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

		// Compute item space (ColumnWidth)
		int maxWidth = list.Select(i =>
			TextRenderer.MeasureText(
				i.DisplayName
				, scalingCheckedListBoxOrigin.Font
				, scalingCheckedListBoxOrigin.Size
				, TextFormatFlags.SingleLine
			).Width
		).Max();
		maxWidth += scalingCheckedListBoxOrigin.VerticalScrollBarWidth();// Add this for the size of the checkbox in front of the text
		scalingCheckedListBoxOrigin.ColumnWidth = maxWidth;

		scalingCheckedListBoxOrigin.Items.AddRange(list);
	}

	private void InitType()
	{
		scalingCheckedListBoxTypes.Items.Clear();// Remove design mode items
		var list = HighlightService.GetTypeOptions();

		// Compute item space (ColumnWidth)
		int maxWidth = list.Select(i =>
			TextRenderer.MeasureText(
				i.DisplayName
				, scalingCheckedListBoxTypes.Font
				, scalingCheckedListBoxTypes.Size
				, TextFormatFlags.SingleLine
			).Width
		).Max();
		maxWidth += scalingCheckedListBoxTypes.VerticalScrollBarWidth();// Add this for the size of the checkbox in front of the text
		scalingCheckedListBoxTypes.ColumnWidth = maxWidth;

		scalingCheckedListBoxTypes.Items.AddRange([.. list]);
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
		// Compute item space (ColumnWidth)
		var maxWidth = listRarity.Select(i =>
			TextRenderer.MeasureText(
				i.DisplayName
				, scalingCheckedListBoxRarity.Font
				, scalingCheckedListBoxRarity.Size
				, TextFormatFlags.SingleLine
			).Width
		).Max();
		maxWidth += scalingCheckedListBoxRarity.VerticalScrollBarWidth();// Add this for the size of the checkbox in front of the text
		scalingCheckedListBoxRarity.ColumnWidth = maxWidth;

		scalingCheckedListBoxRarity.Items.AddRange(listRarity);
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

	private void AdjustLocation()
	{
		// Adjust location: over the link, in the view holding both.
		var parent = Parent;
		if (parent is null)
			return;

		var linklocation = parent.PointToClient(_link.PointToScreen(Point.Empty));
		Location = new Point(linklocation.X, linklocation.Y - Height);
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
