//-----------------------------------------------------------------------
// <copyright file="VaultPanel.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Access to the bag button context menu for the automation endpoint.
/// </summary>
public partial class VaultPanel
{
	/// <summary>
	/// Right-clicks a bag button through its regular handler (which selects the bag and builds the menu), reads the
	/// menu, then closes it; with <paramref name="path"/>, clicks that entry after the calling tool has returned.
	/// </summary>
	/// <returns>The menu entries, and the entry clicked; no entries when the bag has no menu (not a vault bag).</returns>
	internal (IReadOnlyList<AutomationMenuEntry> Entries, string? Clicked) AutomationBagMenu(int bag, IReadOnlyList<string>? path)
	{
		this.RequireBag(bag);

		this.contextMenu.Items.Clear();
		var button = this.BagButtons[bag];
		this.BagButtonClick(button, AutomationInput.CenterClick(button, MouseButtons.Right));
		try
		{
			var entries = AutomationMenus.Describe(this.contextMenu.Items);
			string? clicked = path is { Count: > 0 } && entries.Count > 0
				? AutomationMenus.ClickLater(this, this.contextMenu, path)
				: null;
			return (entries, clicked);
		}
		finally
		{
			this.contextMenu.Close();
		}
	}

	/// <summary>
	/// Right-clicks a bag button like <see cref="AutomationBagMenu"/> and clicks its "Change icon" entry after the calling
	/// tool has returned (the bag button settings are modal).
	/// </summary>
	/// <returns>The text of the entry clicked.</returns>
	/// <exception cref="InvalidOperationException">The bag has no such entry: only the vault bags have it.</exception>
	internal string AutomationChangeIcon(int bag)
	{
		this.RequireBag(bag);

		this.contextMenu.Items.Clear();
		var button = this.BagButtons[bag];
		this.BagButtonClick(button, AutomationInput.CenterClick(button, MouseButtons.Right));
		var entry = this.contextMenu.Items.OfType<ToolStripItem>().FirstOrDefault(i => i.Tag is BagMenuAction.ChangeIcon);
		if (entry is null)
		{
			this.contextMenu.Close();
			throw new AutomationRefusedException("This bag has no 'Change icon' entry: only the vault bags' menu has it.");
		}

		return AutomationMenus.ClickLater(this, [entry]);
	}

	/// <summary>
	/// Right-clicks a bag button like <see cref="AutomationBagMenu"/> (which shows that bag), closes the menu, then runs
	/// its "Export tab > File" without the file dialog.
	/// </summary>
	/// <returns><see langword="false"/> when there was no tab to export.</returns>
	/// <exception cref="InvalidOperationException">The bag's menu has no such entry: only the vault bags holding items have it.</exception>
	internal bool AutomationExportTab(int bag, string path)
	{
		this.RequireBag(bag);

		this.contextMenu.Items.Clear();
		var button = this.BagButtons[bag];
		this.BagButtonClick(button, AutomationInput.CenterClick(button, MouseButtons.Right));
		var exportTab = this.contextMenu.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Text == Resources.PlayerPanelMenuExportTab);
		bool hasFile = exportTab?.DropDownItems.OfType<ToolStripItem>().Any(i => i.Text == Resources.PlayerPanelMenuExportTabFile && i.Enabled) == true;
		this.contextMenu.Close();
		if (!hasFile)
			throw new AutomationRefusedException("This bag's menu has no 'Export tab > File' entry: only the vault bags holding items have it.");

