using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

/// <summary>
/// Where the game lists an attribute group in an item tooltip.
/// </summary>
/// <param name="Store">The store of the game that shows it, or what follows the stores.</param>
/// <param name="Output">The output of the store: 1 for base stats, 2 and 3 for bonuses (damage outputs: 2 = % and damage over time,
/// 3 = flat damage and debuffs).</param>
/// <param name="Index">The rank of the attribute type in the store, from the game's load sequence.</param>
/// <param name="Global">A group of a global chance ("x% chance of one/all of"), listed together in the % output.</param>
public readonly record struct GameAttributeSlot(GameAttributeStore Store, int Output, int Index, bool Global = false)
{
	/// <summary>
	/// An attribute the game order does not know: listed after the others.
	/// </summary>
	public static GameAttributeSlot Other { get; } = new(GameAttributeStore.Other, 0, 0);
}

/// <summary>
/// The stores of the game's item attributes (AttributePak), then what the tooltip lists after them.
/// </summary>
public enum GameAttributeStore
{
	Offense,
	Retaliation,
	Defense,
	Character,
	Skill,
	Racial,
	SkillAugment,
	GrantedSkill,
	Pet,
	Other,
}

/// <summary>
/// The order in which the game (the game) lists the attributes of an item's base, prefix and suffix records:
/// it loads the three records together, walks each store's attribute types in a fixed order and shows, for each type,
/// the base line, then the prefix line, then the suffix line. The stores come in an order that depends on the item class.
/// </summary>
/// <remarks>
/// Read from the game (Anniversary Edition): the game, the CreateUIAttributeText of
/// Weapon, Armor and WeaponArmor, and the Load sequence of each attribute store. The game adds the three values of a
/// character or skill attribute into one line; here each record keeps its own line, only the order follows the game.
/// </remarks>
public static class GameAttributeOrder
{
	// Effect type codes: O offense, OM offense modifier, OS offense slow (over time), OSM offense slow modifier,
	// R/RM/RS/RSM the same for retaliation, D defense, C character.
	private static readonly string[] OffenseSequence =
	[
		"O:offensiveBasePhysical", "O:offensiveBaseFire", "O:offensiveBaseCold", "O:offensiveBaseLightning", "O:offensiveBasePoison", "O:offensiveBaseLife",
		"O:offensiveBonusPhysical", "O:offensivePhysical", "OM:offensivePhysicalModifier", "OS:offensiveSlowPhysical", "OSM:offensiveSlowPhysical",
		"O:offensivePierceRatio", "OM:offensivePierceRatioModifier", "O:offensivePierce", "OM:offensivePierceModifier",
		"OS:offensiveSlowBleeding", "OSM:offensiveSlowBleeding",
		"O:offensiveElemental", "OM:offensiveElementalModifier", "OM:offensiveTotalDamageModifier",
		"O:offensiveFire", "OS:offensiveSlowFire", "OM:offensiveFireModifier", "OSM:offensiveSlowFire",
		"O:offensiveCold", "OS:offensiveSlowCold", "OM:offensiveColdModifier", "OSM:offensiveSlowCold",
		"O:offensiveLightning", "OS:offensiveSlowLightning", "OM:offensiveLightningModifier", "OSM:offensiveSlowLightning",
		"O:offensivePoison", "OS:offensiveSlowPoison", "OM:offensivePoisonModifier", "OSM:offensiveSlowPoison",
		"O:offensivePercentCurrentLife", "O:offensiveLife", "OS:offensiveSlowLife", "OM:offensiveLifeModifier", "OSM:offensiveSlowLife",
		"O:offensiveLifeLeech", "OS:offensiveSlowLifeLeach", "OSM:offensiveSlowLifeLeach",
		"O:offensiveManaBurn", "OM:offensiveManaBurnRatioAdder", "OS:offensiveSlowManaLeach", "OSM:offensiveSlowManaLeach",
		"OS:offensiveSlowOffensiveAbility", "OSM:offensiveSlowOffensiveAbility", "OS:offensiveSlowDefensiveAbility", "OSM:offensiveSlowDefensiveAbility",
		"OS:offensiveSlowOffensiveReduction", "OSM:offensiveSlowOffensiveReduction", "OS:offensiveSlowDefensiveReduction", "OSM:offensiveSlowDefensiveReduction",
		"O:offensiveTotalDamageReductionPercent", "O:offensiveTotalDamageReductionAbsolute",
		"O:offensiveTotalResistanceReductionPercent", "O:offensiveTotalResistanceReductionAbsolute",
		"O:offensiveFumble", "O:offensiveProjectileFumble",
		"O:offensiveSleep", "OM:offensiveSleepModifier", "O:offensiveStun", "OM:offensiveStunModifier",
		"O:offensiveFreeze", "O:offensivePetrify", "O:offensiveConvert", "O:offensiveConfusion",
		"O:offensiveTaunt", "O:offensiveFear", "O:offensiveTrap",
		"OS:offensiveSlowTotalSpeed", "OSM:offensiveSlowTotalSpeed", "OS:offensiveSlowAttackSpeed", "OSM:offensiveSlowAttackSpeed",
		"OS:offensiveSlowRunSpeed", "OSM:offensiveSlowRunSpeed",
		"O:offensiveDisruption",
	];

