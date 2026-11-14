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
		var selectedItems = this.mainView.SelectedItems();
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
}
