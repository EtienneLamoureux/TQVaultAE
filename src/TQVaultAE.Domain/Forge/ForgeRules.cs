using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Forge;

/// <summary>Rules set chosen in the Forge, from the most to the least restrictive.</summary>
public enum ForgeMode
{
	/// <summary>Like Strict, plus same base item and level requirement checks.</summary>
	Game,
	Strict,
	Relax,
	/// <summary>Anything goes, and any part of an item can be used.</summary>
	God,
}

/// <summary>Slots of the Forge.</summary>
public enum ForgeSlot
{
	Base,
	Prefix,
	Suffix,
	Relic1,
	Relic2,
}

/// <summary>Why a drop or a forge is refused, or what was cleared while accepting it.</summary>
public enum ForgeMessage
{
	ItemMustBeStoredFirst,
	MustSelectBaseItemFirst,
	MustSelectArmorOrWeaponOrJewellery,
	MustHavePrefix,
	MustHaveSuffix,
	MustSelectRelicOrItemWithRelic,
	NoEpicLegendary,
	PrefixSuffixOfAnotherBaseItem,
	PrefixSuffixHigherLevelRequirement,
	PrefixSuffixOfAnotherGearType,
	PrefixSuffixBetterThanBase,
	MustBeRelic,
	MustHaveRelic,
	RelicNotAllowed,
	MustHaveTinkeredSuffix,
	MustSelectBaseItem,
	MustSelectItemToMerge,
}

/// <summary>Items placed in the Forge slots.</summary>
public sealed class ForgeSelection
{
	private readonly Dictionary<ForgeSlot, Item?> _items = [];

	public Item? this[ForgeSlot slot]
	{
		get => this._items.GetValueOrDefault(slot);
		set => this._items[slot] = value;
	}

	public Item? Base => this[ForgeSlot.Base];
	public Item? Prefix => this[ForgeSlot.Prefix];
	public Item? Suffix => this[ForgeSlot.Suffix];
	public Item? Relic1 => this[ForgeSlot.Relic1];
	public Item? Relic2 => this[ForgeSlot.Relic2];

	/// <summary><c>true</c> when <paramref name="item"/> already sits in a slot.</summary>
	public bool Contains(Item item) => this._items.Values.Any(i => ReferenceEquals(i, item));

	public ForgeSelection Copy()
	{
		var copy = new ForgeSelection();
		foreach (var (slot, item) in this._items)
			copy[slot] = item;
		return copy;
	}
}

/// <summary>
/// Outcome of dropping an item on a slot.
/// </summary>
/// <param name="Accepted">The item goes into the slot.</param>
/// <param name="Rejection">Why it was refused, when not accepted.</param>
/// <param name="Warnings">Messages for the slots cleared to accept it.</param>
/// <param name="Cleared">Slots to empty, applied even when the drop is refused (as the Forge always did).</param>
public sealed record ForgeDecision(bool Accepted, ForgeMessage? Rejection, IReadOnlyList<ForgeMessage> Warnings, IReadOnlyCollection<ForgeSlot> Cleared);

/// <summary>
/// Forge rules: whether an item may go into a slot for a <see cref="ForgeMode"/>, and whether the selection can be forged.
/// </summary>
/// <remarks>
/// On the base slot, prefix/suffix mismatches do not refuse the new base item: they clear the prefix and suffix
/// instead, and the following checks see them cleared. On the prefix and suffix slots the same checks refuse the drop.
/// </remarks>
public sealed class ForgeRules
{
	private readonly Func<Item, int> _levelRequirement;

	/// <param name="levelRequirement">Level requirement of an item (Game mode checks).</param>
	public ForgeRules(Func<Item, int> levelRequirement)
		=> this._levelRequirement = levelRequirement;

	/// <summary>What <paramref name="mode"/> allows and requires: the checks <see cref="CheckDrop"/> applies.</summary>
	public static ForgeCapabilities Capabilities(ForgeMode mode)
	{
		bool strict = mode is ForgeMode.Strict or ForgeMode.Game;
		bool game = mode == ForgeMode.Game;
		bool god = mode == ForgeMode.God;

		return new(
			GearOnly: !god,
			EpicLegendaryAllowed: !strict,
			AffixFromSameBaseItem: game,
			AffixLevelNotAboveBase: game,
			AffixFromSameGearType: strict,
			AffixNotRarerThanBase: strict,
			AffixMustBePresent: !god,
			AnyPartOfMaterial: god,
			RelicSource: god ? ForgeRelicSource.Anything : strict ? ForgeRelicSource.RelicOrCharm : ForgeRelicSource.RelicOrSocketedGear,
			RelicMustFitBase: !god,
			SecondRelicNeedsTinkerer: strict,
			StricterModeExists: Enum.GetValues<ForgeMode>().Any(other => IsDowngrade(mode, other)));
	}

