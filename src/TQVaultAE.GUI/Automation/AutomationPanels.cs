//-----------------------------------------------------------------------
// <copyright file="AutomationPanels.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using ModelContextProtocol;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Stable names for the item grids of the main form.
/// </summary>
internal static class AutomationPanels
{
	/// <summary>
	/// A named item grid and the panel that hosts its bag buttons.
	/// </summary>
	public sealed record NamedPanel(string Id, SackPanel Panel, VaultPanel? Container);

	public const string Vault = "vault";
	public const string SecondaryVault = "secondaryVault";
	public const string Player = "player";
	public const string PlayerBag = "playerBag";
	public const string Stash = "stash";
	public const string Equipment = "equipment";

	/// <summary>
	/// Lists the item grids that currently exist. Must run on the UI thread.
	/// </summary>
	public static List<NamedPanel> List(MainForm form)
	{
		var result = new List<NamedPanel>();

		void Add(string id, SackPanel? panel, VaultPanel? container)
		{
			if (panel is not null)
				result.Add(new NamedPanel(id, panel, container));
		}

		var vault = form.AutomationVaultPanel;
		Add(Vault, vault?.BagSackPanel, vault);

		var secondary = form.AutomationSecondaryVaultPanel;
		Add(SecondaryVault, secondary?.BagSackPanel, secondary);

		var player = form.AutomationPlayerPanel;
		if (player is not null)
		{
			// PlayerPanel.SackPanel is the main inventory; BagSackPanel holds the extra bags.
			if (!ReferenceEquals(player.SackPanel, player.BagSackPanel))
				Add(Player, player.SackPanel, player);
			Add(PlayerBag, player.BagSackPanel, player);
		}

		var stash = form.AutomationStashPanel;
		if (stash is not null)
		{
			Add(Stash, stash.BagSackPanel, stash);
			Add(Equipment, stash.EquipmentPanel, stash);
		}

		return result;
	}

	/// <summary>
	/// Finds an item grid by id. Must run on the UI thread.
	/// </summary>
	public static NamedPanel Resolve(MainForm form, string id)
	{
		var panels = List(form);
		return panels.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase))
			?? throw new McpException($"Unknown panel '{id}'. Known panels: {string.Join(", ", panels.Select(p => p.Id))}");
	}

	/// <summary>
	/// Gets the id of a panel instance, or null when it is not one of the named grids.
	/// </summary>
	public static string? IdOf(MainForm form, SackPanel? panel)
		=> panel is null ? null : List(form).FirstOrDefault(p => ReferenceEquals(p.Panel, panel))?.Id;

	/// <summary>
	/// Gets a value indicating whether the control and all its parents are visible.
	/// </summary>
	public static bool IsShown(Control control) => control.Visible && control.FindForm() is not null;

	/// <summary>
	/// Gets the bag a panel shows. <see cref="StashPanel"/> hides <see cref="VaultPanel.CurrentBag"/> with its own
	/// (<c>new</c>, not an override), so reading it through a <see cref="VaultPanel"/> reference always gave 0.
	/// </summary>
	public static int ShownBag(this VaultPanel container)
		=> container is StashPanel stash ? stash.CurrentBag : container.CurrentBag;
}
