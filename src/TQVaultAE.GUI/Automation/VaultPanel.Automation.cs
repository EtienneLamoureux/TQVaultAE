//-----------------------------------------------------------------------
// <copyright file="VaultPanel.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Components.Buttons;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Debug-only access to the bag button context menu for the automation endpoint.
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
		if (bag < 0 || bag >= this.BagButtons.Count)
			throw new InvalidOperationException($"Bag {bag} out of range 0..{this.BagButtons.Count - 1}.");

		this.contextMenu.Items.Clear();
		var button = this.BagButtons[bag];
		this.BagButtonClick(button, new MouseEventArgs(MouseButtons.Right, 1, button.Width / 2, button.Height / 2, 0));
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

	/// <summary>The sort button that sorts <paramref name="target"/>, as its click handler picks it.</summary>
	private AutoSortButton SortButtonOf(SackPanel target)
		=> this.AutoSortButtons.FirstOrDefault(b => ReferenceEquals(this.SortTarget(b), target))
			?? throw new InvalidOperationException("This panel has no sort button.");

	/// <summary>
	/// Right-clicks the sort button of <paramref name="target"/> through its regular handler, reads the menu, then
	/// closes it; with <paramref name="path"/>, clicks that entry after the calling tool has returned.
	/// </summary>
	internal (IReadOnlyList<AutomationMenuEntry> Entries, string? Clicked) AutomationSortMenu(SackPanel target, IReadOnlyList<string>? path)
	{
		var button = this.SortButtonOf(target);
		this.contextMenu.Items.Clear();
		this.AutoSortButtonClick(button, new MouseEventArgs(MouseButtons.Right, 1, button.Width / 2, button.Height / 2, 0));
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

	/// <summary>Left-clicks the sort button of <paramref name="target"/> after the calling tool has returned (a failed sort shows a message).</summary>
	internal void AutomationSortClickLater(SackPanel target)
	{
		var button = this.SortButtonOf(target);
		this.BeginInvoke(() => this.AutoSortButtonClick(button, new MouseEventArgs(MouseButtons.Left, 1, button.Width / 2, button.Height / 2, 0)));
	}

	/// <summary>The icon settings of each bag: display mode, label and the On/Off/Over pictures, null for the default.</summary>
	internal IReadOnlyList<object?> AutomationBagIcons
		=> this.BagButtons.Select(b => b.Sack?.BagButtonIconInfo is { } info
			? (object)new { mode = info.DisplayMode.ToString(), info.Label, on = info.On?.Raw, off = info.Off?.Raw, over = info.Over?.Raw }
			: null).ToList();
}