	/// <summary>Checks dropping <paramref name="dragged"/> on <paramref name="slot"/>.</summary>
	public ForgeDecision CheckDrop(ForgeMode mode, ForgeSlot slot, Item dragged, ForgeSelection current)
	{
		var state = current.Copy();
		var warnings = new List<ForgeMessage>();
		var cleared = new HashSet<ForgeSlot>();

		void Clear(params ForgeSlot[] slots)
		{
			foreach (var s in slots)
			{
				state[s] = null;
				cleared.Add(s);
			}
		}

		ForgeDecision Refuse(ForgeMessage reason) => new(false, reason, warnings, cleared);
		ForgeDecision Accept() => new(true, null, warnings, cleared);

		var can = Capabilities(mode);
		bool gear = dragged.IsWeaponShield || dragged.IsArmor || dragged.IsJewellery;

		switch (slot)
		{
			case ForgeSlot.Base:
			{
				var baseItem = state.Base;

				if (can.AffixFromSameBaseItem && baseItem is not null && dragged.BaseItemId != baseItem.BaseItemId)
					WarnAndClearAffixes(ForgeMessage.PrefixSuffixOfAnotherBaseItem);

				if (can.AffixLevelNotAboveBase && baseItem is not null && this._levelRequirement(dragged) > this._levelRequirement(baseItem))
					WarnAndClearAffixes(ForgeMessage.PrefixSuffixHigherLevelRequirement);

				if (!can.EpicLegendaryAllowed && dragged.Rarity > Rarity.Rare)
					return Refuse(ForgeMessage.NoEpicLegendary);

				if (can.AffixFromSameGearType && baseItem is not null && ClassOf(dragged) != ClassOf(baseItem))
					WarnAndClearAffixes(ForgeMessage.PrefixSuffixOfAnotherGearType);

				if (can.AffixNotRarerThanBase && baseItem is not null
					&& ((state.Prefix?.Rarity ?? 0) > dragged.Rarity || (state.Suffix?.Rarity ?? 0) > dragged.Rarity))
					WarnAndClearAffixes(ForgeMessage.PrefixSuffixBetterThanBase);

				// A second relic needs a "Tinkerer" suffix, from the new base item when there is no suffix item.
				if (can.SecondRelicNeedsTinkerer && state.Relic2 is not null && state.Suffix is not null
					&& !state.Suffix.AcceptExtraRelic && dragged.AcceptExtraRelic)
					Clear(ForgeSlot.Relic2);

				// Relics that the new base item cannot hold
				if (can.RelicMustFitBase)
				{
					if (RelicDoesNotFit(dragged, state.Relic1))
						Clear(ForgeSlot.Relic1);
					if (RelicDoesNotFit(dragged, state.Relic2))
						Clear(ForgeSlot.Relic2);
				}

				return gear || !can.GearOnly ? Accept() : Refuse(ForgeMessage.MustSelectArmorOrWeaponOrJewellery);
			}

			case ForgeSlot.Prefix:
			case ForgeSlot.Suffix:
			{
				var baseItem = state.Base;
				if (baseItem is null)
					return Refuse(ForgeMessage.MustSelectBaseItemFirst);

				if (can.AffixFromSameBaseItem && dragged.BaseItemId != baseItem.BaseItemId)
					return Refuse(ForgeMessage.PrefixSuffixOfAnotherBaseItem);

				if (can.AffixLevelNotAboveBase && this._levelRequirement(dragged) > this._levelRequirement(baseItem))
					return Refuse(ForgeMessage.PrefixSuffixHigherLevelRequirement);

				if (!can.EpicLegendaryAllowed && dragged.Rarity > Rarity.Rare)
					return Refuse(ForgeMessage.NoEpicLegendary);

				if (can.AffixFromSameGearType && ClassOf(dragged) != ClassOf(baseItem))
					return Refuse(ForgeMessage.PrefixSuffixOfAnotherGearType);

				if (can.AffixNotRarerThanBase && baseItem.Rarity < dragged.Rarity)
					return Refuse(ForgeMessage.PrefixSuffixBetterThanBase);

				if (slot == ForgeSlot.Prefix)
				{
					if (can.AffixMustBePresent && !dragged.HasPrefix)
						return Refuse(ForgeMessage.MustHavePrefix);
				}
				else
				{
					// A suffix that is not "of the Tinkerer" cannot keep a second relic.
					if (can.SecondRelicNeedsTinkerer && state.Relic2 is not null && !dragged.AcceptExtraRelic)
						Clear(ForgeSlot.Relic2);

					if (can.AffixMustBePresent && !dragged.HasSuffix)
						return Refuse(ForgeMessage.MustHaveSuffix);
				}

				return gear || !can.GearOnly ? Accept() : Refuse(ForgeMessage.MustSelectArmorOrWeaponOrJewellery);
			}

			case ForgeSlot.Relic1:
			case ForgeSlot.Relic2:
			{
				var baseItem = state.Base;
				if (baseItem is null)
					return Refuse(ForgeMessage.MustSelectBaseItemFirst);

				if (can.RelicSource == ForgeRelicSource.RelicOrCharm && !dragged.IsRelicOrCharm)
					return Refuse(ForgeMessage.MustBeRelic);

				if (can.RelicSource == ForgeRelicSource.RelicOrSocketedGear && !(dragged.IsRelicOrCharm || dragged.HasRelicOrCharmSlot1))
					return Refuse(ForgeMessage.MustHaveRelic);

				if (can.RelicMustFitBase && RelicDoesNotFit(baseItem, dragged))
					return Refuse(ForgeMessage.RelicNotAllowed);

				if (slot == ForgeSlot.Relic2 && can.SecondRelicNeedsTinkerer
					&& !((state.Suffix is null && baseItem.AcceptExtraRelic) || (state.Suffix?.AcceptExtraRelic ?? false)))
					return Refuse(ForgeMessage.MustHaveTinkeredSuffix);

				bool holdsRelic = (gear && dragged.HasRelicOrCharmSlot1) || dragged.IsRelicOrCharm;
				return can.RelicSource == ForgeRelicSource.Anything || holdsRelic ? Accept() : Refuse(ForgeMessage.MustSelectRelicOrItemWithRelic);
			}

			default:
				throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
		}

		void WarnAndClearAffixes(ForgeMessage message)
		{
			warnings.Add(message);
			Clear(ForgeSlot.Prefix, ForgeSlot.Suffix);
		}
	}

