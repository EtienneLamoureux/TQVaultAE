using System.ComponentModel;
using System.Globalization;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components.Rendering;


namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	#region Vault Export/Import

	private ScalingButton vaultExportButton = null!; // Created while the form loads.
	private ContextMenuStrip vaultExportContextMenu = null!; // Created while the form loads.

	/// <summary>Position of the "Modded vault" submenu in <see cref="vaultExportContextMenu"/>.</summary>
	private int vaultModMenuIndex;

	/// <summary>The "Modded vault" submenu shown now; null before the menu first opens.</summary>
	private ToolStripMenuItem? vaultModMenu;

	/// <summary>The "Share to PasteBin" entry of <see cref="vaultExportContextMenu"/>.</summary>
	private ToolStripItem? vaultPasteBinItem;

	/// <summary>
	/// Prepares the vault tools menu as it opens: the "Modded vault" submenu for the vault shown, and "Share to PasteBin"
	/// enabled when an API key is set (it may be set in the settings after startup).
	/// </summary>
	private void PrepareVaultMenu()
	{
		this.RefreshVaultModMenu();
		if (this.vaultPasteBinItem is not null)
			this.vaultPasteBinItem.Enabled = this.itemExchangeService?.HasPasteBinApiKey == true;
	}

	/// <summary>
	/// Rebuilds the "Modded vault" submenu for the vault shown: the base game and the installed mods, its mod checked.
	/// </summary>
	private void RefreshVaultModMenu()
	{
		if (this.vaultModMenu is not null)
		{
			this.vaultExportContextMenu.Items.Remove(this.vaultModMenu);
			this.vaultModMenu.Dispose();
		}

		var vault = this.vaultPanel.Vault;
		this.vaultModMenu = ModMenu.Create(this.vaultExportContextMenu, Resources.VaultModded
			, this.Services.ItemModService.InstalledMods, vault?.Mod, mod =>
			{
				this.vaultPanel.ChangeVaultMod(mod);
				this.vaultListComboBox.Invalidate();
				this.secondaryVaultListComboBox.Invalidate();
			}
			, vault is null ? null : () => this.Services.ItemModService.Suggest(vault.SelectMany(sack => sack)));
		this.vaultModMenu.Enabled = vault is not null;
		this.vaultExportContextMenu.Items.Insert(this.vaultModMenuIndex, this.vaultModMenu);
	}

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
			catch (Exception ex)
			{
				// Falls back to the default button when the game bitmaps are unavailable.
				Log.LogWarning(ex, "Game bitmaps for the vault export button are unavailable");
			}
		}

		var buttonSize = up?.Size ?? new Size(28, 28);
		vaultExportButton = new ScalingButton
		{
			Size = buttonSize,
			Margin = new Padding(3, 0, 0, 0)
		};

		if (up is not null)
		{
			vaultExportButton.UpBitmap = up;
			vaultExportButton.OverBitmap = over ?? up;
			vaultExportButton.DownBitmap = down ?? up;
			vaultExportButton.UseCustomGraphic = true;
			vaultExportButton.Text = string.Empty;
		}
		else
		{
			vaultExportButton.Text = "...";
			vaultExportButton.FlatStyle = FlatStyle.Flat;
			vaultExportButton.Font = FontService.GetFontLight(10F, UIService.FontZoom);
			vaultExportButton.FlatAppearance.BorderSize = 0;
		}

		vaultExportContextMenu = new ContextMenuStrip
		{
			BackColor = Theme.Background,
			ForeColor = Theme.MenuText,
			ShowImageMargin = false
		};

		vaultExportContextMenu.Items.Add(Resources.MainFormMaintainVault, null, (s, e) => this.MaintainVaultFilesDialog());

		// The vault's mod: built when the menu opens, for the vault shown then.
		vaultExportContextMenu.Items.Add("-");
		vaultModMenuIndex = vaultExportContextMenu.Items.Count;
		vaultExportContextMenu.Opening += (s, e) => this.PrepareVaultMenu();

		vaultExportContextMenu.Items.Add("-");
		vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuExportTabFile, null, ExportVaultToFileClicked);
		vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuExportTabClipboard, null, ExportVaultToClipboardClicked);

		// Enabled when the menu opens (PrepareVaultMenu).
		this.vaultPasteBinItem = vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuExportTabPasteBin, null, ExportVaultToPasteBinClicked);

		vaultExportContextMenu.Items.Add("-");
		vaultExportContextMenu.Items.Add(Resources.PlayerPanelMenuImportTabFile, null, ImportFromFileClicked);

		vaultExportButton.Click += (s, e) =>
		{
			vaultExportContextMenu.Show(vaultExportButton, new Point(0, vaultExportButton.Height));
		};

		this.flowLayoutPanelVaultSelector.Controls.Add(vaultExportButton);

		ScaleControl(this.UIService, vaultExportButton, builtInCode: true);
	}

	private void ExportVaultToFileClicked(object? sender, EventArgs e)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
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
				this.itemExchangeService.ExportVaultToFile(vault, saveDialog.FileName);
				this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ExportVaultSuccessFile, saveDialog.FileName));
			}
			catch (Exception ex)
			{
				Log.LogError(ex, "Failed to export vault to file");
				this.UIService.ShowError(Resources.ExportVaultFailFile);
			}
		}
	}

	private void ExportVaultToClipboardClicked(object? sender, EventArgs e)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
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

	private async void ExportVaultToPasteBinClicked(object? sender, EventArgs e)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
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

	private void ImportFromFileClicked(object? sender, EventArgs e)
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

		this.ImportFile(dialog.FileName);
	}

	/// <summary>
	/// Imports an exported file: items into the current tab, or a whole vault (replace or new vault, asked).
	/// </summary>
	internal void ImportFile(string fileName)
	{
		try
		{
			var result = this.itemExchangeService.ImportFromFile(fileName);
			this.HandleImportResult(result);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to import from file");
			this.UIService.NotifyUser(Resources.ImportFailFile);
		}
	}

	/// <summary>
	/// Shows the error of a failed import, or puts the imported items or vault in place.
	/// </summary>
	private void HandleImportResult(ImportResult result)
	{
		if (!result.Success)
		{
			this.UIService.NotifyUser(result.ErrorMessage ?? string.Empty);
			return;
		}

		if (result.Scope == ExportScope.Vault)
			this.HandleVaultImportFromJson(result);
		else
			this.HandleNonVaultImport(result);
	}

	private void HandleNonVaultImport(ImportResult importResult)
	{
		if (this.vaultPanel.Player is null)
		{
			this.UIService.ShowError(Resources.GlobalNoVaultImportTarget);
			return;
		}

		var sackPanel = this.vaultPanel.SackPanel;
		var sack = sackPanel.Sack;
		if (sack is null)
		{
			this.UIService.ShowError(Resources.GlobalNoActiveTabImport);
			return;
		}

		var before = sack.ToHashSet();
		var imported = this.itemExchangeService.PlaceImportedItems(sack, importResult, sackPanel.SackSize.Width, sackPanel.SackSize.Height
			, sackPanel.ContainerMap);

		// Into a vault linked to a mod: the imported items follow its link like dropped ones.
		this.vaultPanel.AdoptVaultMod(sack, sack.Where(item => !before.Contains(item)));

		if (importResult.Scope == ExportScope.Tab && importResult.TabIconInfo is not null)
		{
			var button = this.vaultPanel.BagButtons[this.vaultPanel.CurrentBag];
			button.ApplyIconInfo(sack);
		}

		this.vaultPanel.Refresh();
		this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportSuccessCount, imported, importResult.TotalCount));
		this.NotifyDroppedMods(importResult);
	}

	/// <summary>
	/// Names the mods an import dropped because they are not installed here.
	/// </summary>
	private void NotifyDroppedMods(ImportResult importResult)
	{
		if (importResult.DroppedMods.Count != 0)
			this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.ImportDroppedMods, string.Join(", ", importResult.DroppedMods)));
	}

	private void HandleVaultImportFromJson(ImportResult importResult)
	{
		var targetVault = this.vaultPanel.Player;
		if (targetVault is null)
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

		TaskDialogButton result;
#if DEBUG
		// Automation answers this dialog like a message box: Yes replaces, No creates a new vault, Cancel cancels.
		if (TQVaultAE.GUI.Automation.AutomationHost.TryTakeDialogAnswer(page.Heading, page.Text, MessageBoxButtons.YesNoCancel, out var automated))
			result = automated switch
			{
				DialogResult.Yes => replaceButton,
				DialogResult.No => createButton,
				_ => TaskDialogButton.Cancel,
			};
		else
#endif
			result = TaskDialog.ShowDialog(page);

		if (result == TaskDialogButton.Cancel)
			return;

		if (result == createButton)
		{
			var newName = this.itemExchangeService.ImportAsNewVault(importResult, DateTime.Now);

			this.GetVaultList(false);
			this.vaultListComboBox.SelectedItem = newName;
			this.LoadVault(newName, false);

			this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportVaultSuccessCreate, newName));
			this.NotifyDroppedMods(importResult);
			return;
		}

		if (!this.itemExchangeService.IsVaultEmpty(targetVault))
		{
			var warning = this.UIService.ShowWarning(
				Resources.ImportVaultWarning,
				null,
				ShowMessageButtons.OKCancel);

			if (!warning.IsOK)
				return;
		}

		this.itemExchangeService.ImportVaultInto(targetVault, importResult);

		// Into a vault linked to a mod: the imported items follow its link like dropped ones.
		foreach (var sack in targetVault)
			this.vaultPanel.AdoptVaultMod(sack, [.. sack]);

		this.vaultPanel.Refresh();

		if (importResult.SackIconInfo is not null)
		{
			for (int i = 0; i < targetVault.NumberOfSacks && i < this.vaultPanel.BagButtons.Count; i++)
			{
				if (importResult.SackIconInfo.ContainsKey(i))
				{
					var sack = targetVault.GetSack(i);
					if (sack is not null)
						this.vaultPanel.BagButtons[i].ApplyIconInfo(sack);
				}
			}
		}

		var totalItems = importResult.SackItems?.Sum(kvp => kvp.Value.Count) ?? 0;
		this.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportVaultSuccess, totalItems));
		this.NotifyDroppedMods(importResult);
	}

	#endregion
}
