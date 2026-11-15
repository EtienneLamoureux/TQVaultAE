using System.ComponentModel;
using System.Globalization;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;


namespace TQVaultAE.GUI.Views;

/// <summary>
/// The vault tools button next to the vault list, with its menu (maintenance, the vault's mod, export and import), and
/// the placement of imported items and vaults.
/// </summary>
public partial class MainFormUserControl
{
	#region Vault Export/Import

	private ScalingButton vaultExportButton = null!; // Created while the form loads.
	private ContextMenuStrip vaultExportContextMenu = null!; // Created while the form loads.

	/// <summary>The menu of the vault tools button.</summary>
	internal ContextMenuStrip VaultMenu => this.vaultExportContextMenu;

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
	internal void PrepareVaultMenu()
	{
		this.RefreshVaultModMenu();
		if (this.vaultPasteBinItem is not null)
			this.vaultPasteBinItem.Enabled = this.Services.ItemExchangeService.HasPasteBinApiKey == true;
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
			, this.Services.ItemModService, vault?.Mod, mod =>
			{
				// The mod's data may have loaded in the background meanwhile: still the vault shown.
				if (vault is null || this.vaultPanel.Vault != vault)
					return;

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
				up = this.Services.UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONUP01.TEX");
				over = this.Services.UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONOVER01.TEX");
				down = this.Services.UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONDOWN01.TEX");
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
			vaultExportButton.Font = this.Services.FontService.GetFontLight(10F, this.Services.UIService.FontZoom);
			vaultExportButton.FlatAppearance.BorderSize = 0;
		}

		vaultExportContextMenu = new ContextMenuStrip
		{
			ShowImageMargin = false
		};
		MenuStyle.Apply(vaultExportContextMenu, this.Services.FontService, this.Services.UIService.FontZoom);

		vaultExportContextMenu.Items.Add(Resources.MainFormMaintainVault, null, (s, e) => this.MaintainVaultFilesDialog());

		// The vault's mod: built when the menu opens, for the vault shown then.
		vaultExportContextMenu.Items.Add("-");
		vaultModMenuIndex = vaultExportContextMenu.Items.Count;
		vaultExportContextMenu.Opening += (s, e) =>
		{
			// The font at the zoom of now (the menu is built once), before the mod sub menu copies it.
			vaultExportContextMenu.Font = MenuStyle.MenuFont(this.Services.FontService, this.Services.UIService.FontZoom);
			this.PrepareVaultMenu();
		};

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

		// Built in code at its design size: the scale gives it the zoom and the DPI.
		float scale = this.Services.UIService.Scale;
		vaultExportButton.Location = new Point((int)Math.Round(vaultExportButton.Left * scale), (int)Math.Round(vaultExportButton.Top * scale));
		vaultExportButton.Size = TQVaultAE.Application.Layout.WindowLayout.Scaled(vaultExportButton.Size, scale);
	}