		return this.ExportTabToFile(path);
	}

	/// <summary>
	/// The sort button that sorts <paramref name="target"/>, as its click handler picks it; refused while it is hidden
	/// (the stash panel's equipment tab, a panel with no bag), as the user cannot click it then.
	/// </summary>
	private AutoSortButton SortButtonOf(SackPanel target)
	{
		var button = this.AutoSortButtons.FirstOrDefault(b => ReferenceEquals(this.SortTarget(b), target))
			?? throw new AutomationRefusedException("This panel has no sort button.");
		return button.Visible ? button : throw new AutomationRefusedException("The sort button of this panel is hidden.");
	}

	/// <summary>
	/// Right-clicks the sort button of <paramref name="target"/> through its regular handler, reads the menu, then
	/// closes it; with <paramref name="path"/>, clicks that entry after the calling tool has returned.
	/// </summary>
	internal (IReadOnlyList<AutomationMenuEntry> Entries, string? Clicked) AutomationSortMenu(SackPanel target, IReadOnlyList<string>? path)
	{
		var button = this.SortButtonOf(target);
		this.contextMenu.Items.Clear();
		this.AutoSortButtonClick(button, AutomationInput.CenterClick(button, MouseButtons.Right));
		try
		{
			var entries = AutomationMenus.Describe(this.contextMenu.Items);
			string? clicked = path is { Count: > 0 } ? AutomationMenus.ClickLater(this, this.contextMenu, path) : null;
			return (entries, clicked);
		}
		finally
		{
			this.contextMenu.Close();
		}
	}

	/// <summary>
	/// Right-clicks a bag button like the user and draws the menu it opens, with the sub menus opened along
	/// <paramref name="open"/>: the vault bag menu built by the click handler, or the stash tab's own menu (its tool tip
	/// entries). The menu is closed after, nothing picked.
	/// </summary>
	internal Bitmap AutomationRenderBagMenu(int bag, IReadOnlyList<string> open)
	{
		this.RequireBag(bag);

		var button = this.BagButtons[bag];
		var at = AutomationInput.Center(button);
		if (button.ContextMenuStrip is { } own)
		{
			own.Show(button, at);
			return AutomationMenus.Render(own, open);
		}

		this.contextMenu.Items.Clear();
		this.BagButtonClick(button, new MouseEventArgs(MouseButtons.Right, 1, at.X, at.Y, 0));
		if (!this.contextMenu.Visible)
			throw new InvalidOperationException("This bag button opens no menu: only the vault bags and the stash tabs have one.");

		return AutomationMenus.Render(this.contextMenu, open);
	}

	/// <summary>
	/// Right-clicks the sort button of <paramref name="target"/> through its regular handler and draws the menu it opens;
	/// the menu is closed after, nothing picked.
	/// </summary>
	internal Bitmap AutomationRenderSortMenu(SackPanel target)
	{
		var button = this.SortButtonOf(target);
		this.contextMenu.Items.Clear();
		this.AutoSortButtonClick(button, AutomationInput.CenterClick(button, MouseButtons.Right));
		return AutomationMenus.Render(this.contextMenu, []);
	}

	/// <summary>The key under which the sort button of <paramref name="target"/> keeps its own order, and the order a left click uses.</summary>
	internal (SortButtonKey Key, SackSortAlgorithm Algorithm) AutomationSortOrder(SackPanel target)
	{
		var button = this.SortButtonOf(target);
		return (this.SortKey(button), this.SortAlgorithmOf(button));
	}

	/// <summary>Left-clicks the sort button of <paramref name="target"/> after the calling tool has returned (a failed sort shows a message).</summary>
	internal void AutomationSortClickLater(SackPanel target)
	{
		var button = this.SortButtonOf(target);
		this.BeginInvoke(() => this.AutoSortButtonClick(button, AutomationInput.CenterClick(button, MouseButtons.Left)));
	}

	/// <summary>The icon settings of each bag: display mode, label and the On/Off/Over pictures, null for the default.</summary>
	internal IReadOnlyList<object?> AutomationBagIcons
		=> this.BagButtons.Select(b => b.Sack?.BagButtonIconInfo is { } info
			? (object)new { mode = info.DisplayMode.ToString(), info.Label, on = info.On?.Raw, off = info.Off?.Raw, over = info.Over?.Raw }
			: null).ToList();
}
