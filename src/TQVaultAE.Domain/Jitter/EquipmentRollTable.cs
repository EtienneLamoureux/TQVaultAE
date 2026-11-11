//-----------------------------------------------------------------------
// <copyright file="EquipmentRollTable.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.Domain.Jitter;

/// <summary>
/// Equipment attribute stores, in the order the game loads them when it creates an item.
/// </summary>
public enum RollStore
{
	/// <summary>Character attributes: affixes always roll, uncapped.</summary>
	Character,

	/// <summary>Skill attributes: same rules as <see cref="Character"/>.</summary>
	Skill,

	/// <summary>Damage attributes: every instance rolls, capped.</summary>
	Damage,

	/// <summary>Retaliation attributes: same rules as <see cref="Damage"/>.</summary>
	Retaliation,

	/// <summary>Defense attributes: same rules as <see cref="Damage"/>.</summary>
	Defense,
}

/// <summary>
/// Which record instances of an attribute consume RNG draws.
/// </summary>
public enum RollScope
{
	/// <summary>Base item, prefix and suffix instances all roll (the normal case).</summary>
	All,

	/// <summary>Only prefix/suffix instances roll, at the attribute's base percent, and only on armor items
	/// (base armor protection and absorption: the base armor is static).</summary>
	AffixOnlyArmor,

	/// <summary>Nothing rolls for this attribute (e.g. base damage, level requirement reduction).</summary>
	None,
}

/// <summary>
/// Which items load an attribute: armor items or the others.
/// </summary>
public enum RollItemFilter
{
	/// <summary>Every equipment item.</summary>
	Any,

	/// <summary>Only armor items (helms, torsos, arms, legs).</summary>
	ArmorOnly,

	/// <summary>Every equipment item except armor (weapons, shields, jewellery).</summary>
	NonArmorOnly,
}

/// <summary>
/// One value rolled by an attribute's jitter, in the game's order.
/// </summary>
/// <param name="Variable">Database variable name (case-insensitive).</param>
/// <param name="DefaultFrom">When the variable is absent or zero, the loader copies this variable's value first.</param>
/// <param name="RoundToInt">The game truncates the value to an integer before and after the roll (base physical damage range).</param>
/// <param name="RangeOf">
/// Damage pairs are stored as (min, range): this field is the max variable, but the game rolls
/// <c>max(0, max - min)</c> with <paramref name="RangeOf"/> the min variable, and the in-game max is
/// <c>rolled min + rolled range</c>. No draw when max is absent or not above min.
/// </param>
/// <param name="Scales">
/// <see langword="false"/> when the game's scaling leaves this value alone
/// (e.g. the pierce ratio of base physical damage). For <paramref name="RangeOf"/> fields only the min is
/// scaled and the range is kept, so the max moves by the min's scaling.
/// </param>
public sealed record RollField(string Variable, string? DefaultFrom = null, bool RoundToInt = false, string? RangeOf = null, bool Scales = true);

/// <summary>
/// One attribute of a store, in the order the game loads them.
/// </summary>
/// <param name="Store">Owning store.</param>
/// <param name="Order">1-based position inside its store.</param>
/// <param name="BaseJitterPercent">Percent the base instance rolls at (0 = base never rolls).</param>
/// <param name="Scope">Which instances roll.</param>
/// <param name="Fields">Values rolled per instance, in order.</param>
/// <param name="Requires">Variables that must all be non-zero in a record for its instance to exist
/// (otherwise the game skips the instance and nothing rolls), e.g. the min of a damage pair.</param>
/// <param name="Items">Which items load the attribute at all.</param>
/// <param name="Scales"><see langword="false"/> when the game does not scale the attribute (the speeds, level
/// requirement reduction, base attack speed): the base item's <c>attributeScalePercent</c> does not apply.</param>
public sealed record RollAttribute(RollStore Store, int Order, float BaseJitterPercent, RollScope Scope, RollField[] Fields, string[]? Requires = null, RollItemFilter Items = RollItemFilter.Any, bool Scales = true);

/// <summary>
/// Roll order of every equipment attribute, as the game rolls an item's stats from its seed.
/// The data lives in <c>EquipmentRollTable.Data.cs</c>.
/// </summary>
public static partial class EquipmentRollTable
{
	/// <summary>
	/// All attributes, ordered by <see cref="RollStore"/> then <see cref="RollAttribute.Order"/>.
	/// </summary>
	public static IReadOnlyList<RollAttribute> Attributes { get; } = BuildOrdered();

	private static RollAttribute[] BuildOrdered()
		=> CreateAttributes().OrderBy(a => a.Store).ThenBy(a => a.Order).ToArray();

	private static partial RollAttribute[] CreateAttributes();
}
