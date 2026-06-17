using Microsoft.Extensions.Logging;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	private void HandleClipboardExport()
	{
		var selectedItems = GetActivePanelSelectedItems();
		if (selectedItems == null || selectedItems.Count == 0)
			return;

		try
		{
			if (selectedItems.Count == 1)
			{
				var json = this.itemExchangeService.SerializeItem(selectedItems[0]);
				Clipboard.SetText(json);
			}
			else
			{
				var json = this.itemExchangeService.SerializeItems(selectedItems);
				Clipboard.SetText(json);
			}
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to export items to clipboard");
		}
	}

	private async Task HandleClipboardImportAsync()
	{
		try
		{
			if (!Clipboard.ContainsText())
				return;

			var text = Clipboard.GetText();
			if (string.IsNullOrWhiteSpace(text))
				return;

			ImportResult result;

			// Check for PasteBin URL first
			if (this.itemExchangeService.IsPasteBinUrl(text.Trim()))
			{
				try
				{
					var url = text.Trim();
					var json = await this.itemExchangeService.ImportFromPasteBinAsync(url);
					result = this.itemExchangeService.ImportFromJson(json);

					if (result.Success)
					{
						if (result.Scope == ExportScope.Vault)
							HandleVaultImportFromJson(result);
						else
							HandleNonVaultImport(result);
					}
					else
						this.UIService.NotifyUser(result.ErrorMessage);

					return;
				}
				catch (Exception ex)
				{
					Log.LogError(ex, "Failed to import from PasteBin URL");
					this.UIService.ShowError($"Failed to import from PasteBin: {ex.Message}");
					return;
				}
			}

			result = this.itemExchangeService.ImportFromJson(text);
			if (!result.Success)
			{
				this.UIService.NotifyUser(result.ErrorMessage);
				return;
			}

			if (result.Scope == ExportScope.Vault)
				HandleVaultImportFromJson(result);
			else
				HandleNonVaultImport(result);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to import items from clipboard");
			this.UIService.NotifyUser("Invalid clipboard data.");
		}
	}

	private IReadOnlyList<Item> GetActivePanelSelectedItems()
	{
		if (this.vaultPanel?.SackPanel?.SelectionActive == true)
			return this.vaultPanel.SackPanel.GetSelectedItems();
		if (this.secondaryVaultPanel?.SackPanel?.SelectionActive == true)
			return this.secondaryVaultPanel.SackPanel.GetSelectedItems();
		if (this.playerPanel?.SackPanel?.SelectionActive == true)
			return this.playerPanel.SackPanel.GetSelectedItems();
		if (this.playerPanel?.BagSackPanel?.SelectionActive == true)
			return this.playerPanel.BagSackPanel.GetSelectedItems();
		if (this.stashPanel?.SackPanel?.SelectionActive == true)
			return this.stashPanel.SackPanel.GetSelectedItems();
		return null;
	}
}