	private static readonly string[] RetaliationSequence =
	[
		"R:retaliationPhysical", "RS:retaliationSlowPhysical", "RM:retaliationPhysicalModifier", "RSM:retaliationSlowPhysical",
		"R:retaliationPierceRatio", "RM:retaliationPierceRatioModifier", "R:retaliationPierce", "RM:retaliationPierceModifier",
		"RS:retaliationSlowBleeding", "RSM:retaliationSlowBleeding",
		"R:retaliationElemental", "RM:retaliationElementalModifier",
		"R:retaliationCold", "RS:retaliationSlowCold", "RM:retaliationColdModifier", "RSM:retaliationSlowCold",
		"R:retaliationPoison", "RS:retaliationSlowPoison", "RM:retaliationPoisonModifier", "RSM:retaliationSlowPoison",
		"R:retaliationLightning", "RS:retaliationSlowLightning", "RM:retaliationLightningModifier", "RSM:retaliationSlowLightning",
		"R:retaliationFire", "RS:retaliationSlowFire", "RM:retaliationFireModifier", "RSM:retaliationSlowFire",
		"R:retaliationPercentCurrentLife", "R:retaliationLife", "RS:retaliationSlowLife", "RM:retaliationLifeModifier", "RSM:retaliationSlowLife",
		"RS:retaliationSlowLifeLeach", "RSM:retaliationSlowLifeLeach", "RS:retaliationSlowManaLeach", "RSM:retaliationSlowManaLeach",
		"RS:retaliationSlowOffensiveAbility", "RSM:retaliationSlowOffensiveAbility", "RS:retaliationSlowDefensiveAbility", "RSM:retaliationSlowDefensiveAbility",
		"RS:retaliationSlowOffensiveReduction", "RSM:retaliationSlowOffensiveReduction",
		"R:retaliationStun", "RM:retaliationStunModifier",
		"RS:retaliationSlowTotalSpeed", "RSM:retaliationSlowTotalSpeed", "RS:retaliationSlowAttackSpeed", "RSM:retaliationSlowAttackSpeed",
		"RS:retaliationSlowRunSpeed", "RSM:retaliationSlowRunSpeed",
	];

	private static readonly string[] DefenseSequence =
	[
		// Base armor, then the shield block (where the game prints the block chance is not settled).
		"D:defensiveProtection", "D:defensiveAbsorption", "S:defensiveBlock", "S:blockAbsorption", "S:blockRecoveryTime",
		"D:defensiveProtectionModifier", "D:defensiveAbsorptionModifier",
		"D:defensivePhysical", "D:defensivePhysicalModifier", "D:defensivePhysicalDuration", "D:defensivePhysicalDurationModifier",
		"D:defensivePhysicalDurationChanceModifier",
		"D:defensivePierce", "D:defensivePierceModifier", "D:defensivePierceDuration", "D:defensivePierceDurationModifier",
		"D:defensiveBleeding", "D:defensiveBleedingDuration", "D:defensiveBleedingModifier", "D:defensiveBleedingDurationModifier",
		"D:defensiveElementalResistance", "D:defensiveElemental", "D:defensiveElementalModifier",
		"D:defensiveFire", "D:defensiveFireDuration", "D:defensiveFireModifier", "D:defensiveFireDurationModifier",
		"D:defensiveCold", "D:defensiveColdDuration", "D:defensiveColdModifier", "D:defensiveColdDurationModifier",
		"D:defensiveLightning", "D:defensiveLightningDuration", "D:defensiveLightningModifier", "D:defensiveLightningDurationModifier",
		"D:defensivePoison", "D:defensivePoisonDuration", "D:defensivePoisonModifier", "D:defensivePoisonDurationModifier",
		"D:defensiveLife", "D:defensiveLifeDuration", "D:defensiveLifeModifier", "D:defensiveLifeDurationModifier",
		"D:defensiveSlowLifeLeach", "D:defensiveSlowLifeLeachDuration", "D:defensiveSlowLifeLeachModifier", "D:defensiveSlowLifeLeachDurationModifier",
		"D:defensiveSlowManaLeach", "D:defensiveSlowManaLeachDuration", "D:defensiveSlowManaLeachModifier", "D:defensiveSlowManaLeachDurationModifier",
		"D:defensiveSleep", "D:defensiveSleepModifier", "D:defensiveStun", "D:defensiveStunModifier",
		"D:defensiveTrap", "D:defensiveFreeze", "D:defensivePetrify", "D:defensiveDisruption", "D:defensiveTaunt", "D:defensiveFear",
		"D:defensiveConfusion", "D:defensiveConvert",
		"D:defensiveBlockModifier", "D:defensiveReflect", "D:defensiveTotalSpeedResistance", "D:defensiveTotalSpeedChance",
	];

