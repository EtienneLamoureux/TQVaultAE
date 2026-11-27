using System.Globalization;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;
using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Insert into item: the item menu of a relic or charm starts the pair selection (<see cref="ItemPairSelection"/>) with
/// the game's rules or none, the click on another item sockets the relic into it through the forge's rules and merge
/// (<see cref="ForgeRules.PlanSocket"/>, <see cref="Application.Contracts.Services.IForgeService.Socket"/>). When two
/// slots take it, or the only one is taken, a menu at the click asks which slot (a replacement is never silent).
/// </summary>
public partial class SackPanel
{
	/// <summary>A slot entry of the Insert into item menu: what clicking it sockets where.</summary>
	/// <param name="Pair">The relic (first) and the item that receives it (second).</param>
	/// <param name="Mode">The rules.</param>
	/// <param name="Slot">The relic slot.</param>
	internal sealed record SocketSlotChoice(ItemPair Pair, ForgeMode Mode, ForgeSlot Slot);

	/// <summary>The forge mode of a socket purpose: the game's rules, or none (God mode).</summary>
	private static ForgeMode SocketMode(ItemPairPurpose purpose)
		=> purpose == ItemPairPurpose.SocketGameRules ? ForgeMode.Game : ForgeMode.God;

	/// <summary>Adds the Insert into item sub menu of a relic or charm: by the game's rules or by none.</summary>
	private void AddSocketMenuItems(Item relic)
	{
		ToolStripMenuItem Choice(string text, ItemPairPurpose purpose, string name)
			=> new ToolStripMenuItem(text, null, (_, _) => this.StartSocketSelection(relic, purpose), name).StyledLike(this.CustomContextMenu);

		var subMenu = new ToolStripMenuItem(Resources.SackPanelMenuSocket, null,
			Choice(Resources.SackPanelMenuSocketGameRules, ItemPairPurpose.SocketGameRules, nameof(ItemPairPurpose.SocketGameRules)),
			Choice(Resources.SackPanelMenuSocketNoRules, ItemPairPurpose.SocketNoRules, nameof(ItemPairPurpose.SocketNoRules)))
		{
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		}.StyledLike(this.CustomContextMenu);

		this.CustomContextMenu.Items.Add(subMenu);
	}

	/// <summary>Starts the Insert into item selection with <paramref name="relic"/>, from the item menu.</summary>
	private void StartSocketSelection(Item relic, ItemPairPurpose purpose)
	{
		this.DragInfo.PairSelection.Start(relic, purpose, this.ShownSack);
		this.UIService.NotifyUser(Resources.SackPanelSocketSelectionStarted, TQColor.Orange);
	}

	/// <summary>
	/// The item that receives the relic was clicked in this panel: refuses with the reason in the notification line,
	/// sockets right away into the only free slot that takes it, or asks which slot with a menu at
	/// <paramref name="location"/>.
	/// </summary>
	private void SocketInto(ItemPair pair, Point location)
	{
		if (this.ItemsReadOnly)
		{
			if (!this.NotifyIfReadOnlyCharacter(this.Sack))
				this.UIService.NotifyUser(Resources.SackPanelSocketReadOnly, TQColor.Red);
			return;
		}

		var mode = SocketMode(pair.Purpose);
		var plan = this.Services.ForgeService.CreateRules().PlanSocket(mode, pair.First, pair.Second);

		if (plan.Refusal is { } refusal)
			this.NotifySocketRefused(refusal);
		else if (plan.Direct is { } direct)
			this.Socket(new SocketSlotChoice(pair, mode, direct.Slot));
		else
			this.ShowSocketSlotMenu(pair, mode, plan, location);
	}

	/// <summary>
	/// Shows the slot choice in the panel's context menu: a title, then one entry per slot that takes the relic, naming
	/// what it replaces. Closing the menu without a choice changes nothing.
	/// </summary>
	private void ShowSocketSlotMenu(ItemPair pair, ForgeMode mode, SocketPlan plan, Point location)
	{
		var menu = this.CustomContextMenu;
		menu.Items.Clear();
		menu.Items.Add(new ToolStripMenuItem(Resources.SackPanelMenuSocket) { Enabled = false }.StyledLike(menu));
		menu.Items.Add("-");

		var names = plan.Options.Any(o => o.Replaces) ? this.Services.ItemText.GetFriendlyNames(pair.Second) : null;
		foreach (var option in plan.Options)
		{
			int number = option.Slot == ForgeSlot.Relic1 ? 1 : 2;
			string text = option.Replaces
				? string.Format(CultureInfo.CurrentCulture, Resources.SackPanelSocketSlotReplace, number, SocketedName(pair.Second, option.Slot, names!))
				: string.Format(CultureInfo.CurrentCulture, Resources.SackPanelSocketSlot, number);

			menu.Items.Add(new ToolStripMenuItem(text) { Name = option.Slot.ToString(), Tag = new SocketSlotChoice(pair, mode, option.Slot) }.StyledLike(menu));
		}

		menu.Show(this, location);
	}

	/// <summary>The name of what <paramref name="slot"/> of <paramref name="item"/> holds, for the replace entry.</summary>
	private static string SocketedName(Item item, ForgeSlot slot, Domain.Results.ToFriendlyNameResult names)
	{
		var (description, id) = slot == ForgeSlot.Relic1
			? (names.RelicInfo1Description, item.relicID)
			: (names.RelicInfo2Description, item.relic2ID);

		return string.IsNullOrWhiteSpace(description) ? Path.GetFileNameWithoutExtension(RecordId.RawOf(id)) : description;
	}

	/// <summary>
	/// Sockets the relic of <paramref name="choice"/> into the item, in this panel's sack: the relic leaves its sack, the
	/// item is marked modified, and the panels showing either are redrawn.
	/// </summary>
	private void Socket(SocketSlotChoice choice)
	{
		var (relic, target) = (choice.Pair.First, choice.Pair.Second);
		if (choice.Pair.FirstSack is not { } relicSack || !relicSack.Any(i => ReferenceEquals(i, relic)) || !this.ShownSack.Any(i => ReferenceEquals(i, target)))
		{
			this.UIService.NotifyUser(Resources.SackPanelSocketItemMoved, TQColor.Red);
			return;
		}

		var result = this.Services.ForgeService.Socket(
			this.Services.ForgeService.CreateRules(), choice.Mode, choice.Slot, relic, relicSack, target, this.ShownSack);

		if (result.Problem is { } problem)
		{
			this.NotifySocketRefused(problem);
			return;
		}

		this.InvalidateItemCacheAll(target, relic);
		BagButtonTooltip.InvalidateCache(this.ShownSack, relicSack);
		(this as EquipmentPanel)?.GetGearStatBonus();
		this.OnShownItemsChanged();

		// The relic may lie in another panel: redraw them all.
		this.FindForm()?.Invalidate(true);

		int number = choice.Slot == ForgeSlot.Relic1 ? 1 : 2;
		this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.SackPanelSocketDone
			, this.Services.ItemText.GetFriendlyNames(relic).FullNameClean, number), TQColor.Orange);
	}

	/// <summary>Tells why the forge's rules refuse the socketing, as the forge does.</summary>
	private void NotifySocketRefused(ForgeMessage message)
		=> this.UIService.NotifyUser(ForgeMessageText.Of(message, TranslationService.TranslateXTag("x3tagSuffix01")), TQColor.Red);
}