	private void ExportVaultToFileClicked(object? sender, EventArgs e)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
		{
			this.Services.UIService.ShowWarning(Resources.GlobalNoVaultLoaded);
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
				this.Services.ItemExchangeService.ExportVaultToFile(vault, saveDialog.FileName);
				this.Services.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ExportVaultSuccessFile, saveDialog.FileName));
			}
			catch (Exception ex)
			{
				Log.LogError(ex, "Failed to export vault to file");
				this.Services.UIService.ShowError(Resources.ExportVaultFailFile);
			}
		}
	}

	private void ExportVaultToClipboardClicked(object? sender, EventArgs e)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
		{
			this.Services.UIService.ShowWarning(Resources.GlobalNoVaultLoaded);
			return;
		}

		try
		{
			var json = this.Services.ItemExchangeService.SerializePlayerCollection(vault);
			Clipboard.SetText(json);
			this.Services.UIService.NotifyUser(Resources.ExportVaultSuccessClipboard);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to export vault to clipboard");
			this.Services.UIService.ShowError(Resources.ExportVaultFailClipboard);
		}
	}

	private async void ExportVaultToPasteBinClicked(object? sender, EventArgs e)
	{
		var vault = this.vaultPanel.Player;
		if (vault is null)
		{
			this.Services.UIService.ShowWarning(Resources.GlobalNoVaultLoaded);
			return;
		}

		try
		{
			var json = this.Services.ItemExchangeService.SerializePlayerCollection(vault);
			var url = await this.Services.ItemExchangeService.ExportToPasteBinAsync(json, vault.PlayerName);
			Clipboard.SetText(url);
			this.Services.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ExportVaultSuccessPasteBin, url));
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to export vault to PasteBin");
			this.Services.UIService.ShowError(string.Format(CultureInfo.InvariantCulture, Resources.ExportVaultFailPasteBin, ex.Message));
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
			var result = this.Services.ItemExchangeService.ImportFromFile(fileName);
			this.HandleImportResult(result);
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to import from file");
			this.Services.UIService.NotifyUser(Resources.ImportFailFile);
		}
	}

	/// <summary>
	/// Shows the error of a failed import, or puts the imported items or vault in place.
	/// </summary>
	internal void HandleImportResult(ImportResult result)
	{
		if (!result.Success)
		{
			this.Services.UIService.NotifyUser(result.ErrorMessage ?? string.Empty);
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
			this.Services.UIService.ShowError(Resources.GlobalNoVaultImportTarget);
			return;
		}

		var sackPanel = this.vaultPanel.SackPanel;
		var sack = sackPanel.Sack;
		if (sack is null)
		{
			this.Services.UIService.ShowError(Resources.GlobalNoActiveTabImport);
			return;
		}

		var imported = this.Services.ItemExchangeService.PlaceImportedItems(sack, importResult, sackPanel.SackSize.Width, sackPanel.SackSize.Height
			, sackPanel.ContainerMap);

		// Into a vault linked to a mod: the imported items follow its link like dropped ones.
		this.vaultPanel.AdoptVaultMod(sack, imported);

		if (importResult.SetsTabIcon)
		{
			var button = this.vaultPanel.BagButtons[this.vaultPanel.CurrentBag];
			button.ApplyIconInfo(sack);
		}

		this.vaultPanel.Refresh();
		this.Services.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportSuccessCount, imported.Count, importResult.TotalCount));
		this.NotifyDroppedMods(importResult);
	}

	/// <summary>
	/// Names the mods an import dropped because they are not installed here.
	/// </summary>
	private void NotifyDroppedMods(ImportResult importResult)
	{
		if (importResult.DroppedMods.Count != 0)
			this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.ImportDroppedMods, string.Join(", ", importResult.DroppedMods)));
	}

	private void HandleVaultImportFromJson(ImportResult importResult)
	{
		var targetVault = this.vaultPanel.Player;
		if (targetVault is null)
		{
			this.Services.UIService.ShowError(Resources.GlobalNoVaultImportTarget);
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
		// Automation answers this dialog like a message box: Yes replaces, No creates a new vault, Cancel cancels.
		if (TQVaultAE.GUI.Automation.AutomationHost.TryTakeDialogAnswer(page.Heading, page.Text, MessageBoxButtons.YesNoCancel, out var automated))
			result = automated switch
			{
				DialogResult.Yes => replaceButton,
				DialogResult.No => createButton,
				_ => TaskDialogButton.Cancel,
			};
		else
			result = TaskDialog.ShowDialog(page);

		if (result == TaskDialogButton.Cancel)
			return;

		if (result == createButton)
		{
			var newName = this.Services.ItemExchangeService.ImportAsNewVault(importResult, DateTime.Now);

			this.GetVaultList(false);
			this.SelectVault(newName);

			this.Services.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportVaultSuccessCreate, newName));
			this.NotifyDroppedMods(importResult);
			return;
		}

		if (!this.Services.ItemExchangeService.IsVaultEmpty(targetVault))
		{
			var warning = this.Services.UIService.ShowWarning(
				Resources.ImportVaultWarning,
				null,
				ShowMessageButtons.OKCancel);

			if (!warning.IsOK)
				return;
		}

		this.Services.ItemExchangeService.ImportVaultInto(targetVault, importResult);

		// Into a vault linked to a mod: the imported items follow its link like dropped ones.
		foreach (var sack in targetVault)
			this.vaultPanel.AdoptVaultMod(sack, [.. sack]);

		this.vaultPanel.Refresh();

		for (int i = 0; i < targetVault.NumberOfSacks && i < this.vaultPanel.BagButtons.Count; i++)
		{
			if (importResult.SetsBagIcon(i) && targetVault.GetSack(i) is { } sack)
				this.vaultPanel.BagButtons[i].ApplyIconInfo(sack);
		}

		this.Services.UIService.NotifyUser(string.Format(CultureInfo.InvariantCulture, Resources.ImportVaultSuccess, importResult.TotalCount));
		this.NotifyDroppedMods(importResult);
	}

	#endregion
}
