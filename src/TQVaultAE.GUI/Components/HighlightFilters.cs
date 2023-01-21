using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Helpers;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

public partial class HighlightFilters : UserControl
{
	private LinkLabel _link;
	public SessionContext UserContext { get; internal set; }
	public ITranslationService TranslationService { get; internal set; }
	public IFontService FontService { get; internal set; }

	public HighlightFilters()
	{
		InitializeComponent();

		DoubleBuffered = true;
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

		BackgroundImageLayout = ImageLayout.Stretch;
		BackgroundImage = Resources.caravan_bg;
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
		maxWidth += SystemInformation.VerticalScrollBarWidth;// Add this for the size of the checkbox in front of the text
		scalingCheckedListBoxOrigin.ColumnWidth = maxWidth;

		scalingCheckedListBoxOrigin.Items.AddRange(list);
	}

	private void InitType()
	{
		scalingCheckedListBoxTypes.Items.Clear();// Remove design mode items
		var list = new ItemType[] {
			new ItemType{
				Value = Item.ICLASS_HEAD,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_HEAD)).Trim(' ', ':')
			},
			new ItemType{
				Value = Item.ICLASS_FOREARM,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_FOREARM)).Trim(' ', ':')
			},
			new ItemType{
				Value = Item.ICLASS_UPPERBODY,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_UPPERBODY)).Trim(' ', ':')
			},
			new ItemType{
				Value = Item.ICLASS_LOWERBODY,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_LOWERBODY)).Trim(' ', ':')
			},
			new ItemType{
				Value = Item.ICLASS_AXE,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_AXE))
			},
			new ItemType{
				Value = Item.ICLASS_MACE,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_MACE))
			},
			new ItemType{
				Value = Item.ICLASS_SWORD,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SWORD))
			},
			new ItemType{
				Value = Item.ICLASS_SHIELD,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SHIELD))
			},
			new ItemType{
				Value = Item.ICLASS_RANGEDONEHAND,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_RANGEDONEHAND), true)
			},
			new ItemType{
				Value = Item.ICLASS_BOW,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_BOW))
			},
			new ItemType{
				Value = Item.ICLASS_SPEAR,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SPEAR))
			},
			new ItemType{
				Value = Item.ICLASS_STAFF,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_STAFF)).Trim(' ', ':')
			},
			new ItemType{
				Value = Item.ICLASS_AMULET,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_AMULET))
			},
			new ItemType{
				Value = Item.ICLASS_RING,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_RING))
			},
			new ItemType{
				Value = Item.ICLASS_ARTIFACT,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_ARTIFACT))
			},
			new ItemType{
				Value = Item.ICLASS_FORMULA,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_FORMULA))
			},
			new ItemType{
				Value = Item.ICLASS_RELIC,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_RELIC))
			},
			new ItemType{
				Value = Item.ICLASS_CHARM,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_CHARM))
			},
			new ItemType{
				Value = Item.ICLASS_SCROLL,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SCROLL))
			},
			new ItemType{
				Value = string.Join("|", Item.ICLASS_POTIONMANA, Item.ICLASS_POTIONHEALTH, Item.ICLASS_SCROLL_ETERNAL),
				DisplayName = TranslationService.TranslateXTag("tagTutorialTip14Title")
			},
			new ItemType{
				Value = Item.ICLASS_QUESTITEM,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_QUESTITEM))
			},
			new ItemType{
				Value = Item.ICLASS_EQUIPMENT,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_EQUIPMENT)).Trim(' ', '~')
			},
			new ItemType{
				Value = Item.ICLASS_DYE,
				DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_DYE))
			},
		};

		// Compute item space (ColumnWidth)
		int maxWidth = list.Select(i =>
			TextRenderer.MeasureText(
				i.DisplayName
				, scalingCheckedListBoxTypes.Font
				, scalingCheckedListBoxTypes.Size
				, TextFormatFlags.SingleLine
			).Width
		).Max();
		maxWidth += SystemInformation.VerticalScrollBarWidth;// Add this for the size of the checkbox in front of the text
		scalingCheckedListBoxTypes.ColumnWidth = maxWidth;

		scalingCheckedListBoxTypes.Items.AddRange(list);
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
		maxWidth += SystemInformation.VerticalScrollBarWidth;// Add this for the size of the checkbox in front of the text
		scalingCheckedListBoxRarity.ColumnWidth = maxWidth;

		scalingCheckedListBoxRarity.Items.AddRange(listRarity);
	}

	internal void ResetAll()
	{
		ResetNumeric();
		scalingCheckBoxMax.Checked =
		scalingCheckBoxMin.Checked =
		scalingCheckBoxSetItem.Checked =
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

	private void AdjustLocation()
	{
		// Adjust location
		var frm = FindForm();
		var linklocation = frm.PointToClient(_link.PointToScreen(Point.Empty));
		Location = new Point(linklocation.X, linklocation.Y - Height);
	}

	private void buttonReset_Click(object sender, EventArgs e)
	{
		var frm = FindForm();
		ResetAll();
		UserContext.HighlightFilter = null;
		_link.LinkVisited = false;
		Visible = false;
		UserContext.FindHighlight();
		frm.Refresh();
	}

	private void buttonSave_Click(object sender, EventArgs e)
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
			ClassItem = scalingCheckedListBoxTypes.CheckedItems.OfType<ItemType>().SelectMany(x => x.Value.Split('|')).ToList(),
			Rarity = scalingCheckedListBoxRarity.CheckedItems.OfType<ItemRarity>().Select(x => x.Value).ToList(),
			Origin = scalingCheckedListBoxOrigin.CheckedItems.OfType<ItemOrigin>().Select(x => x.Value).ToList(),
			HavingPrefix = scalingCheckBoxHavingPrefix.Checked,
			HavingSuffix = scalingCheckBoxHavingSuffix.Checked,
			HavingRelic = scalingCheckBoxHavingRelic.Checked,
			HavingCharm = scalingCheckBoxHavingCharm.Checked,
			IsSetItem = scalingCheckBoxSetItem.Checked,
		};

		if (filter.MaxRequierement || filter.MinRequierement || filter.HavingPrefix || filter.HavingSuffix || filter.HavingRelic || filter.HavingCharm || filter.IsSetItem
			|| filter.ClassItem.Any() || filter.Rarity.Any() || filter.Origin.Any())
		{
			UserContext.HighlightFilter = filter;
			_link.LinkVisited = true;
			Visible = false;
			UserContext.FindHighlight();
			frm.Refresh();
			return;
		}

		// No filters
		UserContext.HighlightFilter = null;
		_link.LinkVisited = false;
		Visible = false;
		UserContext.FindHighlight();
		frm.Refresh();
	}

	private void numericUpDownMinLvl_ValueChanged(object sender, EventArgs e)
	{
		scalingCheckBoxMin.Checked = true;
	}
	private void numericUpDownMaxLvl_ValueChanged(object sender, EventArgs e)
	{
		scalingCheckBoxMax.Checked = true;
	}
}
