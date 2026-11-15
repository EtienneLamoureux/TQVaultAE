using Microsoft.Extensions.Logging;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The clipboard keys of the main content: the selected items copied as text, items pasted from text.
/// </summary>
public partial class MainFormUserControl
{
	/// <summary>Copies the selected items to the clipboard as exported text.</summary>
	internal void CopySelectionToClipboard()
	{
		var selectedItems = this.SelectedItems();
		if (selectedItems is null || selectedItems.Count == 0)
			return;

		try
		{
			var json = this.Services.ItemExchangeService.SerializeSelection(selectedItems);
			if (json is not null)
				Clipboard.SetText(json);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to export items to clipboard");
		}
	}

	/// <summary>Imports the items of the clipboard's text (exported JSON, or a PasteBin link) into the panels.</summary>
	internal async Task PasteFromClipboardAsync()
	{
		try
		{
			if (!Clipboard.ContainsText())
				return;

			var text = Clipboard.GetText();
			if (string.IsNullOrWhiteSpace(text))
				return;

			var result = await this.Services.ItemExchangeService.ImportFromTextAsync(text);
			this.HandleImportResult(result);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to import items from clipboard");
			this.Services.UIService.NotifyUser("Invalid clipboard data.");
		}
	}
}
