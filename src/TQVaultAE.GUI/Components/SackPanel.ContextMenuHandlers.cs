//-----------------------------------------------------------------------
// <copyright file="SackPanel.ContextMenuHandlers.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Class for holding all of the UI functions of the sack panel.
/// </summary>
public partial class SackPanel
{
	/// <summary>
	/// Deletes the highlighted item
	/// </summary>
	/// <param name="focusedItem">Item containing the focus (which will get deleted)</param>
	/// <param name="suppressMessage">Determines whether we will show a delete confirmation message.</param>
	private void DeleteItem(Item focusedItem, bool suppressMessage)
	{
		if (focusedItem != null)
		{
			if (suppressMessage || USettings.SuppressWarnings || MessageBox.Show(
				Resources.SackPanelDeleteMsg,
				Resources.SackPanelDelete,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button1,
				RightToLeftOptions) == DialogResult.Yes)
			{
				// remove item
				this.Sack.RemoveItem(focusedItem);
				// Remove from search database
				this.ItemDatabaseService.RemoveItemFromDatabase(focusedItem);
				this.Refresh();
				BagButtonTooltip.InvalidateCache(this.Sack);
			}
		}
	}

	/// <summary>
	/// Adds the highlighted item to the selected items list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void AddFocusedItemToSelectedItems(object sender, MouseEventArgs e)
	{
		if (this.DisableMultipleSelection)
			return;

		this.MouseMoveCallback(sender, e); // process the mouse moving to this location...just in case

		Item focusedItem = this.FindItem(this.LastCellWithFocus);
		if (focusedItem != null)
		{
			// Allocate the List if not already done.
			if (this.selectedItems == null)
				this.selectedItems = new List<Item>();

			if (!this.IsItemSelected(focusedItem))
			{
				// Check to see if the item is already selected
				this.selectedItems.Add(focusedItem);
				this.selectedItemsChanged = true;
			}
			else
				this.RemoveSelectedItem(focusedItem);

			this.Invalidate(this.GetItemScreenRectangle(focusedItem));
		}

		this.OnItemSelected(this, new SackPanelEventArgs(null, null));
		this.MouseMoveCallback(sender, e); // process mouse move again to apply the graphical effects of dragging an item.
	}

	/// <summary>
	/// Adds the items inside of the mouse drag to the selected items list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void SelectItemsInMouseDraw(object sender, MouseEventArgs e)
	{
		if (!this.mouseDraw)
			return;

		// Allocate the List if not already done.
		if (this.selectedItems == null)
			this.selectedItems = new List<Item>();

		Collection<Item> itemsUnderDraw = FindAllItems(FindAllCells(GetMouseDragRectangle()));

		if (itemsUnderDraw != null)
		{
			foreach (Item item in itemsUnderDraw)
			{
				if (!selectedItems.Contains(item))
				{
					this.selectedItems.Add(item);
				}
			}

			this.OnItemSelected(this, new SackPanelEventArgs(null, null));
		}
	}

	/// <summary>
	/// Gets the localized string for an AutoMoveLocation.
	/// </summary>
	/// <param name="autoMoveLocation">AutoMoveLocation that is to be translated</param>
	/// <returns>Localized String name of the AutoMoveLocation</returns>
	private string GetStringFromAutoMove(AutoMoveLocation autoMoveLocation)
	{
		if (autoMoveLocation == AutoMoveLocation.SecondaryVault && this.SecondaryVaultShown)
			return Resources.SackPanelMenuVault2;

		if (autoMoveLocation == AutoMoveLocation.Player && !this.SecondaryVaultShown)
			return Resources.SackPanelMenuPlayer;

		if (autoMoveLocation == AutoMoveLocation.Stash && !this.SecondaryVaultShown)
			return Resources.SackPanelMenuStash;

		if (autoMoveLocation == AutoMoveLocation.Vault)
			return Resources.SackPanelMenuVault;

		if (autoMoveLocation == AutoMoveLocation.Trash)
			return Resources.SackPanelMenuTrash;

		return string.Empty;
	}

