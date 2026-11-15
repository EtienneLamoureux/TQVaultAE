//-----------------------------------------------------------------------
// <copyright file="SackPanel.ContextMenu.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel
{

	private void AddAutoMoveMenuItems(Item? focusedItem)
	{
		var destinations = ItemActionRules.MoveDestinations(new MoveContext(
			this.AutoMoveLocation
			, this.SackType
			, this.MaxSacks
			, this.PlayerCollection?.NumberOfSacks ?? 0
			, this.DragInfo.AllAutoMoveLocations
			, this.SecondaryVaultShown
			, this.userContext.CurrentPlayer?.IsImmortalThrone
			, focusedItem
			, this.CurrentSack));

		var moveChoices = new List<ToolStripItem>();
		foreach (var destination in destinations)
		{
			string label = destination switch
			{
				{ IsMainPlayerSack: true } => Resources.SackPanelMenuPlayer,
				{ BagIndex: not null } => string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, destination.BagNumber),
				_ => GetStringFromAutoMove(destination.Location),
			};

			var moveChoice = new ToolStripMenuItem(label, null, MoveItemClicked, label)
			{
				Tag = destination.BagIndex is int bagIndex ? new MenuItemTagBagIndex(bagIndex) : (object)destination.Location,
			}.StyledLike(this.CustomContextMenu);
			moveChoices.Add(moveChoice);
		}

		var moveSubMenu = new ToolStripMenuItem(Resources.SackPanelMenuMoveTo, null, moveChoices.ToArray())
		{
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		}.StyledLike(this.CustomContextMenu);

		this.CustomContextMenu.Items.Add(moveSubMenu);
	}
	
	/// <summary>
	/// Adds the Search sub menu: the item's copies, or the items sharing its base, prefix or suffix, among every loaded
	/// item, shown in the results window. A prefix or suffix search is disabled for an item without one.
	/// </summary>
	private void AddFindCopiesMenuItems(Item focusedItem)
	{
		var choices = new List<ToolStripItem>();
		foreach (var kind in ItemCopySearch.Kinds)
		{
			var choice = new ToolStripMenuItem(ItemCopySearchText.MenuLabel(kind), null, (_, _) => this.FindCopiesClicked(focusedItem, kind), kind.ToString())
			{
				Enabled = ItemCopySearch.IsAvailable(focusedItem, kind),
			}.StyledLike(this.CustomContextMenu);
			choices.Add(choice);
		}

		var subMenu = new ToolStripMenuItem(Resources.SackPanelMenuSearch, null, choices.ToArray())
		{
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		}.StyledLike(this.CustomContextMenu);

		this.CustomContextMenu.Items.Add(subMenu);
	}

	private void FindCopiesClicked(Item focusedItem, CopySearchKind kind)
		=> (this.FindForm() as MainForm)?.ShowItemCopies(focusedItem, kind);

	private void AddExportMenuItems(Item? focusedItem, bool singleSelectionFocused)
	{
		bool single = focusedItem is not null && (this.selectedItems is null || singleSelectionFocused);
		bool several = this.selectedItems is not null && !singleSelectionFocused;
		if (!single && !several)
			return;

		// The PasteBin entry of a single item is disabled without an API key, the one of a selection left out.
		this.CustomContextMenu.Items.Add(MenuStyle.ExportSubMenu(
			this.CustomContextMenu,
			Resources.SackPanelMenuExportItem,
			(Resources.SackPanelMenuExportClipboard, this.ExportItemToClipboardClicked),
			(Resources.SackPanelMenuExportItemFile, this.ExportItemToFileClicked),
			(Resources.SackPanelMenuExportItemPasteBin, this.ExportItemToPasteBinClicked),
			this.ExchangeService?.HasPasteBinApiKey == true,
			hidePasteBinWithoutKey: several));
	}
	
	/// <summary>
	/// Adds a top level entry whose click <see cref="ContextMenuItemClicked"/> dispatches on <paramref name="action"/>.
	/// </summary>
	private void AddActionMenuItem(string text, ItemActions action)
		=> this.CustomContextMenu.Items.Add(text).Tag = action;

	private void AddRegularItemEditMenuItems(Item focusedItem)
	{
		var actions = ItemActionRules.EditActions(focusedItem);

		if (actions.HasFlag(ItemActions.Seed))
			this.AddActionMenuItem(Resources.SackPanelMenuSeed, ItemActions.Seed);

		if (actions.HasFlag(ItemActions.ForceSeed))
			this.AddActionMenuItem(Resources.SackPanelMenuSeedForce, ItemActions.ForceSeed);

		// Complete a charm or relic not already completed.
		if (actions.HasFlag(ItemActions.CompleteRelic))
		{
			this.AddActionMenuItem(
				focusedItem.IsCharmOnly ? Resources.SackPanelMenuCharm : Resources.SackPanelMenuRelic
				, ItemActions.CompleteRelic);
		}

		// Craft an artifact from formulae.
		if (actions.HasFlag(ItemActions.CraftArtifact))
			this.AddActionMenuItem(Resources.SackPanelMenuFormulae, ItemActions.CraftArtifact);
	}
	
	private void AddItemSetMenuItems(Item focusedItem)
	{
		// If the item is a set item, then add a menu to create the rest of the set
		var setMembers = this.Services.AffixCatalogService.GetSetMemberChoices(focusedItem);
		if (setMembers.Count > 0)
		{
			var choices = new List<ToolStripItem>();
			foreach (var setMember in setMembers)
			{
				var choice = new ToolStripMenuItem()
				{
					Text = setMember.Label,
					Name = setMember.RecordId,
					ToolTipText = setMember.RecordId,
				}.StyledLike(this.CustomContextMenu);
				choice.Click += NewSetItemClicked;

				choices.Add(choice);
			}

			var subMenu = new ToolStripMenuItem(Resources.SackPanelMenuSet, null, choices.ToArray())
			{
				DisplayStyle = ToolStripItemDisplayStyle.Text,
			}.StyledLike(this.CustomContextMenu);

			this.CustomContextMenu.Items.Add(subMenu);
		}
	}

	private void AddSocketedItemCompletionBonusMenuItems(Item focusedItem)
	{
		// If the item contains completed relic/charm
		// add a menu of possible completion bonuses to choose from.
		var (relic1, relic2) = this.Services.AffixCatalogService.GetSocketedBonusChoices(focusedItem);
		var text = $"{Resources.SackPanelMenuBonus} {TranslationService.TranslateXTag("tagRelic")}";

		if (relic1.Count > 0)
			this.AddBonusMenu(relic1, $"{text} 1", ChangeSocketedBonusItemClicked, true);

		if (relic2.Count > 0)
			this.AddBonusMenu(relic2, $"{text} 2", ChangeSocketedBonusItemClicked, false);
	}

	private void AddRelicOrArticaftCompletionBonusMenuItems(Item focusedItem)
	{
		// If the item is a completed relic/charm/artifact then
		// add a menu of possible completion bonuses to choose from.
		var choices = this.Services.AffixCatalogService.GetCompletionBonusChoices(focusedItem);
		if (choices.Count > 0)
			this.AddBonusMenu(choices, Resources.SackPanelMenuBonus, ChangeBonusItemClicked, null);
	}

	/// <summary>
	/// Adds a sub menu listing completion bonuses.
	/// </summary>
	/// <param name="bonuses">The bonuses, already sorted.</param>
	/// <param name="menuText">Text of the sub menu.</param>
	/// <param name="handler">Click handler of each bonus.</param>
	/// <param name="tag">Tag of each bonus menu item (which socket for socketed relics).</param>
	private void AddBonusMenu(IReadOnlyList<BonusChoice> bonuses, string menuText, EventHandler handler, object? tag)
	{
		var choices = new List<ToolStripItem>();
		foreach (var bonus in bonuses)
		{
			var choice = new ToolStripMenuItem()
			{
				Text = bonus.Label,
				Name = bonus.Id.Raw,
				ToolTipText = bonus.Id.Raw,
				Tag = tag,
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
			}.StyledLike(this.CustomContextMenu);
			choice.Click += handler;
			this.DecorateChoice(choice, bonus.Unknown, bonus.Untranslated, bonus.IsCurrent);

			choices.Add(choice);
		}

		var subMenu = new ToolStripMenuItem(menuText, null, choices.ToArray())
		{
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		}.StyledLike(this.CustomContextMenu);

		this.CustomContextMenu.Items.Add(subMenu);
	}

	/// <summary>
	/// Marks a bonus or affix choice: the icon and tool tip prefix of an unknown or untranslated one, and the current one
	/// in bold on a darker background.
	/// </summary>
	private void DecorateChoice(ToolStripMenuItem choice, bool unknown, bool untranslated, bool current)
	{
		if (unknown)
		{
			choice.Image = this.CustomContextMenuAffixUnknown;
			choice.ToolTipText = "Unknown : " + choice.ToolTipText;
		}
		else if (untranslated)
		{
			choice.Image = this.CustomContextMenuAffixUntranslated;
			choice.ToolTipText = "No Translation : " + choice.ToolTipText;
		}

		if (current)
		{
			choice.Font = this.FontService.WithStyle(choice.Font, FontStyle.Bold);
			choice.BackColor = ControlPaint.Dark(choice.BackColor);
		}
	}

	#region ContextMenuMouseWheel

	private void _VaultForm_GlobalMouseWheelUp(object? sender, EventArgs e)
	{
		if (this.CustomContextMenu.Visible) // Only for context menu & sub menu because there isn't MouseWheel event at ToolStripMenuItem
			SendKeys.SendWait("{UP}");
	}

	private void _VaultForm_GlobalMouseWheelDown(object? sender, EventArgs e)
	{
		if (this.CustomContextMenu.Visible) // Only for context menu & sub menu because there isn't MouseWheel event at ToolStripMenuItem
			SendKeys.SendWait("{DOWN}");
	}

	bool _ContextMenuMouseWheelEnabled = false;
	private void HookContextMenuMouseWheel()
	{
		// Link mouse wheel to menu for scrolling.
		// Put here because i need the Form to be fully initalized and there is no Load() event here
		if (!_ContextMenuMouseWheelEnabled)
		{
			this._VaultForm = this.FindForm() as VaultForm;
			if (this._VaultForm is not null)
			{
				this._VaultForm.GlobalMouseWheelDown += _VaultForm_GlobalMouseWheelDown;
				this._VaultForm.GlobalMouseWheelUp += _VaultForm_GlobalMouseWheelUp;
				_ContextMenuMouseWheelEnabled = true;
			}
		}
	}

	#endregion

	private void AddPrefixSuffixMenuItems(Item focusedItem)
	{
		HookContextMenuMouseWheel();

		#region Add Prefix/Suffix pickup

		AffixCatalog? affixes = this.Services.AffixCatalogService.GetAffixCatalog(focusedItem, this.USettings.EnableEpicLegendaryAffixes);
		if (affixes is not null)
		{
			this.CustomContextMenu.Items.Add("-");

			var dispStl = ToolStripItemDisplayStyle.Text;
			var tagSRemove = TranslationService.TranslateXTag("tagSRemove");
			// What the percentages mean, shown above the choices.
			var header = affixes.AllAffixesOfGearType ? Resources.AffixShareHeader : Resources.AffixChanceHeader;

			ToolStripMenuItem currentchoicesMenu;

			#region Broken

			if (affixes.HasBroken)
			{
				currentchoicesMenu = new ToolStripMenuItem()
				{
					Text = TranslationService.TranslateXTag("tagTutorialTip05TextC"),// Broken
					DisplayStyle = dispStl,
				}.StyledLike(this.CustomContextMenu);
				this.CustomContextMenu.Items.Add(currentchoicesMenu);

				BuildAffixesMenuItems(focusedItem.prefixID, currentchoicesMenu, ChangePrefixItemClicked
					, affixes.Broken, header
				);
			}

			#endregion

			#region Prefix

			if (affixes.HasPrefix)
			{
				currentchoicesMenu = new ToolStripMenuItem()
				{
					Text = Resources.GlobalPrefix,
					DisplayStyle = dispStl,
				}.StyledLike(this.CustomContextMenu);
				this.CustomContextMenu.Items.Add(currentchoicesMenu);

				BuildAffixesMenuItems(focusedItem.prefixID, currentchoicesMenu, ChangePrefixItemClicked
					, affixes.Prefix, header
				);
			}

			if (focusedItem.HasPrefix)
			{
				var prefixMenu = new ToolStripMenuItem()
				{
					Text = $"{tagSRemove} {Resources.GlobalPrefix}",
					DisplayStyle = dispStl,
				}.StyledLike(this.CustomContextMenu);
				prefixMenu.Click += RemovePrefixItemClicked;
				this.CustomContextMenu.Items.Add(prefixMenu);
			}

			#endregion

			#region Suffix

			if (affixes.HasSuffix)
			{
				currentchoicesMenu = new ToolStripMenuItem()
				{
					Text = Resources.GlobalSuffix,
					DisplayStyle = dispStl,
				}.StyledLike(this.CustomContextMenu);
				this.CustomContextMenu.Items.Add(currentchoicesMenu);

				BuildAffixesMenuItems(focusedItem.suffixID, currentchoicesMenu, ChangeSuffixItemClicked
					, affixes.Suffix, header
				);
			}

			if (focusedItem.HasSuffix)
			{
				var suffixMenu = new ToolStripMenuItem()
				{
					Text = $"{tagSRemove} {Resources.GlobalSuffix}",
					DisplayStyle = dispStl,
				}.StyledLike(this.CustomContextMenu);
				suffixMenu.Click += RemoveSuffixItemClicked;
				this.CustomContextMenu.Items.Add(suffixMenu);
			}

			#endregion

			#region Swap Affixes Display by Effect/Name

			var swapDisplayMenuItem = new ToolStripMenuItem()
			{
				Text = _DisplayAffixesByEffect
					? Resources.AffixesDisplayByName
					: Resources.AffixesDisplayByEffect,
				DisplayStyle = dispStl,
			}.StyledLike(this.CustomContextMenu);
			swapDisplayMenuItem.Click += SwapAffixesDisplayModeClicked;
			this.CustomContextMenu.Items.Add(swapDisplayMenuItem);

			#endregion
		}

		#endregion
	}


	/// <summary>
	/// Adds the Modded submenu: links the item to an installed mod, or to none.
	/// </summary>
	private void AddModMenuItems(Item focusedItem)
	{
		this.CustomContextMenu.Items.Add("-");
		this.CustomContextMenu.Items.Add(ModMenu.Create(this.CustomContextMenu, Resources.CharacterModded
			, this.Services.ItemModService, focusedItem.Mod, mod => this.ChangeModItemClicked(focusedItem, mod)
			, () => this.Services.ItemModService.Suggest([focusedItem])));
	}

	private void ChangeModItemClicked(Item focusedItem, string? mod)
	{
		// The mod's data may have loaded in the background meanwhile: the item must still be in the tab shown.
		if (this.ShownSack is not { } sack || !sack.Contains(focusedItem))
			return;

		// Read with the chosen map, the item may take another size: without room in this tab, the choice is refused
		// and the user moves the item to another tab first (MODLINK.md).
		var outcome = this.Services.ItemEditService.ChangeMod(this.ShownSack, focusedItem, mod, this.HasRoomOnceReadAgain);
		if (outcome == ModChoiceOutcome.NoRoom)
			this.NotifyNoRoomInTab(focusedItem, Resources.ModChoiceNoRoomInTab);

		if (outcome != ModChoiceOutcome.Changed)
			return;

		ItemTooltip.HideTooltip();
		this.InvalidateItemCacheAll(focusedItem);
		BagButtonTooltip.InvalidateCache(this.ShownSack);
		this.Refresh();
	}

	private void SwapAffixesDisplayModeClicked(object? sender, EventArgs e)
	{
		_DisplayAffixesByEffect = !_DisplayAffixesByEffect;
	}

	/// <summary>
	/// <c>false</c> Group By AffixName;
	/// <c>true</c> Group By Effect 
	/// </summary>
	private static bool _DisplayAffixesByEffect = false;

	private void BuildAffixesMenuItems(
		RecordId currentSelectedAffix
		, ToolStripMenuItem currentchoicesMenu
		, EventHandler handler
		, IEnumerable<AffixGroup> currentaffixGroup
		, string header
	)
	{
		var dispStl = ToolStripItemDisplayStyle.ImageAndText;

		currentchoicesMenu.DropDownItems.Add(new ToolStripMenuItem()
		{
			Text = header,
			Enabled = false,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		}.StyledLike(this.CustomContextMenu));
		currentchoicesMenu.DropDownItems.Add("-");


		if (_DisplayAffixesByEffect)// Group By Effect
			currentaffixGroup = this.Services.AffixCatalogService.GroupByEffect(currentaffixGroup);

		foreach (var grp in currentaffixGroup)
		{

			ToolStripMenuItem choicesMenu = currentchoicesMenu;// Default
			ToolStripMenuItem? affixMenu = null;
			if (grp.Count > 1)
			{
				// Multiple variant per affix
				affixMenu = new ToolStripMenuItem()
				{
					Text = grp.Key.Translation,
					DisplayStyle = dispStl,
				}.StyledLike(this.CustomContextMenu);
				choicesMenu.DropDownItems.Add(affixMenu);
				choicesMenu = affixMenu; // Add a menu level
			}

			foreach (var val in grp)
			{
				var choice = new ToolStripMenuItem()
				{
					Text = val.FormattedText,
					Name = val.AffixId.Raw,
					ToolTipText = val.AffixId.Raw,
					DisplayStyle = dispStl,
				}.StyledLike(this.CustomContextMenu);
				choice.Click += handler;

				if (affixMenu is not null)
					choice.Text = this.Services.AffixCatalogService.FormatVariantLabel(val, _DisplayAffixesByEffect);

				bool current = val.AffixId == currentSelectedAffix;
				this.DecorateChoice(choice, val.LootRandomizer.Unknown, val.LootRandomizer.TranslationTagIsEmpty, current);

				// The variants' menu of the current affix looks like it.
				if (current && affixMenu is not null)
				{
					affixMenu.Font = choice.Font;
					affixMenu.BackColor = choice.BackColor;
				}

				choicesMenu.DropDownItems.Add(choice);
			}
		}
	}
}
