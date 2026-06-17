using System.ComponentModel;
using System.Globalization;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;


namespace TQVaultAE.GUI;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	#region Vault Export/Import

	private ScalingButton vaultExportButton;
	private ContextMenuStrip vaultExportContextMenu;

	private void SetupVaultExportButton()
	{
		var up = default(Bitmap);
		var over = default(Bitmap);
		var down = default(Bitmap);
		if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
		{
			try
			{
				up = this.UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONUP01.TEX");
				over = this.UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONOVER01.TEX");
				down = this.UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONDOWN01.TEX");
			}
			catch
			{
				// fall through to default if game bitmaps unavailable
			}
		}

		var buttonSize = up?.Size ?? new Size(28, 28);
		vaultExportButton = new ScalingButton
		{
			Size = buttonSize,
			Margin = new Padding(3, 0, 0, 0)
		};

		if (up != null)
		{
			vaultExportButton.UpBitmap = up;
			vaultExportButton.OverBitmap = over;
			vaultExportButton.DownBitmap = down;
			vaultExportButton.UseCustomGraphic = true;
			vaultExportButton.Text = string.Empty;
		}
		else
		{
			vaultExportButton.Text = "...";
			vaultExportButton.FlatStyle = FlatStyle.Flat;
			vaultExportButton.Font = FontService.GetFontLight(10F, UIService.Scale);
			vaultExportButton.FlatAppearance.BorderSize = 0;
		}

		vaultExportContextMenu = new ContextMenuStrip
		{
			BackColor = Color.FromArgb(46, 41, 31),
			ForeColor = Color.FromArgb(200, 200, 200),
			ShowImageMargin = false
		};

		vaultExportContextMenu.Items.Add(Resources.MainFormMaintainVault, null, (s, e) => this.MaintainVaultFilesDialog());

		vaultExportContextMenu.Items.Add("-");
		vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuExportTabFile, null, ExportVaultToFileClicked);
		vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuExportTabClipboard, null, ExportVaultToClipboardClicked);

		var pasteBinEnabled = this.itemExchangeService?.HasPasteBinApiKey == true;
		var pasteBinItem = vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuExportTabPasteBin, null, ExportVaultToPasteBinClicked);
		pasteBinItem.Enabled = pasteBinEnabled;

		vaultExportContextMenu.Items.Add("-");
		vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuImportTabFile, null, ImportFromFileClicked);

		vaultExportButton.Click += (s, e) =>
		{
			vaultExportContextMenu.Show(vaultExportButton, new Point(0, vaultExportButton.Height));
		};

		this.flowLayoutPanelVaultSelector.Controls.Add(vaultExportButton);

		ScaleControl(this.UIService, vaultExportButton);
	}

	private void ExportVaultToFileClicked(object sender, EventArgs e)
	{
		var vault = this.vaultPanel?.Player;
		if (vault == null)
		{
			this.UIService.ShowWarning(Resources.GlobalNoVaultLoaded);
			return;
		}

		using var saveDialog = new SaveFileDialog
		{
			Filter = Resources.GlobalJsonFileFilter,
			FileName = $"{vault.PlayerName}.json",
			DefaultExt = "json"
		};

		if (saveDialog.ShowDialog() == DialogResult.OK)
		{
			try
			{
				var json = this.itemExchangeService.SerializePlayerCollection(vault);
				System.IO.File.WriteAllText(saveDialog.FileName, json);
				this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ExportVaultSuccessFile, saveDialog.FileName));
			}
			catch (Exception ex)
			{
				Log.LogError(ex, "Failed to export vault to file");
				this.UIService.ShowError(Resources.ExportVaultFailFile);
			}
		}
	}

	private void ExportVaultToClipboardClicked(object sender, EventArgs e)
	{
		var vault = this.vaultPanel?.Player;
		if (vault == null)
		{
			this.UIService.ShowWarning(Resources.GlobalNoVaultLoaded);
			return;
		}

		try
		{
			var json = this.itemExchangeService.SerializePlayerCollection(vault);
			Clipboard.SetText(json);
			this.UIService.NotifyUser(Resources.ExportVaultSuccessClipboard);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to export vault to clipboard");
			this.UIService.ShowError(Resources.ExportVaultFailClipboard);
		}
	}

	private async void ExportVaultToPasteBinClicked(object sender, EventArgs e)
	{
		var vault = this.vaultPanel?.Player;
		if (vault == null)
		{
			this.UIService.ShowWarning(Resources.GlobalNoVaultLoaded);
			return;
		}

		try
		{
			var json = this.itemExchangeService.SerializePlayerCollection(vault);
			var url = await this.itemExchangeService.ExportToPasteBinAsync(json, vault.PlayerName);
			Clipboard.SetText(url);
			this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ExportVaultSuccessPasteBin, url));
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to export vault to PasteBin");
			this.UIService.ShowError(string.Format(CultureInfo.InvariantCulture, Resources.ExportVaultFailPasteBin, ex.Message));
		}
	}

	private void ImportFromFileClicked(object sender, EventArgs e)
	{
		ImportFromFile();
	}

	internal void ImportFromFile()
	{
		using var dialog = new OpenFileDialog
		{
			Filter = Resources.GlobalJsonFileFilter,
			DefaultExt = "json",
			FileName = "*.json"
		};

		if (dialog.ShowDialog() != DialogResult.OK)
			return;

		try
		{
			var json = File.ReadAllText(dialog.FileName);
			var result = this.itemExchangeService.ImportFromJson(json);

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
			Log.LogError(ex, "Failed to import from file");
			this.UIService.NotifyUser(Resources.ImportFailFile);
		}
	}

	private void HandleNonVaultImport(ImportResult importResult)
	{
		if (this.vaultPanel?.Player == null)
		{
			this.UIService.ShowError(Resources.GlobalNoVaultImportTarget);
			return;
		}

		var sackPanel = this.vaultPanel.SackPanel;
		var sack = sackPanel.Sack;
		if (sack == null)
		{
			this.UIService.ShowError(Resources.GlobalNoActiveTabImport);
			return;
		}

		var items = importResult.Scope == ExportScope.Item
			? new[] { importResult.Item }
			: importResult.Items ?? [];

		var imported = 0;

		foreach (var item in items)
		{
			var x = Math.Max(0, item.PositionX);
			var y = Math.Max(0, item.PositionY);
			var w = item.Width;
			var h = item.Height;

			if (sackPanel.IsCellAvailable(x, y, w, h))
			{
				item.PositionX = x;
				item.PositionY = y;
			}
			else
			{
				var pos = sackPanel.FindOpenCells(w, h);
				if (pos.X < 0)
					continue;

				item.PositionX = pos.X;
				item.PositionY = pos.Y;
			}

			sack.AddItem(item);
			imported++;
		}

		if (importResult.Scope == ExportScope.Tab && importResult.TabIconInfo != null)
		{
			sack.BagButtonIconInfo = importResult.TabIconInfo;
			var button = this.vaultPanel.BagButtons[this.vaultPanel.CurrentBag];
			button.ApplyIconInfo(sack);
		}

		sack.IsModified = true;
		this.vaultPanel.Refresh();
		this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportSuccessCount, imported, importResult.TotalCount));
	}

	private void HandleVaultImportFromJson(ImportResult importResult)
	{
		var targetVault = this.vaultPanel?.Player;
		if (targetVault == null)
		{
			this.UIService.ShowError(Resources.GlobalNoVaultImportTarget);
			return;
		}

		var replaceButton = new TaskDialogButton(Resources.ImportVaultReplaceButton);
		var createButton = new TaskDialogButton(Resources.ImportVaultCreateNewButton);

		var page = new TaskDialogPage
		{
			Text = string.Format(CultureInfo.InvariantCulture, Resources.ImportVaultDialogText, importResult.VaultName),
			Heading = Resources.ImportVaultHeading,
			Icon = TaskDialogIcon.Information,
			Buttons = { replaceButton, createButton, TaskDialogButton.Cancel }
		};

		var result = TaskDialog.ShowDialog(page);

		if (result == TaskDialogButton.Cancel)
			return;

		if (result == createButton)
		{
			var baseName = importResult.VaultName;
			var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			var newName = $"{baseName}_{timestamp}";

			var newVault = this.vaultService.CreateVault(newName);
			this.itemExchangeService.ImportVaultInto(newVault, importResult);
			foreach (var sack in newVault)
				sack.IsModified = true;

			this.vaultService.SaveVault(newVault, newName);

			this.GetVaultList(false);
			this.vaultListComboBox.SelectedItem = newName;
			this.LoadVault(newName, false);

			this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportVaultSuccessCreate, newName));
			return;
		}

		var nonEmpty = false;
		for (int i = 0; i < targetVault.NumberOfSacks; i++)
		{
			var sack = targetVault.GetSack(i);
			if (sack != null && !sack.IsEmpty)
			{
				nonEmpty = true;
				break;
			}
		}

		if (nonEmpty)
		{
			var warning = this.UIService.ShowWarning(
				Resources.ImportVaultWarning,
				null,
				ShowMessageButtons.OKCancel);

			if (!warning.IsOK)
				return;
		}

		this.itemExchangeService.ImportVaultInto(targetVault, importResult);
		this.vaultPanel.Refresh();

		if (importResult.SackIconInfo != null)
		{
			for (int i = 0; i < targetVault.NumberOfSacks && i < this.vaultPanel.BagButtons.Count; i++)
			{
				if (importResult.SackIconInfo.ContainsKey(i))
				{
					var sack = targetVault.GetSack(i);
					if (sack != null)
						this.vaultPanel.BagButtons[i].ApplyIconInfo(sack);
				}
			}
		}

		var totalItems = importResult.SackItems?.Sum(kvp => kvp.Value.Count) ?? 0;
		this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportVaultSuccess, totalItems));
	}

	#endregion
}