	/// <summary>
	/// Handler for selecting Move Item from the context menu
	/// Auto moves an item to another sack or panel.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MoveItemClicked(object sender, EventArgs e)
	{
		if (!this.DragInfo.IsActive)
		{
			Item focusedItem;
			AutoMoveLocation automoveDestination = AutoMoveLocation.NotSet;
			int destIndex = -1;

			// Find out what was selected
			ToolStripMenuItem toolStripItem = (ToolStripMenuItem)sender;
			if (toolStripItem != null)
			{
				if (toolStripItem.Tag is MenuItemTagBagIndex mit)
				{
					automoveDestination = this.AutoMoveLocation;
					destIndex = mit.Index;
				}
				else if (toolStripItem.Name == Resources.SackPanelMenuVault)
					automoveDestination = AutoMoveLocation.Vault;
				else if (toolStripItem.Name == Resources.SackPanelMenuPlayer)
					automoveDestination = AutoMoveLocation.Player;
				else if (toolStripItem.Name == Resources.SackPanelMenuTrash)
					automoveDestination = AutoMoveLocation.Trash;
				else if (toolStripItem.Name == Resources.SackPanelMenuVault2)
					automoveDestination = AutoMoveLocation.SecondaryVault;
				else if (toolStripItem.Name == Resources.SackPanelMenuStash)
					automoveDestination = AutoMoveLocation.Stash;
			}

			if (this.selectedItems != null)
			{
				// Moving selected items.
				var autoMoveQuery = from Item item in this.selectedItems
									where item != null
									orderby (((item.Height * 3) + item.Width) * 100) + item.ItemGroup descending
									select item;

				foreach (Item item in autoMoveQuery)
				{
					if (!this.DragInfo.IsActive)
					{
						// Check to make sure the last item got placed.
						this.DragInfo.Set(this, this.Sack, item, new Point(1, 1));
						this.DragInfo.AutoMoveDestination = automoveDestination;
						this.DragInfo.DestIndex = destIndex;
						this.OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
					}
				}

				this.ClearSelectedItems();
			}
			else
			{
				// Single item highlighted
				focusedItem = this.FindItem(this.contextMenuCellWithFocus);
				if (focusedItem != null)
				{
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));
					this.DragInfo.AutoMoveDestination = automoveDestination;
					this.DragInfo.DestIndex = destIndex;
					this.OnAutoMoveItem(this, new SackPanelEventArgs(null, null));
				}
			}

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);
		}
	}

	private void ExportItemToClipboardClicked(object sender, EventArgs e)
	{
		try
		{
			if (this.ExchangeService == null)
				return;

			if (this.selectedItems != null)
			{
				var json = this.ExchangeService.SerializeItems(this.selectedItems);
				Clipboard.SetText(json);
				return;
			}

			Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
			if (focusedItem == null)
				return;

			var jsonItem = this.ExchangeService.SerializeItem(focusedItem);
			Clipboard.SetText(jsonItem);
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to export item to clipboard");
		}
	}

	private void ExportItemToFileClicked(object sender, EventArgs e)
	{
		try
		{
			if (this.ExchangeService == null)
				return;

			if (this.selectedItems != null)
			{
				var json = this.ExchangeService.SerializeItems(this.selectedItems);
				using var dialog = new SaveFileDialog
				{
					Filter = "JSON files (*.json)|*.json",
					DefaultExt = "json",
					FileName = "items_export.json"
				};

				if (dialog.ShowDialog() == DialogResult.OK)
					File.WriteAllText(dialog.FileName, json);

				return;
			}

			Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
			if (focusedItem == null)
				return;

			var jsonItem = this.ExchangeService.SerializeItem(focusedItem);
			using var singleDialog = new SaveFileDialog
			{
				Filter = "JSON files (*.json)|*.json",
				DefaultExt = "json",
				FileName = "item_export.json"
			};

			if (singleDialog.ShowDialog() == DialogResult.OK)
				File.WriteAllText(singleDialog.FileName, jsonItem);
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to export item to file");
		}
	}

	private async void ExportItemToPasteBinClicked(object sender, EventArgs e)
	{
		try
		{
			if (this.ExchangeService == null)
				return;

			if (this.selectedItems != null)
			{
				var json = this.ExchangeService.SerializeItems(this.selectedItems);
				var url = await this.ExchangeService.ExportToPasteBinAsync(json, "Multi-item export");
				Clipboard.SetText(url);
				this.UIService.NotifyUser($"Items exported to PasteBin: {url}");
				return;
			}

			Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
			if (focusedItem == null)
				return;

			var jsonItem = this.ExchangeService.SerializeItem(focusedItem);
			var friendlyName = this.ItemProvider.GetFriendlyNames(focusedItem).FullNameClean;
			var pasteName = string.IsNullOrWhiteSpace(friendlyName) ? null : friendlyName;
			var urlItem = await this.ExchangeService.ExportToPasteBinAsync(jsonItem, pasteName);
			Clipboard.SetText(urlItem);
			this.UIService.NotifyUser($"Item exported to PasteBin: {urlItem}");
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to export item to PasteBin");
			this.UIService.ShowError($"Failed to export to PasteBin: {ex.Message}");
		}
	}

	/// <summary>
	/// Handler for selecting New Set Item from the context menu
	/// Creates the selected item from the set and sets it as the drag item.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void NewSetItemClicked(object sender, EventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem != null)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			if (item != null)
			{
				// Create the item
				var newId = item.Name.ToRecordId();
				Item newItem = focusedItem.MakeEmptyCopy(newId);
				ItemProvider.GetDBData(newItem);

				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

				// now drag the new item
				this.DragInfo.MarkModified(newItem);
				Refresh();

				ItemTooltip.HideTooltip();
				BagButtonTooltip.InvalidateCache(this.Sack);
			}
		}
	}

	private void RemoveSuffixItemClicked(object sender, EventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			// change the item
			focusedItem.suffixID = RecordId.Empty;
			focusedItem.suffixInfo = null;
			// mark the sack as modified also
			this.Sack.IsModified = focusedItem.IsModified = true;
			this.InvalidateItemCacheAll(focusedItem);
		}
	}

	private void RemovePrefixItemClicked(object sender, EventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			// change the item
			focusedItem.prefixID = RecordId.Empty;
			focusedItem.prefixInfo = null;
			// mark the sack as modified also
			this.Sack.IsModified = focusedItem.IsModified = true;
			this.InvalidateItemCacheAll(focusedItem);
		}
	}

	/// <summary>
	/// Handler for changing suffix from the context menu
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ChangeSuffixItemClicked(object sender, EventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
			if (menuItem is not null)
			{
				string newAffix = menuItem.Name;
				var newAffixId = newAffix.ToRecordId();

				// See if the bonus is different
				if (newAffixId != focusedItem.suffixID)
				{
					// change the item
					focusedItem.suffixID = newAffixId;
					focusedItem.suffixInfo = Database.GetInfo(newAffixId);
					// mark the sack as modified also
					this.Sack.IsModified = focusedItem.IsModified = true;
					this.InvalidateItemCacheAll(focusedItem);
				}
			}
		}
	}

	/// <summary>
	/// Handler for changing Prefix from the context menu
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ChangePrefixItemClicked(object sender, EventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
			if (menuItem is not null)
			{
				string newAffix = menuItem.Name;
				var newAffixId = newAffix.ToRecordId();

				// See if the bonus is different
				if (newAffixId != focusedItem.prefixID)
				{
					// change the item
					focusedItem.prefixID = newAffixId;
					focusedItem.prefixInfo = Database.GetInfo(newAffixId);
					// mark the sack as modified also
					this.Sack.IsModified = focusedItem.IsModified = true;
					this.InvalidateItemCacheAll(focusedItem);
				}
			}
		}
	}

	/// <summary>
	/// Handler for selecting Change Bonus from the context menu
	/// Changes a socketed item's completion bonus.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ChangeSocketedBonusItemClicked(object sender, EventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem is not null)
		{
			ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

			var isRelic1 = (menuItem.Tag as bool?) ?? false;

			if (menuItem is not null)
			{
				string newBonus = menuItem.Name;
				var newBonusId = newBonus.ToRecordId();

				// See if the bonus is different
				var relicId = isRelic1 ? focusedItem.RelicBonusId : focusedItem.RelicBonus2Id;
				if (newBonusId != relicId)
				{
					// change the item
					if (isRelic1)
					{
						focusedItem.RelicBonusId = newBonusId;
						focusedItem.RelicBonusInfo = Database.GetInfo(newBonusId);
					}
					else
					{
						focusedItem.RelicBonus2Id = newBonusId;
						focusedItem.RelicBonus2Info = Database.GetInfo(newBonusId);
					}
					// mark the sack as modified also
					this.Sack.IsModified = focusedItem.IsModified = true;
					this.InvalidateItemCacheAll(focusedItem);
				}
			}
		}
	}

	/// <summary>
	/// Handler for selecting Change Bonus from the context menu
	/// Changes an item's completion bonus.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ChangeBonusItemClicked(object sender, EventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem != null)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			if (item != null)
			{
				string newBonus = item.Name;
				var newBonusId = newBonus.ToRecordId();

				// See if the bonus is different
				if (newBonusId != focusedItem.RelicBonusId)
				{
					// change the item
					focusedItem.RelicBonusId = newBonusId;
					focusedItem.RelicBonusInfo = Database.GetInfo(newBonusId);
					focusedItem.IsModified = true;

					// mark the sack as modified also
					this.Sack.IsModified = true;
					this.InvalidateItemCacheAll(focusedItem);
				}
			}
		}
	}

	/// <summary>
	/// Handler for clicking an item on the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">ToolStripItemClickedEventArgs data</param>
	private void ContextMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		Item focusedItem = this.FindItem(this.contextMenuCellWithFocus);
		if (focusedItem != null || this.selectedItems != null)
		{
			string selectedMenuItem = e.ClickedItem.Text;
			if (selectedMenuItem == Resources.SackPanelMenuDelete)
			{
				if (this.selectedItems != null)
				{
					if (USettings.SuppressWarnings || MessageBox.Show(
						Resources.SackPanelDeleteMultiMsg,
						Resources.SackPanelDeleteMulti,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Warning,
						MessageBoxDefaultButton.Button1,
						RightToLeftOptions) == DialogResult.Yes)
					{
						foreach (Item sackSelectedItem in this.selectedItems)
							this.DeleteItem(sackSelectedItem, true);

						this.ClearSelection();
					}
				}
				else
				{
					this.DeleteItem(focusedItem, false);
				}
			}
			else if (selectedMenuItem == Resources.SackPanelMenuRemoveRelic
				|| selectedMenuItem == Resources.SackPanelMenuRemoveRelic2)
			{
				if (USettings.SuppressWarnings || MessageBox.Show(
					Resources.SackPanelRemoveRelicMsg,
					Resources.SackPanelMenuRemoveRelic,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions) == DialogResult.Yes)
				{
					// Set DragInfo to focused item.
					this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

					// pull out the relic
					Item relic = selectedMenuItem == Resources.SackPanelMenuRemoveRelic
						? ItemProvider.RemoveRelic1(focusedItem)
						: ItemProvider.RemoveRelic2(focusedItem);

					// Put relic in DragInfo
					this.DragInfo.MarkModified(relic);
					this.InvalidateItemCacheItemTooltip(focusedItem);
					Refresh();
				}
			}
			else if (selectedMenuItem == Resources.SackPanelMenuCopy)
			{
				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

				// copy the item
				Item newItem = focusedItem.Duplicate(false);

				// now drag it
				this.DragInfo.MarkModified(newItem);
				Refresh();
			}
			else if (selectedMenuItem == Resources.SackPanelMenuDuplicate)
			{
				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

				// copy the item
				Item newItem = focusedItem.Duplicate(true);

				// now drag it
				this.DragInfo.MarkModified(newItem);
				Refresh();
			}
			else if (selectedMenuItem == Resources.SackPanelMenuProperties)
			{
				var dlg = this.ServiceProvider.GetService<ItemProperties>();
				dlg.Item = focusedItem;
				dlg.ShowDialog();
			}
			else if (selectedMenuItem == Resources.SackPanelMenuSeed)
			{
				var dlg = this.ServiceProvider.GetService<ItemSeedDialog>();
				dlg.SelectedItem = focusedItem;
				int origSeed = focusedItem.Seed;
				dlg.ShowDialog();

				// See if the seed was changed
				if (focusedItem.Seed != origSeed)
				{
					// Tell the sack that it has been modified
					this.Sack.IsModified = true;
					this.InvalidateItemCacheItemTooltip(focusedItem);
				}
			}
			else if (selectedMenuItem == Resources.SackPanelMenuSeedForce)
			{
				int origSeed = focusedItem.Seed;
				focusedItem.Seed = Item.GenerateSeed();
				focusedItem.IsModified = true;

				// See if the seed was changed
				if (focusedItem.Seed != origSeed)
				{
					// Tell the sack that it has been modified
					this.Sack.IsModified = true;
					this.InvalidateItemCacheItemTooltip(focusedItem);
				}
			}
			else if (selectedMenuItem == Resources.SackPanelMenuCharm || selectedMenuItem == Resources.SackPanelMenuRelic)
			{
				focusedItem.Number = 10;

				float randPercent = (float)Item.GenerateSeed() / 0x7fff;
				LootTableCollection table = ItemProvider.BonusTableRelicOrArtifact(focusedItem);

				if (table != null && table.Length > 0)
				{
					int i = table.Length;
					foreach (var e1 in table)
					{
						i--;
						if (randPercent <= e1.Value.WeightPercent || i == 0)
						{
							focusedItem.RelicBonusId = e1.Key;
							break;
						}
						else
							randPercent -= e1.Value.WeightPercent;
					}
				}

				ItemProvider.GetDBData(focusedItem);

				focusedItem.IsModified = true;
				this.Sack.IsModified = true;
				InvalidateItemCacheItemTooltip(focusedItem);
				Refresh();
			}
			else if (selectedMenuItem == Resources.SackPanelMenuFormulae)
			{
				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

				// create artifact
				Item artifact = ItemProvider.CraftArtifact(focusedItem);

				// generate bonus
				float randPercent = (float)Item.GenerateSeed() / 0x7fff;
				LootTableCollection table = ItemProvider.BonusTableRelicOrArtifact(artifact);

				if (table != null && table.Length > 0)
				{
					int i = table.Length;
					foreach (var e1 in table)
					{
						i--;
						if (randPercent <= e1.Value.WeightPercent || i == 0)
						{
							artifact.RelicBonusId = e1.Key;
							artifact.RelicBonusInfo = Database.GetInfo(e1.Key);
							artifact.IsModified = true;
							break;
						}
						else
							randPercent -= e1.Value.WeightPercent;
					}
				}

				// Put artifact in DragInfo
				this.DragInfo.MarkModified(artifact);

				InvalidateItemCacheItemTooltip(focusedItem);

				Refresh();
			}
			else if (selectedMenuItem == Resources.SackPanelMenuSplit)
			{
				// Set DragInfo to focused item.
				this.DragInfo.Set(this, this.Sack, focusedItem, new Point(1, 1));

				// pull out all but one item
				Item newItem = focusedItem.PopAllButOneItem();

				// Put the item in DragInfo
				this.DragInfo.MarkModified(newItem);

				InvalidateItemCacheItemTooltip(focusedItem);

				Refresh();
			}
			else if (selectedMenuItem == Resources.SackPanelMenuClear)
			{
				this.ClearSelection();
				Refresh();
			}

			ItemTooltip.HideTooltip();
			BagButtonTooltip.InvalidateCache(this.Sack);

		}
	}
}
