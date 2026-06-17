//-----------------------------------------------------------------------
// <copyright file="SackPanel.ContextMenu.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel
{

	private void AddAutoMoveMenuItems(Item? focusedItem)
	{
		List<(string label, int? bagIndex)> choices = new();

		if (this.MaxSacks > 1)
		{
			for (int i = 0; i < this.MaxSacks; ++i)
			{
				// The sacks do not need to list sack#0 since it is the Main player panel
				var bagOffest = focusedItem.Place.SackType switch
				{
					SackType.Player => 0,
					SackType.Vault => 1,
					_ => 0,
				};

				string label;
				if (this.SackType == SackType.Player && i == 0)
					label = Resources.SackPanelMenuPlayer;
				else if (this.SackType == SackType.Player && i >= this.PlayerCollection.NumberOfSacks)
					continue;
				else
					label = string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, i + bagOffest);

				if (i != focusedItem.Place.SackNumber)
				{
					choices.Add((label, i));
				}
			}
		}

		var autoMoveChoices = (
			from location in this.DragInfo.AllAutoMoveLocations
			where location != this.AutoMoveLocation
			select location
		).Distinct();

		// TQ original save
		if (this.userContext.CurrentPlayer is not null && !this.userContext.CurrentPlayer.IsImmortalThrone)
		{
			autoMoveChoices = autoMoveChoices.Where(loc =>
					loc != Models.AutoMoveLocation.Stash // There is no Stash on TQ original save
					&& loc != Models.AutoMoveLocation.Trash // TODO What is that ?
			);

			// You can't move TQIT+ items in Equipement, Sack and inventory
			if (focusedItem.GameDlc != GameDlc.TitanQuest)
			{
				autoMoveChoices = autoMoveChoices.Where(loc =>
					loc != Models.AutoMoveLocation.Player
				);
			}
		}

		foreach (var choice in autoMoveChoices)
		{
			string location = this.GetStringFromAutoMove(choice);
			if (!string.IsNullOrEmpty(location))
				choices.Add((location, null));
		}

		var moveChoices = new List<ToolStripItem>();
		foreach (var choice in choices)
		{
			var moveChoice = new ToolStripMenuItem(choice.label, null, MoveItemClicked, choice.label)
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				Tag = choice.bagIndex.HasValue ? new MenuItemTagBagIndex(choice.bagIndex.Value) : null,
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
		if (focusedItem != null && (this.selectedItems == null || singleSelectionFocused))
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

		if (this.selectedItems != null && !singleSelectionFocused)
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
	
	private void AddRegularItemEditMenuItems(Item focusedItem)
	{
		this.CustomContextMenu.Items.Add(Resources.SackPanelMenuSeed);
		this.CustomContextMenu.Items.Add(Resources.SackPanelMenuSeedForce);

		// Add option to complete a charm or relic if
		// not already completed.
		if (focusedItem.IsRelicOrCharm && !focusedItem.IsRelicComplete)
		{
			if (focusedItem.IsCharmOnly)
				this.CustomContextMenu.Items.Add(Resources.SackPanelMenuCharm);
			else
				this.CustomContextMenu.Items.Add(Resources.SackPanelMenuRelic);
		}

		// Add option to craft an artifact from formulae.
		if (focusedItem.IsFormulae)
			this.CustomContextMenu.Items.Add(Resources.SackPanelMenuFormulae);
	}
	
	private void AddItemSetMenuItems(Item focusedItem)
	{
		// If the item is a set item, then add a menu to create the rest of the set
		var setItems = ItemProvider.GetSetItems(focusedItem);
		if (setItems?.setMembers?.Any() ?? false)
		{
			var choices = new List<ToolStripItem>();
			foreach (var setPiece in setItems.setMembers)
			{
				// do not put the current item in the menu
				var setPieceId = setPiece.Key.ToRecordId();
				if (focusedItem.BaseItemId == setPieceId) continue;

				// Get the name of the item
				Info info = setPiece.Value;
				if (info is null) continue;

				var choice = new ToolStripMenuItem()
				{
					Text = this.TranslationService.TranslateXTag(info.DescriptionTag),
					Name = setPiece.Key,
					BackColor = this.CustomContextMenu.BackColor,
					Font = this.CustomContextMenu.Font,
					ForeColor = this.CustomContextMenu.ForeColor,
					ToolTipText = setPiece.Key,
				};
				choice.Click += NewSetItemClicked;

				choices.Add(choice);
			}

			var subMenu = new ToolStripMenuItem(Resources.SackPanelMenuSet, null, choices.OrderBy(i => i.Text).ToArray())
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
		if (ItemProvider.BonusTableSocketedRelic(focusedItem, out var ltrel1, out var ltrel2))
		{
			var text = $"{Resources.SackPanelMenuBonus} {TranslationService.TranslateXTag("tagRelic")}";

			if (ltrel1?.Any() ?? false)
				BuildMenu(focusedItem, ltrel1, $"{text} 1", true);

			if (ltrel2?.Any() ?? false)
				BuildMenu(focusedItem, ltrel2, $"{text} 2", false);

		}

		void BuildMenu(Item itm, LootTableCollection table, string menuText, bool isRelic1)
		{
			var choices = new List<ToolStripItem>();
			foreach (var tableitem in table)
			{
				var choice = new ToolStripMenuItem()
				{
					Text = string.Format("{2} : {0} ({1:p2}) {3}"
						, tableitem.Key.PrettyFileName
						, tableitem.Value.WeightPercent
						, tableitem.Value.LootRandomizer.Translation
						, tableitem.Key.Dlc.GetSuffix()
					),
					Name = tableitem.Key.Raw,
					BackColor = this.CustomContextMenu.BackColor,
					Font = this.CustomContextMenu.Font,
					ForeColor = this.CustomContextMenu.ForeColor,
					ToolTipText = tableitem.Key.Raw,
					Tag = isRelic1,
					DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
				};
				choice.Click += ChangeSocketedBonusItemClicked;

				if (tableitem.Value.LootRandomizer.Unknown)
				{
					choice.Image = this.CustomContextMenuAffixUnknown;
					choice.ToolTipText = "Unknown : " + choice.ToolTipText;
				}
				else if (tableitem.Value.LootRandomizer.TranslationTagIsEmpty)
				{
					choice.Image = this.CustomContextMenuAffixUntranslated;
					choice.ToolTipText = "No Translation : " + choice.ToolTipText;
				}

				// make the currently selected bonus bold
				var relicId = isRelic1 ? itm.RelicBonusId : itm.RelicBonus2Id;
				if (tableitem.Key == relicId)
				{
					choice.Font = new Font(choice.Font, FontStyle.Bold);
					choice.BackColor = ControlPaint.Dark(choice.BackColor);
				}

				choices.Add(choice);
			}

			var subMenu = new ToolStripMenuItem(menuText, null, choices.OrderBy(i => i.Text).ToArray())
			{
				BackColor = this.CustomContextMenu.BackColor,
				Font = this.CustomContextMenu.Font,
				ForeColor = this.CustomContextMenu.ForeColor,
				DisplayStyle = ToolStripItemDisplayStyle.Text,
			};

			this.CustomContextMenu.Items.Add(subMenu);
		}
	}

	private void AddRelicOrArticaftCompletionBonusMenuItems(Item focusedItem)
	{
		// If the item is a completed relic/charm/artifact then
		// add a menu of possible completion bonuses to choose from.
		if ((focusedItem.IsRelicOrCharm && focusedItem.IsRelicComplete)
			|| focusedItem.IsArtifact
		)
		{
			LootTableCollection table = ItemProvider.BonusTableRelicOrArtifact(focusedItem);
			if (table?.Any() ?? false)
			{
				var choices = new List<ToolStripItem>();
				foreach (var tableitem in table)
				{
					var choice = new ToolStripMenuItem()
					{
						Text = string.Format("{0} : {1} ({2:p2}) {3}"
							, tableitem.Value.LootRandomizer.Translation
							, tableitem.Key.PrettyFileName
							, tableitem.Value.WeightPercent
							, tableitem.Key.Dlc.GetSuffix()
						),
						Name = tableitem.Key.Raw,
						BackColor = this.CustomContextMenu.BackColor,
						Font = this.CustomContextMenu.Font,
						ForeColor = this.CustomContextMenu.ForeColor,
						ToolTipText = tableitem.Key.Raw,
						DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
					};
					choice.Click += ChangeBonusItemClicked;

					if (tableitem.Value.LootRandomizer.Unknown)
					{
						choice.Image = this.CustomContextMenuAffixUnknown;
						choice.ToolTipText = "Unknown : " + choice.ToolTipText;
					}
					else if (tableitem.Value.LootRandomizer.TranslationTagIsEmpty)
					{
						choice.Image = this.CustomContextMenuAffixUntranslated;
						choice.ToolTipText = "No Translation : " + choice.ToolTipText;
					}

					// make the currently selected bonus bold
					if (tableitem.Key == focusedItem.RelicBonusId)
					{
						choice.Font = new Font(choice.Font, FontStyle.Bold);
						choice.BackColor = ControlPaint.Dark(choice.BackColor);
					}

					choices.Add(choice);
				}

				var subMenu = new ToolStripMenuItem()
				{
					Text = Resources.SackPanelMenuBonus,
					BackColor = this.CustomContextMenu.BackColor,
					Font = this.CustomContextMenu.Font,
					ForeColor = this.CustomContextMenu.ForeColor,
					DisplayStyle = ToolStripItemDisplayStyle.Text,
				};

				subMenu.DropDownItems.AddRange(choices.OrderBy(i => i.Text).ToArray());

				this.CustomContextMenu.Items.Add(subMenu);
			}
		}
	}

	#region ContextMenuMouseWheel

	private void _VaultForm_GlobalMouseWheelUp(object sender, EventArgs e)
	{
		if (this.CustomContextMenu.Visible) // Only for context menu & sub menu because there isn't MouseWheel event at ToolStripMenuItem
			SendKeys.SendWait("{UP}");
	}

	private void _VaultForm_GlobalMouseWheelDown(object sender, EventArgs e)
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
			this._VaultForm.GlobalMouseWheelDown += _VaultForm_GlobalMouseWheelDown;
			this._VaultForm.GlobalMouseWheelUp += _VaultForm_GlobalMouseWheelUp;
			_ContextMenuMouseWheelEnabled = true;
		}
	}

	#endregion

	private void AddPrefixSuffixMenuItems(Item focusedItem)
	{
		HookContextMenuMouseWheel();

		#region Add Prefix/Suffix pickup

		if (focusedItem.IsArmor || focusedItem.IsWeaponShield || focusedItem.IsJewellery)
		{
			ItemAffixes affixes;
			if (USettings.EnableEpicLegendaryAffixes
				&& (focusedItem.Rarity == Rarity.Epic || focusedItem.Rarity == Rarity.Legendary))
			{
				// Get all available affixes for an item type
				affixes = this.ItemProvider.GetAllAvailableAffixes(focusedItem.GearType);
			}
			else
			{
				// Get affixes related to a specific item
				affixes = this.ItemProvider.GetItemAffixes(focusedItem.BaseItemId);
			}

			if (affixes is not null)
			{
				this.CustomContextMenu.Items.Add("-");

				var fnt = this.CustomContextMenu.Font;
				var foreC = this.CustomContextMenu.ForeColor;
				var backC = this.CustomContextMenu.BackColor;
				var dispStl = ToolStripItemDisplayStyle.Text;
				var tagSRemove = TranslationService.TranslateXTag("tagSRemove");

				#region curate all affixes at once

				var AffixTypes = new[] { // UNPIVOT
						(AffixTypeId: 0, AffixType: affixes.Broken),
						(AffixTypeId: 1, AffixType: affixes.Prefix),
						(AffixTypeId: 2, AffixType: affixes.Suffix)
					};

				var curatedAffixes =
					from types in AffixTypes
					from dlc in types.AffixType
					from ltvalues in dlc.Value
					from value in ltvalues
					select new
					{
						TypeId = types.AffixTypeId,
						AffixId = value.Key,
						value.Value.WeightPercent,
						value.Value.LootRandomizer
					} into flat
					group flat by new { flat.TypeId, flat.AffixId } into grp
					let f = grp.First()
					let _AffixIdDlc = grp.Key.AffixId.Dlc
					let _translation = f.LootRandomizer.Translation
					let _WeightPercent = grp.Max(v => v.WeightPercent)
					let affixEntry = new AffixEntry(
						grp.Key.TypeId,
						grp.Key.AffixId,
						_AffixIdDlc,
						_translation,
						_WeightPercent,
						string.Format("{0} : {1} ({2:p2}) {3}" // Default format for Order by affix name
							, _translation
							, grp.Key.AffixId.PrettyFileName
							, _WeightPercent
							, _AffixIdDlc.GetSuffix()
						),
						f.LootRandomizer
					)
					group affixEntry by new AffixGroupKey(affixEntry.TypeId, affixEntry.Translation) into grp2
					orderby grp2.Key.TypeId, grp2.Key.Translation // Order by affix name
					select new AffixGroup(grp2.Key, grp2);

				#endregion

				ToolStripMenuItem currentchoicesMenu;

				#region Broken

				if (affixes.Broken.Count > 0)
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
						, curatedAffixes.Where(fp => fp.Key.TypeId == 0) // Broken
					);
				}

				#endregion

				#region Prefix

				if (affixes.Prefix.Count > 0)
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
						, curatedAffixes.Where(fp => fp.Key.TypeId == 1) // Prefix
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

				if (affixes.Suffix.Count > 0)
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
						, curatedAffixes.Where(fp => fp.Key.TypeId == 2) // Suffix
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
		}

		#endregion
	}


	private void SwapAffixesDisplayModeClicked(object sender, EventArgs e)
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
		{
			currentaffixGroup =
				from grp in currentaffixGroup
				from av in grp
				let effect = av.AffixId.PrettyFileNameExploded.Effect
				let flattenedAffix = new AffixEntry(
					av.TypeId,
					av.AffixId,
					av.AffixIdDlc,
					effect,
					av.WeightPercent,
					string.Format("{0} : ({1}) {2} ({3:p2}) {4}"
						, effect
						, av.AffixId.PrettyFileNameExploded.Number
						, av.Translation
						, av.WeightPercent
						, av.AffixIdDlc.GetSuffix()
					),
					av.LootRandomizer
				)
				orderby flattenedAffix.Translation, flattenedAffix.AffixId.PrettyFileNameExploded.Number
				group flattenedAffix by new AffixGroupKey(flattenedAffix.TypeId, flattenedAffix.Translation) into grp2
				select new AffixGroup(grp2.Key, grp2);
		}

		foreach (var grp in currentaffixGroup)
		{

			ToolStripMenuItem choicesMenu = currentchoicesMenu;// Default
			ToolStripMenuItem affixMenu = null;
			if (grp.Count() > 1)
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
				{
					choice.Text = _DisplayAffixesByEffect
							// By Effect
							? string.Format("({0}) {1} ({2:p2}) {3}"
								, val.AffixId.PrettyFileNameExploded.Number
								, val.LootRandomizer.Translation // Sub menu item display affix Name
								, val.WeightPercent
								, val.AffixIdDlc.GetSuffix()
							)
							// By Name
							: string.Format("({0}) {1} ({2:p2}) {3}"
								, val.AffixId.PrettyFileNameExploded.Number
								, val.AffixId.PrettyFileNameExploded.Effect // Sub menu item display affix Effect
								, val.WeightPercent
								, val.AffixIdDlc.GetSuffix()
							);
				}

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