	private static readonly string[] CharacterSequence =
	[
		"C:characterBaseAttackSpeedTag",
		"C:characterStrength", "C:characterStrengthModifier", "C:characterIntelligence", "C:characterIntelligenceModifier",
		"C:characterDexterity", "C:characterDexterityModifier", "C:characterLife", "C:characterLifeModifier",
		"C:characterLifeRegen", "C:characterLifeRegenModifier", "C:characterMana", "C:characterManaModifier",
		"C:characterManaRegen", "C:characterManaRegenModifier",
		"C:characterManaLimitReserve", "C:characterManaLimitReserveModifier",
		"C:characterManaLimitReserveReduction", "C:characterManaLimitReserveReductionModifier",
		"C:characterOffensiveAbility", "C:characterOffensiveAbilityModifier", "C:characterDefensiveAbility", "C:characterDefensiveAbilityModifier",
		"C:characterTotalSpeedModifier", "C:characterAttackSpeed", "C:characterAttackSpeedModifier",
		"C:characterSpellCastSpeed", "C:characterSpellCastSpeedModifier", "C:characterRunSpeed", "C:characterRunSpeedModifier",
		"C:characterDefensiveBlockRecoveryReduction", "C:characterEnergyAbsorptionPercent", "C:characterDodgePercent",
		"C:characterDeflectProjectile", "C:characterDeflectProjectiles",
		"C:characterGlobalReqReduction",
		"C:characterWeaponStrengthReqReduction", "C:characterWeaponDexterityReqReduction", "C:characterWeaponIntelligenceReqReduction",
		"C:characterMeleeStrengthReqReduction", "C:characterMeleeDexterityReqReduction", "C:characterMeleeIntelligenceReqReduction",
		"C:characterArmorStrengthReqReduction", "C:characterArmorDexterityReqReduction", "C:characterArmorIntelligenceReqReduction",
		"C:characterJewelryStrengthReqReduction", "C:characterJewelryDexterityReqReduction", "C:characterJewelryIntelligenceReqReduction",
		"C:characterShieldStrengthReqReduction", "C:characterShieldDexterityReqReduction", "C:characterShieldIntelligenceReqReduction",
		"C:characterHuntingStrengthReqReduction", "C:characterHuntingDexterityReqReduction", "C:characterHuntingIntelligenceReqReduction",
		"C:characterStaffStrengthReqReduction", "C:characterStaffDexterityReqReduction", "C:characterStaffIntelligenceReqReduction",
		"C:characterLevelReqReduction",
		"C:characterIncreasedExperience",
	];

	private static readonly string[] SkillSequence =
	[
		"C:skillCooldownReduction", "C:skillManaCostReduction", "OM:skillProjectileSpeedModifier",
		"C:skillCooldownReductionModifier", "C:skillManaCostReductionModifier",
		"C:projectileLaunchNumber", "C:projectilePiercingChance", "C:projectileLaunchRotation", "C:skillLifeBonus", "OM:sparkMaxNumber",
	];

	private static readonly Dictionary<string, (GameAttributeStore Store, int Index)> Ranks = BuildRanks();

	/// <summary>
	/// Damage types whose over time effect is listed with the % bonuses (output 2); the other slows (speed, abilities,
	/// reductions) are debuffs, listed with the flat damage (output 3).
	/// </summary>
	private static readonly HashSet<string> DamageOverTimeEffects =
	[
		"offensiveSlowPhysical", "offensiveSlowBleeding", "offensiveSlowFire", "offensiveSlowCold", "offensiveSlowLightning",
		"offensiveSlowPoison", "offensiveSlowLife", "offensiveSlowLifeLeach", "offensiveSlowManaLeach",
	];

