using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	private void HandleClipboardExport()
	{
		var selectedItems = GetActivePanelSelectedItems();
		if (selectedItems is null || selectedItems.Count == 0)
			return;

		try
		{
			var json = this.itemExchangeService.SerializeSelection(selectedItems);
			if (json is not null)
				Clipboard.SetText(json);
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

			var result = await this.itemExchangeService.ImportFromTextAsync(text);
			this.HandleImportResult(result);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to import items from clipboard");
			this.UIService.NotifyUser("Invalid clipboard data.");
		}
	}

	private IReadOnlyList<Item>? GetActivePanelSelectedItems()
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