	/// <summary>Checks that <paramref name="selection"/> can be forged; <c>null</c> when it can.</summary>
	public ForgeMessage? CheckForge(ForgeSelection selection)
	{
		if (selection.Base is null)
			return ForgeMessage.MustSelectBaseItem;

		if (selection.Prefix is null && selection.Suffix is null && selection.Relic1 is null && selection.Relic2 is null)
			return ForgeMessage.MustSelectItemToMerge;

		return null;
	}

	/// <summary>
	/// <c>true</c> when going from <paramref name="previous"/> to <paramref name="next"/> makes the rules stricter: the
	/// items already placed may break the new rules, so the selection is reset.
	/// </summary>
	/// <param name="previous">The mode chosen before, or <c>null</c> when none was chosen yet.</param>
	/// <param name="next">The mode chosen now.</param>
	public static bool IsDowngrade(ForgeMode? previous, ForgeMode next)
		=> previous is { } from && next < from;

	private static string ClassOf(Item item)
		=> string.IsNullOrWhiteSpace(item.ItemClass) ? "NoClass" : item.ItemClass;

	/// <summary>
	/// <c>true</c> when <paramref name="relicSource"/> (a relic, or an item with a socketed relic) cannot go on <paramref name="target"/>.
	/// </summary>
	private static bool RelicDoesNotFit(Item target, Item? relicSource)
		=> relicSource is not null
			&& ((relicSource.IsRelicOrCharm && !target.IsRelicAllowed(relicSource))
				|| (relicSource.HasRelicOrCharmSlot1 && !target.IsRelicAllowed(relicSource.relicID, relicSource.RelicInfo)));
}