	private static Dictionary<string, (GameAttributeStore Store, int Index)> BuildRanks()
	{
		var ranks = new Dictionary<string, (GameAttributeStore, int)>(StringComparer.OrdinalIgnoreCase);
		void Add(GameAttributeStore store, string[] sequence)
		{
			for (int i = 0; i < sequence.Length; i++)
				ranks.TryAdd(sequence[i], (store, i));
		}

		Add(GameAttributeStore.Offense, OffenseSequence);
		Add(GameAttributeStore.Retaliation, RetaliationSequence);
		Add(GameAttributeStore.Defense, DefenseSequence);
		Add(GameAttributeStore.Character, CharacterSequence);
		Add(GameAttributeStore.Skill, SkillSequence);
		return ranks;
	}

	private static string? TypeCode(ItemAttributesEffectType type) => type switch
	{
		ItemAttributesEffectType.Offense => "O",
		ItemAttributesEffectType.OffenseModifier => "OM",
		ItemAttributesEffectType.OffenseSlow => "OS",
		ItemAttributesEffectType.OffenseSlowModifier => "OSM",
		ItemAttributesEffectType.Retaliation => "R",
		ItemAttributesEffectType.RetaliationModifier => "RM",
		ItemAttributesEffectType.RetaliationSlow => "RS",
		ItemAttributesEffectType.RetaliationSlowModifier => "RSM",
		ItemAttributesEffectType.Defense => "D",
		ItemAttributesEffectType.ShieldEffect => "S",
		ItemAttributesEffectType.Character => "C",
		_ => null,
	};

