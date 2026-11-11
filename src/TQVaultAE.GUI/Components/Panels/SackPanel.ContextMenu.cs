//-----------------------------------------------------------------------
// <copyright file="SackPanel.ContextMenu.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
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
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				Tag = destination.BagIndex is int bagIndex ? new MenuItemTagBagIndex(bagIndex) : (object)destination.Location,
			};
			moveChoices.Add(moveChoice);
		}

		var moveSubMenu = new ToolStripMenuItem(Resources.SackPanelMenuMoveTo, null, moveChoices.ToArray())
		{
			BackColor = this.CustomContextMenu.BackColor,
			Font = this.CustomContextMenu.Font,
			ForeColor = this.CustomContextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		};

		this.CustomContextMenu.Items.Add(moveSubMenu);
	}
	
	private void AddExportMenuItems(Item? focusedItem, bool singleSelectionFocused)
	{
		if (focusedItem is not null && (this.selectedItems is null || singleSelectionFocused))
		{
			var hasApiKey = this.ExchangeService?.HasPasteBinApiKey == true;

			var exportClipboardItem = new ToolStripMenuItem(Resources.SackPanelMenuExportClipboard, null, ExportItemToClipboardClicked)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
			};

			var exportFileItem = new ToolStripMenuItem(Resources.SackPanelMenuExportItemFile, null, ExportItemToFileClicked)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
			};

			var exportPasteBinItem = new ToolStripMenuItem(Resources.SackPanelMenuExportItemPasteBin, null)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				Enabled = hasApiKey
			};
			exportPasteBinItem.Click += ExportItemToPasteBinClicked;

			var exportItemMenu = new ToolStripMenuItem(Resources.SackPanelMenuExportItem, null, exportClipboardItem, exportFileItem, exportPasteBinItem)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				DisplayStyle = ToolStripItemDisplayStyle.Text,
			};

			this.CustomContextMenu.Items.Add(exportItemMenu);
		}

		if (this.selectedItems is not null && !singleSelectionFocused)
		{
			var exportItemMenu = new ToolStripMenuItem(Resources.SackPanelMenuExportItem)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				DisplayStyle = ToolStripItemDisplayStyle.Text,
			};

			var exportClipboardItem = new ToolStripMenuItem(Resources.SackPanelMenuExportClipboard, null, ExportItemToClipboardClicked)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
			};
			exportItemMenu.DropDownItems.Add(exportClipboardItem);

			var exportFileItem = new ToolStripMenuItem(Resources.SackPanelMenuExportItemFile, null, ExportItemToFileClicked)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
			};
			exportItemMenu.DropDownItems.Add(exportFileItem);

			if (this.ExchangeService?.HasPasteBinApiKey == true)
			{
				var exportPasteBinItem = new ToolStripMenuItem(Resources.SackPanelMenuExportItemPasteBin, null, ExportItemToPasteBinClicked)
				{
					BackColor = this.CustomContextMenu.BackColor,
					Font = this.CustomContextMenu.Font,
					ForeColor = this.CustomContextMenu.ForeColor,
				};
				exportItemMenu.DropDownItems.Add(exportPasteBinItem);
			}

			this.CustomContextMenu.Items.Add(exportItemMenu);
		}
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
					BackColor = this.CustomContextMenu.BackColor,
					Font = this.CustomContextMenu.Font,
					ForeColor = this.CustomContextMenu.ForeColor,
					ToolTipText = setMember.RecordId,
				};
				choice.Click += NewSetItemClicked;

				choices.Add(choice);
			}

			var subMenu = new ToolStripMenuItem(Resources.SackPanelMenuSet, null, choices.ToArray())
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				DisplayStyle = ToolStripItemDisplayStyle.Text,
			};

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
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				ToolTipText = bonus.Id.Raw,
				Tag = tag,
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
			};
			choice.Click += handler;

			if (bonus.Unknown)
			{
				choice.Image = this.CustomContextMenuAffixUnknown;
				choice.ToolTipText = "Unknown : " + choice.ToolTipText;
			}
			else if (bonus.Untranslated)
			{
				choice.Image = this.CustomContextMenuAffixUntranslated;
				choice.ToolTipText = "No Translation : " + choice.ToolTipText;
			}

			// make the currently selected bonus bold
			if (bonus.IsCurrent)
			{
				choice.Font = new Font(choice.Font, FontStyle.Bold);
				choice.BackColor = ControlPaint.Dark(choice.BackColor);
			}

			choices.Add(choice);
		}

		var subMenu = new ToolStripMenuItem(menuText, null, choices.ToArray())
		{
			BackColor = this.CustomContextMenu.BackColor,
			Font = this.CustomContextMenu.Font,
			ForeColor = this.CustomContextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		};

		this.CustomContextMenu.Items.Add(subMenu);
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

			var fnt = this.CustomContextMenu.Font;
			var foreC = this.CustomContextMenu.ForeColor;
			var backC = this.CustomContextMenu.BackColor;
			var dispStl = ToolStripItemDisplayStyle.Text;
			var tagSRemove = TranslationService.TranslateXTag("tagSRemove");

			ToolStripMenuItem currentchoicesMenu;

			#region Broken

			if (affixes.HasBroken)
			{
				currentchoicesMenu = new ToolStripMenuItem()
				{
					Text = TranslationService.TranslateXTag("tagTutorialTip05TextC"),// Broken
					BackColor = backC,
					Font = fnt,
					ForeColor = foreC,
					DisplayStyle = dispStl,
				};
				this.CustomContextMenu.Items.Add(currentchoicesMenu);

				BuildAffixesMenuItems(focusedItem.prefixID, currentchoicesMenu, ChangePrefixItemClicked
					, affixes.Broken
				);
			}

			#endregion

			#region Prefix

			if (affixes.HasPrefix)
			{
				currentchoicesMenu = new ToolStripMenuItem()
				{
					Text = Resources.GlobalPrefix,
					BackColor = backC,
					Font = fnt,
					ForeColor = foreC,
					DisplayStyle = dispStl,
				};
				this.CustomContextMenu.Items.Add(currentchoicesMenu);

				BuildAffixesMenuItems(focusedItem.prefixID, currentchoicesMenu, ChangePrefixItemClicked
					, affixes.Prefix
				);
			}

			if (focusedItem.HasPrefix)
			{
				var prefixMenu = new ToolStripMenuItem()
				{
					Text = $"{tagSRemove} {Resources.GlobalPrefix}",
					BackColor = backC,
					Font = fnt,
					ForeColor = foreC,
					DisplayStyle = dispStl,
				};
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
					BackColor = backC,
					Font = fnt,
					ForeColor = foreC,
					DisplayStyle = dispStl,
				};
				this.CustomContextMenu.Items.Add(currentchoicesMenu);

				BuildAffixesMenuItems(focusedItem.suffixID, currentchoicesMenu, ChangeSuffixItemClicked
					, affixes.Suffix
				);
			}

			if (focusedItem.HasSuffix)
			{
				var suffixMenu = new ToolStripMenuItem()
				{
					Text = $"{tagSRemove} {Resources.GlobalSuffix}",
					BackColor = backC,
					Font = fnt,
					ForeColor = foreC,
					DisplayStyle = dispStl,
				};
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
				BackColor = backC,
				Font = fnt,
				ForeColor = foreC,
				DisplayStyle = dispStl,
			};
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
			, this.Services.ItemModService.InstalledMods, focusedItem.Mod, this.ChangeModItemClicked
			, () => this.Services.ItemModService.Suggest([focusedItem])));
	}

	private void ChangeModItemClicked(string? mod)
	{
		Item? focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is null || !this.Services.ItemEditService.ChangeMod(this.ShownSack, focusedItem, mod
			, this.HasRoomOnceReadAgain))
		{
			return;
		}

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
	)
	{
		var fnt = this.CustomContextMenu.Font;
		var foreC = this.CustomContextMenu.ForeColor;
		var backC = this.CustomContextMenu.BackColor;
		var dispStl = ToolStripItemDisplayStyle.ImageAndText;


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
					BackColor = backC,
					Font = fnt,
					ForeColor = foreC,
					DisplayStyle = dispStl,
				};
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
					BackColor = backC,
					Font = fnt,
					ForeColor = foreC,
					DisplayStyle = dispStl,
				};
				choice.Click += handler;

				if (val.LootRandomizer.Unknown)
				{
					choice.Image = this.CustomContextMenuAffixUnknown;
					choice.ToolTipText = "Unknown : " + choice.ToolTipText;
				}
				else if (val.LootRandomizer.TranslationTagIsEmpty)
				{
					choice.Image = this.CustomContextMenuAffixUntranslated;
					choice.ToolTipText = "No Translation : " + choice.ToolTipText;
				}

				if (affixMenu is not null)
					choice.Text = this.Services.AffixCatalogService.FormatVariantLabel(val, _DisplayAffixesByEffect);

				// make the currently selected affix bold
				if (val.AffixId == currentSelectedAffix)
				{
					choice.Font = new Font(choice.Font, FontStyle.Bold);
					choice.BackColor = ControlPaint.Dark(choice.BackColor);
					if (affixMenu is not null)
					{
						affixMenu.Font = choice.Font;
						affixMenu.BackColor = choice.BackColor;
					}
				}

				choicesMenu.DropDownItems.Add(choice);
			}
		}
	}
}