	/// <summary>
	/// Gets where the game lists an attribute group.
	/// </summary>
	/// <param name="variableName">The name of the first variable of the group.</param>
	/// <param name="data">The attribute data of that variable, or null when unknown.</param>
	/// <param name="isGlobal">Whether the group belongs to a global chance.</param>
	/// <param name="physicalIsBase">Whether flat physical damage and pierce ratio are base stats (weapons and shields).</param>
	public static GameAttributeSlot SlotOf(string variableName, ItemAttributesData? data, bool isGlobal, bool physicalIsBase)
	{
		if (variableName.Equals("offensiveGlobalChance", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.Offense, 2, -1, Global: true);
		if (variableName.Equals("retaliationGlobalChance", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.Retaliation, 2, -1, Global: true);
		if (variableName.Equals("itemSkillName", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.GrantedSkill, 0, 0);
		if (variableName.StartsWith("racialBonus", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.Racial, 0, 0);
		if (variableName.StartsWith("augmentSkill", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.SkillAugment, 0, 0);
		if (variableName.StartsWith("augmentMastery", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.SkillAugment, 0, 1);
		if (variableName.StartsWith("augmentAll", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.SkillAugment, 0, 2);
		if (variableName.StartsWith("petBonus", StringComparison.OrdinalIgnoreCase))
			return new(GameAttributeStore.Pet, 0, 0);

		// The base attack speed is an "other" attribute in the attribute tables.
		string? key = variableName.Equals("characterBaseAttackSpeedTag", StringComparison.OrdinalIgnoreCase)
			? "C:characterBaseAttackSpeedTag"
			: data is null || TypeCode(data.EffectType) is not { } code ? null : $"{code}:{data.Effect}";

		if (key is null || !Ranks.TryGetValue(key, out var rank))
			return GameAttributeSlot.Other;

		int output = rank.Store switch
		{
			GameAttributeStore.Offense => OffenseOutput(key, data!, physicalIsBase),
			GameAttributeStore.Retaliation => data!.EffectType is ItemAttributesEffectType.RetaliationModifier or ItemAttributesEffectType.RetaliationSlowModifier ? 2 : 3,
			GameAttributeStore.Defense => rank.Index <= 4 ? 1 : 2,
			GameAttributeStore.Character => rank.Index == 0 ? 1 : 2,
			_ => 1,
		};

		// The groups of a global chance are listed together, in the % output.
		if (isGlobal && rank.Store is GameAttributeStore.Offense or GameAttributeStore.Retaliation)
			return new(rank.Store, 2, rank.Index, Global: true);

		return new(rank.Store, output, rank.Index);
	}

	private static int OffenseOutput(string key, ItemAttributesData data, bool physicalIsBase)
	{
		if (data.Effect.StartsWith("offensiveBase", StringComparison.OrdinalIgnoreCase))
			return 1;

		// A weapon's (or shield's) physical damage and pierce ratio are its base stats.
		if (key.Equals("O:offensivePhysical", StringComparison.OrdinalIgnoreCase)
			|| key.Equals("O:offensivePierceRatio", StringComparison.OrdinalIgnoreCase))
			return physicalIsBase ? 1 : 3;

		return data.EffectType switch
		{
			ItemAttributesEffectType.OffenseModifier => key is "OM:offensiveStunModifier" or "OM:offensiveSleepModifier" or "OM:offensiveManaBurnRatioAdder" ? 3 : 2,
			ItemAttributesEffectType.OffenseSlow or ItemAttributesEffectType.OffenseSlowModifier => DamageOverTimeEffects.Contains(data.Effect) ? 2 : 3,
			_ => 3,
		};
	}

	/// <summary>
	/// The store outputs in the order the game shows them for an item class.
	/// </summary>
	private static (GameAttributeStore Store, int Output)[] Sections(GameItemClass itemClass) => itemClass switch
	{
		GameItemClass.Weapon =>
		[
			(GameAttributeStore.Offense, 1), (GameAttributeStore.Character, 1),
			(GameAttributeStore.Offense, 2), (GameAttributeStore.Offense, 3), (GameAttributeStore.Defense, 1), (GameAttributeStore.Defense, 2),
			(GameAttributeStore.Retaliation, 1), (GameAttributeStore.Retaliation, 2), (GameAttributeStore.Retaliation, 3),
			(GameAttributeStore.Character, 2), (GameAttributeStore.Skill, 1),
		],
		GameItemClass.Armor =>
		[
			(GameAttributeStore.Defense, 1),
			(GameAttributeStore.Defense, 2), (GameAttributeStore.Retaliation, 1), (GameAttributeStore.Retaliation, 2), (GameAttributeStore.Retaliation, 3),
			(GameAttributeStore.Offense, 1), (GameAttributeStore.Offense, 2), (GameAttributeStore.Offense, 3),
			(GameAttributeStore.Character, 1), (GameAttributeStore.Character, 2), (GameAttributeStore.Skill, 1),
		],
		GameItemClass.Shield =>
		[
			(GameAttributeStore.Offense, 1),
			(GameAttributeStore.Defense, 1), (GameAttributeStore.Defense, 2), (GameAttributeStore.Offense, 2), (GameAttributeStore.Offense, 3),
			(GameAttributeStore.Retaliation, 1), (GameAttributeStore.Retaliation, 2), (GameAttributeStore.Retaliation, 3),
			(GameAttributeStore.Character, 1), (GameAttributeStore.Character, 2), (GameAttributeStore.Skill, 1),
		],
		_ =>
		[
			(GameAttributeStore.Offense, 1), (GameAttributeStore.Offense, 2), (GameAttributeStore.Offense, 3),
			(GameAttributeStore.Defense, 1), (GameAttributeStore.Defense, 2),
			(GameAttributeStore.Retaliation, 1), (GameAttributeStore.Retaliation, 2), (GameAttributeStore.Retaliation, 3),
			(GameAttributeStore.Character, 1), (GameAttributeStore.Character, 2), (GameAttributeStore.Skill, 1),
		],
	};

	/// <summary>
	/// Gets the class whose store order applies to an item.
	/// </summary>
	public static GameItemClass ClassOf(Item item)
		=> item.IsWeapon ? GameItemClass.Weapon
		: item.IsShield ? GameItemClass.Shield
		: item.IsArmor || item.IsJewellery ? GameItemClass.Armor
		: GameItemClass.Other;

	/// <summary>
	/// Merges the attribute lines of the base, prefix and suffix records in the game's order.
	/// Each list gives its lines with the slot of the group that made them, in the order the record listed them.
	/// </summary>
	/// <param name="itemClass">The class of the item, which orders the stores.</param>
	/// <param name="baseLines">The lines of the base record.</param>
	/// <param name="prefixLines">The lines of the prefix record.</param>
	/// <param name="suffixLines">The lines of the suffix record.</param>
	public static string[] Merge(GameItemClass itemClass
		, IReadOnlyList<(string Text, GameAttributeSlot Slot)> baseLines
		, IReadOnlyList<(string Text, GameAttributeSlot Slot)> prefixLines
		, IReadOnlyList<(string Text, GameAttributeSlot Slot)> suffixLines)
	{
		static (string, GameAttributeSlot, string?)[] NoRange(IReadOnlyList<(string Text, GameAttributeSlot Slot)> lines)
			=> [.. lines.Select(l => (l.Text, l.Slot, (string?)null))];

		return [.. MergeWithRanges(itemClass, NoRange(baseLines), NoRange(prefixLines), NoRange(suffixLines)).Select(l => l.Text)];
	}

	/// <summary>
	/// Merges the attribute lines of the base, prefix and suffix records in the game's order, as <see cref="Merge"/>, each
	/// line keeping the value range shown on its right (null for none, and for the blank line after the base stats).
	/// </summary>
	public static (string Text, string? Range)[] MergeWithRanges(GameItemClass itemClass
		, IReadOnlyList<(string Text, GameAttributeSlot Slot, string? Range)> baseLines
		, IReadOnlyList<(string Text, GameAttributeSlot Slot, string? Range)> prefixLines
		, IReadOnlyList<(string Text, GameAttributeSlot Slot, string? Range)> suffixLines)
	{
		var sections = Sections(itemClass);
		int SectionRank(GameAttributeSlot slot)
		{
			if (slot.Store is GameAttributeStore.Offense or GameAttributeStore.Retaliation or GameAttributeStore.Defense
				or GameAttributeStore.Character or GameAttributeStore.Skill)
			{
				int i = Array.IndexOf(sections, (slot.Store, slot.Store == GameAttributeStore.Skill ? 1 : slot.Output));
				return i < 0 ? sections.Length : i;
			}

			// After the stores: racial bonuses, skill augments, granted skill, pet bonuses, then unknown attributes.
			return sections.Length + 1 + (slot.Store - GameAttributeStore.Racial);
		}

		var entries = new List<(int Section, int Global, int GlobalSource, int Index, int Source, int Line, string Text, string? Range)>();
		void AddAll(IReadOnlyList<(string Text, GameAttributeSlot Slot, string? Range)> lines, int source)
		{
			for (int line = 0; line < lines.Count; line++)
			{
				var (text, slot, range) = lines[line];

				// The base attack speed of a non-weapon has no text.
				if (text.Length == 0 && slot.Store == GameAttributeStore.Character && slot.Output == 1)
					continue;

				// The game lists the racial bonuses of the prefix, then the suffix, then the base.
				int rank = slot.Store == GameAttributeStore.Racial ? (source + 2) % 3 : source;

				// A global chance keeps its groups together under its header line, record by record.
				entries.Add(slot.Global
					? (SectionRank(slot), 1, rank, 0, rank, line, text, range)
					: (SectionRank(slot), 0, 0, slot.Index, rank, line, text, range));
			}
		}

		AddAll(baseLines, 0);
		AddAll(prefixLines, 1);
		AddAll(suffixLines, 2);

		var ordered = entries
			.OrderBy(e => e.Section).ThenBy(e => e.Global).ThenBy(e => e.GlobalSource).ThenBy(e => e.Index)
			.ThenBy(e => e.Source).ThenBy(e => e.Line)
			.Select(e => (e.Text, e.Range, e.Section, e.Source))
			.ToList();

		// The base record's stats (damage, speed, armor, block) come first; a blank line separates them from the
		// affixes and bonuses, including an affix's own armor or damage listed next to them.
		int baseCount = ordered.TakeWhile(e => e.Source == 0 && IsBaseSection(sections, e.Section)).Count();
		var result = ordered.Select(e => (e.Text, e.Range)).ToList();
		if (baseCount > 0 && baseCount < result.Count && result[baseCount].Text.Length != 0)
			result.Insert(baseCount, (string.Empty, null));

		return [.. result];
	}

	/// <summary>
	/// Whether a section holds base stats: the first output of the offense, defense and character stores.
	/// </summary>
	private static bool IsBaseSection((GameAttributeStore Store, int Output)[] sections, int section)
		=> section < sections.Length
		&& sections[section] is { Output: 1, Store: GameAttributeStore.Offense or GameAttributeStore.Defense or GameAttributeStore.Character };
}

/// <summary>
/// The item classes whose attribute stores the game shows in a different order.
/// </summary>
public enum GameItemClass
{
	/// <summary>Relics, charms, artifacts and the rest: the plain AttributePak order.</summary>
	Other,

	Weapon,

	/// <summary>Armor and jewellery.</summary>
	Armor,

	Shield,
}
