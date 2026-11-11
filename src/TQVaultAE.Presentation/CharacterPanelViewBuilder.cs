using System.Drawing;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation;

/// <summary>
/// The state of a character the statistics panel is built from: the save, the attributes as spent, the skills as
/// learned, and the options. The character editor gives its editors' values; the main window gives the save's.
/// </summary>
public sealed record CharacterPanelInput
{
	public required PlayerCollection Player { get; init; }
	public required PlayerInfo Info { get; init; }

	/// <summary>The base attributes (no bonus): Health, Energy, Strength, Intelligence, Dexterity.</summary>
	public required IReadOnlyList<int> Attributes { get; init; }

	/// <summary>The attribute points left.</summary>
	public int AttributePoints { get; init; }
	public int Level { get; init; }
	public long Xp { get; init; }

	/// <summary>The skills with their points.</summary>
	public required IReadOnlyList<SkillRecord> Skills { get; init; }

	/// <summary>The masteries of the character.</summary>
	public IReadOnlyList<MasteryTree> Trees { get; init; } = [];

	/// <summary>The skill bonuses of the equipped items.</summary>
	public IReadOnlyList<SkillAugment> Augments { get; init; } = [];

	/// <summary>The difficulty the statistics are computed for, 0 (Normal) to 2 (Legendary).</summary>
	public int Difficulty { get; init; }

	/// <summary>Whether the auras and buffs count.</summary>
	public bool IncludeBuffs { get; init; }

	/// <summary>True when the masteries changed: the class no longer applies.</summary>
	public bool ClassCleared { get; init; }

	/// <summary>True when the attributes can be changed in the panel: "+" buttons while points are left, and their tool tip.</summary>
	public bool Editable { get; init; }

	/// <summary>True when attributes changed since the start: the Undo button shows.</summary>
	public bool CanUndo { get; init; }
}

/// <summary>
/// Builds what <see cref="CharacterStatsPanel"/> shows of a character: the statistics computed like the game
/// (<see cref="CharacterStatsCalculator"/>: defense on tab I, tab II, the damage and race bonuses of tab III) from the
/// equipped items, the skills, the attributes, the difficulty and the buffs option, with the game's texts. The sums of
/// the equipped items and of the skills are kept until they change.
/// </summary>
public sealed class CharacterPanelViewBuilder(ICharacterStatsService service, ITranslationService translation)
{
	private readonly ICharacterStatsService _service = service;
	private readonly ITranslationService _translation = translation;

	private string? _gearKey;
	private (string Key, AttributeSum Sum)? _skillSum;

	/// <summary>The sum of the equipped items, once computed.</summary>
	public AttributeSum? GearSum { get; private set; }

	/// <summary>The sum of the skills, once computed.</summary>
	public AttributeSum? SkillSum => this._skillSum?.Sum;

	/// <summary>The weapons in hand and the other items for the attack damage, once computed.</summary>
	public OffenseGear? OffenseGear { get; private set; }

	/// <summary>The stance in hand, once computed.</summary>
	public WeaponStance? Stance { get; private set; }

	/// <summary>Resistance icons and the tags of their names, in the order of <see cref="StatResistance"/>.</summary>
	private static readonly (string Icon, string Tag, string Fallback)[] ResistanceIcons =
	[
		("""InGameUI\CharacterScreen\Resistances\FireIcon01.tex""", "tagCResistance01", "Fire Resistance"),
		("""InGameUI\CharacterScreen\Resistances\IceIcon01.tex""", "tagCResistance02", "Cold Resistance"),
		("""InGameUI\CharacterScreen\Resistances\LightningIcon01.tex""", "tagCResistance04", "Lightning Resistance"),
		("""InGameUI\CharacterScreen\Resistances\PiercingIcon01.tex""", "tagCResistance05", "Pierce Resistance"),
		("""InGameUI\CharacterScreen\Resistances\LifeIcon01.tex""", "tagCResistance07", "Vitality Resistance"),
		("""InGameUI\CharacterScreen\Resistances\PoisonIcon01.tex""", "tagCResistance03", "Poison Resistance"),
		("""InGameUI\CharacterScreen\Resistances\BleedingIcon01.tex""", "tagCResistance06", "Bleeding Resistance"),
		("""XPack2\ingameui\characterscreen\resistances\physicalicon01.tex""", "x2tagCResistance11", "Physical Resistance"),
		("""XPack2\ingameui\characterscreen\resistances\slowdownicon01.tex""", "x2tagCResistance16", "Slowdown Resistance"),
		("""XPack2\ingameui\characterscreen\resistances\trapicon01.tex""", "x2tagCResistance14", "Trap Resistance"),
		("""InGameUI\CharacterScreen\Resistances\SpiritIcon01.tex""", "tagCResistance08", "Energy Resistance"),
		("""InGameUI\CharacterScreen\Resistances\DisruptionIcon01.tex""", "tagCResistance10", "Disruption Resistance"),
		("""InGameUI\CharacterScreen\Resistances\StunIcon01.tex""", "tagCResistance09", "Stun Resistance"),
		("""XPack2\ingameui\characterscreen\resistances\freezeicon01.tex""", "x2tagCResistance15", "Freeze Resistance"),
		("""XPack2\ingameui\characterscreen\resistances\sleepicon01.tex""", "x2tagCResistance12", "Sleep Resistance"),
		("""XPack2\ingameui\characterscreen\resistances\pertificationicon01.tex""", "x2tagCResistance13", "Petrification Resistance"),
	];

	/// <summary>Damage icons of tab III, the tag of their name, and the tags of their damage % and damage over time % lines.</summary>
	private static readonly (StatDamage Damage, string Icon, string Tag, string? DamageTag, string? DurationDamageTag)[] DamageIcons =
	[
		// The game's physical rollover has no damage over time block.
		(StatDamage.Physical, """XPack2\ingameui\characterscreen\resistances\physicalicon01.tex""", "DamagePhysical", "DamageModifierPhysical", null),
		(StatDamage.Pierce, """InGameUI\CharacterScreen\Resistances\PiercingIcon01.tex""", "DamagePierce", "DamageModifierPierce", null),
		(StatDamage.Bleeding, """InGameUI\CharacterScreen\Resistances\BleedingIcon01.tex""", "DamageDurationBleeding", null, "DamageDurationModifierBleeding"),
		(StatDamage.Poison, """InGameUI\CharacterScreen\Resistances\PoisonIcon01.tex""", "DamageDurationPoison", null, "DamageDurationModifierPoison"),
		(StatDamage.Pet, """InGameUI\CharacterScreen\petdamageicon_01.tex""", "x4tagCPetStats", null, null),
		(StatDamage.Fire, """InGameUI\CharacterScreen\Resistances\FireIcon01.tex""", "DamageFire", "DamageModifierFire", "DamageDurationModifierFire"),
		(StatDamage.Cold, """InGameUI\CharacterScreen\Resistances\IceIcon01.tex""", "DamageCold", "DamageModifierCold", "DamageDurationModifierCold"),
		(StatDamage.Lightning, """InGameUI\CharacterScreen\Resistances\LightningIcon01.tex""", "DamageLightning", "DamageModifierLightning", "DamageDurationModifierLightning"),
		(StatDamage.Vitality, """InGameUI\CharacterScreen\Resistances\LifeIcon01.tex""", "DamageLife", "DamageModifierLife", "DamageDurationModifierLife"),
		(StatDamage.LifeLeech, """InGameUI\characterscreen\lifeleachicon01.tex""", "DamageDurationLifeLeach", null, "DamageDurationModifierLifeLeach"),
	];

	/// <summary>
	/// The equations of a damage's direct and over time parts (<c>combatequations.dbr</c>) and their damage variable, for
	/// the "Global Multiplier" lines; vitality uses the elemental ones.
	/// </summary>
	private static readonly Dictionary<StatDamage, ((string Equation, string Variable)? Direct, (string Equation, string Variable)? OverTime)> DamageEquations = new()
	{
		[StatDamage.Physical] = (("physicalDamageEquation", "physicalDamageDV"), ("physicalDurationDamageEquation", "physicalDamageDV")),
		[StatDamage.Pierce] = (("pierceDamageEquation", "pierceDamageDV"), null),
		[StatDamage.Bleeding] = (null, ("bleedingDurationDamageEquation", "bleedingDamageDV")),
		[StatDamage.Poison] = (null, ("poisonDurationDamageEquation", "poisonDamageDV")),
		[StatDamage.Fire] = (("elementalDamageEquation", "elementalDamageDV"), ("elementalDurationDamageEquation", "elementalDamageDV")),
		[StatDamage.Cold] = (("elementalDamageEquation", "elementalDamageDV"), ("elementalDurationDamageEquation", "elementalDamageDV")),
		[StatDamage.Lightning] = (("elementalDamageEquation", "elementalDamageDV"), ("elementalDurationDamageEquation", "elementalDamageDV")),
		[StatDamage.Vitality] = (("elementalDamageEquation", "elementalDamageDV"), ("elementalDurationDamageEquation", "elementalDamageDV")),
		[StatDamage.LifeLeech] = (null, ("lifeLeachDurationDamageEquation", "lifeLeachDamageDV")),
		[StatDamage.ManaLeech] = (null, ("manaLeachDurationDamageEquation", "manaLeachDamageDV")),
	};

	/// <summary>Race icons of tab III and the tags of the race names (plural), in the order of <see cref="StatRace"/>.</summary>
	private static readonly (string Icon, string Tag, string Fallback)[] RaceIcons =
	[
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_beastmen_icon.tex""", "tagRace04P", "Beastmen"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_insects_icon.tex""", "tagRace05P", "Insectoids"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_undead_icon.tex""", "tagRace06P", "Undead"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_demons_icon.tex""", "tagRace07P", "Demons"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_humanity_icon.tex""", "x4tagRaceHumankind", "Humankind"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_devices_icon.tex""", "tagRace12P", "Devices"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_beasts_icon.tex""", "tagRace03P", "Beasts"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_plants_icon.tex""", "tagRace01P", "Plants"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_ghosts_icon.tex""", string.Empty, "Ghosts"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_magical_icon.tex""", "tagRace11P", "Magical"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_giants_icon.tex""", string.Empty, "Giants"),
		("""XPack4\ingameui\characterscreen\monsterracebonuses\x4_constructs_icon.tex""", "tagRace10P", "Constructs"),
	];


	private static IEnumerable<Item> Equipment(CharacterPanelInput input) => input.Player.EquipmentSack ?? Enumerable.Empty<Item>();

	/// <summary>Forgets the sums of the equipped items when the equipment or the weapon set in hand changed.</summary>
	private void TrackGear(CharacterPanelInput input)
	{
		var key = (input.Player.UseAlternate ? "+" : "-") + string.Join(";", Equipment(input).Select(i =>
			string.Join(",", i.BaseItemId?.Raw, i.prefixID?.Raw, i.suffixID?.Raw, i.relicID?.Raw, i.RelicBonusId?.Raw, i.relic2ID?.Raw, i.RelicBonus2Id?.Raw,
				i.Seed.ToString(CultureInfo.InvariantCulture), i.Var1.ToString(CultureInfo.InvariantCulture), i.Var2.ToString(CultureInfo.InvariantCulture), i.Mod)));
		if (key == this._gearKey)
			return;

		this._gearKey = key;
		this.GearSum = null;
		this.OffenseGear = null;
		this.Stance = null;
	}

	/// <summary>The statistics of the character.</summary>
	public CharacterStats ComputeStats(CharacterPanelInput input)
	{
		this.TrackGear(input);
		this.GearSum ??= this._service.GetGearAttributes(Equipment(input), input.Player.UseAlternate);

		// The sum of the skills is kept until a point, an item bonus or the buffs option changes.
		var key = (input.IncludeBuffs ? "+" : "-") + this._gearKey + "|"
			+ string.Join(";", input.Skills.Select(r => r.skillName + "=" + r.skillLevel.ToString(CultureInfo.InvariantCulture)));
		if (this._skillSum?.Key != key)
			this._skillSum = (key, this._service.GetSkillAttributes(input.Skills, input.Augments, input.Trees, input.IncludeBuffs));

		return CharacterStatsCalculator.Compute(new CharacterStatsInput(
			input.Attributes[2], input.Attributes[4], input.Attributes[3], this.GearSum, this._skillSum.Value.Sum, Math.Clamp(input.Difficulty, 0, CharacterPanelLayout.Legendary), this._service.GetGameBalance()));
	}

	/// <summary>
	/// Tab I's offense: attack speed from the stance in hand and its animations, the average damage and DPS (after
	/// <see cref="ComputeStats"/>, which keeps the sums up to date).
	/// </summary>
	public (double AttackSpeed, OffenseValues Values) ComputeOffense(CharacterPanelInput input)
	{
		this.TrackGear(input);
		var equipment = Equipment(input);
		bool useAlternate = input.Player.UseAlternate;
		this.OffenseGear ??= this._service.GetOffenseGear(equipment, useAlternate);
		this.Stance ??= this._service.GetWeaponStance(equipment, useAlternate);

		var balance = this._service.GetGameBalance();
		var gear = this.GearSum ?? new AttributeSum();
		var skills = this._skillSum?.Sum ?? new AttributeSum();
		var all = new AttributeSum();
		all.Add(gear);
		all.Add(skills);

		var durations = this._service.GetAttackAnimationDurations(input.Info.IsFemale);
		var attackSpeed = CharacterOffenseCalculator.AttackSpeed(all, this.Stance.Value, balance, Math.Clamp(input.Difficulty, 0, CharacterPanelLayout.Legendary),
			durations.GetValueOrDefault(WeaponStance.Unarmed), durations.GetValueOrDefault(this.Stance.Value));

		double Total(int attribute, string field) => CharacterStatsCalculator.Total(input.Attributes[attribute], all, field);
		var values = CharacterOffenseCalculator.Damage(new OffenseInput(this.OffenseGear, gear, skills,
			Total(2, "characterStrength"), Total(4, "characterDexterity"), Total(3, "characterIntelligence"),
			input.Level, attackSpeed, balance));
		return (attackSpeed, values);
	}

	/// <summary>What the statistics panel shows of the character.</summary>
	public CharacterPanelView Build(CharacterPanelInput input)
	{
		var info = input.Info;
		var culture = CultureInfo.CurrentCulture;
		string Game(string tag, string fallback) => this._translation.TranslateOr(tag, fallback);

		// The bonuses as the statistics sum them, unrounded: the game adds 13.5 strength of a mastery as is.
		this.ComputeStats(input);
		var bonuses = new AttributeSum();
		bonuses.Add(this.GearSum ?? new AttributeSum());
		bonuses.Add(this._skillSum?.Sum ?? new AttributeSum());
		CharacterAttributeView Attribute(int index, string tag, string label, string field, Color color)
		{
			var value = input.Attributes[index];
			var total = CharacterPanelLayout.AttributeTotal(value, bonuses[field], bonuses[field + "Modifier"]);
			var shown = index < 2 ? $"{total} / {total}" : total.ToString(culture);
			var tip = string.Format(culture, input.Editable ? Resources.CEAttributeToolTip : Resources.CEAttributeBaseToolTip, value, total - value);
			return new CharacterAttributeView(Game(tag, label), shown, color, tip);
		}

		var points = input.AttributePoints;
		return new CharacterPanelView
		{
			Name = input.Player.PlayerName,
			// The class goes with the masteries: a change of masteries clears it when it is applied, as the game sets it again.
			ClassName = input.ClassCleared ? string.Empty
				: this._translation.TranslateXTag(info.Class ?? string.Empty, true, true) ?? string.Empty,
			LevelText = CharacterPanelLayout.FormatGameText(Game("tagCLevel", "Level : {%d0}"), input.Level),
			XpText = CharacterPanelLayout.FormatGameText(Game("xtagCXP", "XP : {%t0}"), input.Xp),
			AttributesTitle = Game("tagCAttributes", Resources.CEAttributes),
			PointsText = points <= 0 ? string.Empty
				: CharacterPanelLayout.FormatGameText(this._translation.TranslateXTag(points == 1 ? "tagCPointsSingular" : "tagCPoints") ?? "{%d0}", points),
			CanRaise = input.Editable && points > 0,
			CanUndo = input.Editable && input.CanUndo,
			UndoText = Game("tagSkillUndo", "Undo"),
			Attributes =
			[
				Attribute(0, "tagCHealth", Resources.CEHealth, "characterLife", Color.FromArgb(255, 40, 40)),
				Attribute(1, "tagCEnergy", Resources.CEMana, "characterMana", Color.FromArgb(90, 90, 255)),
				Attribute(2, "tagCStrength", Resources.CEStrength, "characterStrength", Color.White),
				Attribute(3, "tagCIntelligence", Resources.CEIntelligence, "characterIntelligence", Color.White),
				Attribute(4, "tagCDexterity", Resources.CEDexterity, "characterDexterity", Color.White),
			],
			Stats = this.StatsView(input),
			GreatestMonsterTitle = Game("tagCHighestMonster", "Greatest Monster Killed"),
			GreatestMonster = this._translation.TranslateOr(info.GreatestMonster, info.GreatestMonster),
			GreatestMonsterLevel = info.GreatestMonsterKilledLevel == 0 ? string.Empty
				: CharacterPanelLayout.FormatGameText(Game("tagCLevel", "Level : {%d0}"), info.GreatestMonsterKilledLevel),
			Statistics =
			[
				(Game("tagCMonsterKills", "Monsters Killed"), info.NumberOfKills.ToString("N0", culture)),
				(Game("tagCHighestDamage", "Greatest Damage Dealt"), info.GreatestDamageInflicted.ToString("N0", culture)),
				(Game("tagCTime", "Elapsed Time"), CharacterPanelLayout.PlayTime(Game("tagCTimeNumber", "{%d0}:{%d1}:{%d2}"), info.PlayTimeInSeconds)),
				(Game("tagCDeaths", "Total Deaths"), info.NumberOfDeaths.ToString("N0", culture)),
			],
			TabNames = [Game("tagCPrimaryStats", "I"), Game("tagCSecondaryStats", "II"), Game("x4tagCThirdStats", "III")],
			Money = info.Money.ToString("N0", culture),
			Electrum = info.Electrum?.ToString("N0", culture) ?? string.Empty,
		};
	}

	/// <summary>What the panel shows of the computed statistics, with the game's texts.</summary>
	public CharacterStatsView StatsView(CharacterPanelInput input)
	{
		var stats = this.ComputeStats(input);
		var (attackSpeed, offense) = this.ComputeOffense(input);
		var culture = CultureInfo.CurrentCulture;
		var balance = this._service.GetGameBalance();
		string Game(string tag, string fallback) => this._translation.TranslateOr(tag, fallback);
		string Percent(int value) => value.ToString(culture) + "%";
		string Signed(string tag, string fallback, int value) => CharacterPanelLayout.FormatGameText(Game(tag, fallback), value);
		StatRowView Row(string tag, string fallback, string value, string toolTip = "") => new(Game(tag, fallback), value, toolTip);

		// A damage's rollover block: its name, its bonus and its Global Multiplier, then the damage over time's name, its
		// bonus "with +n% Improved Duration" and its multiplier.
		List<string> DamageLines(StatDamage damage, string? damageTag, string? durationDamageTag, string name)
		{
			var lines = new List<string>();
			var bonus = stats.DamageBonuses.GetValueOrDefault(damage);
			var equations = DamageEquations.GetValueOrDefault(damage);
			void Multiplier((string Equation, string Variable)? equation)
			{
				if (equation is not { } e
					|| DamageMultiplier.Of(balance.DamageEquation(e.Equation), e.Variable, stats.Strength, stats.Dexterity, stats.Intelligence, input.Level) is not { } multiplier)
					return;

				var (tag, fallback) = multiplier.Attribute switch
				{
					MultiplierAttribute.Dexterity => ("x4tagCAttribBonusDex", "*{%.2f0} Global Multiplier (Dex)"),
					MultiplierAttribute.Intelligence => ("x4tagCAttribBonusInt", "*{%.2f0} Global Multiplier (Int)"),
					_ => ("x4tagCAttribBonusLvl", "*{%.2f0} Global Multiplier (Lvl)"),
				};
				lines.Add(Game(tag, fallback).Replace("{%.2f0}", multiplier.Value.ToString("0.00", culture), StringComparison.Ordinal));
			}

			// Physical and pierce scale with Strength or Dexterity differently with a weapon and without: the game shows
			// both slopes, each with that attribute alone.
			void WeaponAndSpell(string weaponEquation, string spellEquation, string variable, string attribute, double value, string suffix)
			{
				foreach (var (equation, kind, fallback) in (ReadOnlySpan<(string, string, string)>)[(weaponEquation, "_W", "Weapon"), (spellEquation, "_S", "Spell")])
				{
					var slope = DamageMultiplier.Slope(balance.DamageEquation(equation), variable, attribute, value);
					lines.Add(Game($"x4tagCAttribBonus{suffix}{kind}", $"*{{%.2f0}} {fallback} Multiplier ({suffix})")
						.Replace("{%.2f0}", slope.ToString("0.00", culture), StringComparison.Ordinal));
				}
			}

			if (damageTag is not null)
			{
				lines.AddRange([name, Signed(damageTag, "{%+.0f0}%", bonus.Damage)]);
				if (damage == StatDamage.Physical)
					WeaponAndSpell("physicalDamageEquation", "physicalNonWeaponDamageEquation", "physicalDamageDV", "strengthDV", stats.Strength, "Str");
				else if (damage == StatDamage.Pierce)
					WeaponAndSpell("pierceDamageEquation", "pierceNonWeaponDamageEquation", "pierceDamageDV", "dexterityDV", stats.Dexterity, "Dex");
				else
					Multiplier(equations.Direct);
			}

			if (durationDamageTag is not null)
			{
				if (lines.Count != 0)
					lines.Add(string.Empty);

				var over = durationDamageTag.Replace("DamageDurationModifier", "DamageDuration", StringComparison.Ordinal);
				lines.Add(damageTag is null ? name : Game(over, name));
				lines.Add(Signed(durationDamageTag, "{%+.0f0}%", bonus.DurationDamage) + "  "
					+ Signed("ImprovedTimeFormat", "with {%+.0f0}% Improved Duration", bonus.Duration));
				Multiplier(equations.OverTime);
			}

			return lines;
		}

		// The Pet Bonuses rollover: the pets' damage % for physical, elemental and vitality, then their flat damages, with
		// the same texts without the sign and the % ("0 Physical Damage").
		IEnumerable<string> PetLines(PetDamageBonus pet)
		{
			(string Tag, string Fallback, int Percent, int Flat)[] kinds =
			[
				("DamageModifierPhysical", "{%+.0f0}% Physical Damage", pet.PhysicalPercent, pet.Physical),
				("DamageModifierElemental", "{%+.0f0}% Elemental Damages", pet.ElementalPercent, pet.Elemental),
				("DamageModifierLife", "{%+.0f0}% Vitality Damage", pet.VitalityPercent, pet.Vitality),
			];
			foreach (var kind in kinds)
				yield return Signed(kind.Tag, kind.Fallback, kind.Percent);
			foreach (var kind in kinds)
				yield return CharacterPanelLayout.FormatGameText(Game(kind.Tag, kind.Fallback).Replace("{%+.0f0}%", "{%.0f0}", StringComparison.Ordinal), kind.Flat);
		}

		return new CharacterStatsView
		{
			DefenseTitle = Game("tagCDefense", "Defense"),
			Armor = Row("tagCCombineArmor", "Combined Armor", stats.CombinedArmor.ToString(culture)),
			Resistances = [.. Enum.GetValues<StatResistance>().Select(r => new StatIconView(
				ResistanceIcons[(int)r].Icon,
				Percent(stats.Resistances[r]),
				Game(ResistanceIcons[(int)r].Tag, ResistanceIcons[(int)r].Fallback)))],
			OffenseTitle = Game("tagCOffense", "Offense"),
			Offense =
			[
				Row("tagCTotalDamage", "Average Damage", offense.AverageDamage.ToString(culture),
					string.Format(culture, Resources.CEOffenseDamageToolTip, offense.WeaponDamage, offense.AddedDamage)),
				Row("tagCAttackSpeed", "Attack Speed", Percent(CharacterStatsCalculator.Round(attackSpeed))),
				Row("tagCDPS", "Damage Per Second (DPS)", offense.Dps.ToString(culture)),
			],
			SecondaryTitle = Game("tagCSecondaryResist", "Secondary Resistances:"),
			Tab2Rows =
			[
				Row("tagCLifeRegen", "Health Regeneration", stats.LifeRegen.ToString("0.00", culture)),
				Row("tagCEnergyRegen", "Energy Regeneration", stats.ManaRegen.ToString("0.00", culture)),
				Row("x4tagCManaCostReduction", "Energy Cost Reduction", Percent(stats.ManaCostReduction)),
				Row("x4tagCCooldownReduction", "Recharge Reduction", Percent(stats.CooldownReduction)),
				Row("tagCCastSpeed", "Casting Speed", Percent(stats.CastSpeed)),
				Row("tagCRunSpeed", "Movement Speed", Percent(stats.RunSpeed)),
				Row("tagCOffensiveAbility", "Offensive Ability", stats.OffensiveAbility.ToString(culture)),
				Row("tagCDefensiveAbility", "Defensive Ability", stats.DefensiveAbility.ToString(culture)),
				Row("x4tagCDodgePercent", "Chance to Dodge Attacks", Percent(stats.Dodge)),
				Row("x4tagCDeflectProjectileChance", "Chance to Avoid Projectiles", Percent(stats.DeflectProjectile)),
				Row("tagCBlockPercent", "Block Chance", Percent(stats.Block)),
			],
			DamageTitle = Game("x4tagCDamageStats2", "Damage & Duration Bonuses"),
			// As the game's rollover: one block per damage, its name then its bonus; the damage over time "with +n%
			// Improved Duration". A blank line starts a block, whose first line is its title.
			Damages = [.. DamageIcons.Select(d =>
			{
				var name = Game(d.Tag, d.Damage.ToString());
				if (d.Damage == StatDamage.Pet)
					return new StatIconView(d.Icon, string.Empty, string.Join(Environment.NewLine, [name, .. PetLines(stats.PetBonuses)]));

				var lines = DamageLines(d.Damage, d.DamageTag, d.DurationDamageTag, name);

				// The game shows energy leech under life leech, as a second block.
				if (d.Damage == StatDamage.LifeLeech)
					lines.AddRange([string.Empty, .. DamageLines(StatDamage.ManaLeech, null, "DamageDurationModifierManaLeach", Game("DamageDurationManaLeach", "Energy Leech"))]);

				return new StatIconView(d.Icon, string.Empty, string.Join(Environment.NewLine, lines.Count == 0 ? [name] : lines));
			})],
			Tab3Rows =
			[
				Row("x4tagCADctH", "Weapon Damage as Health", Percent(stats.AttackDamageToHealth)),
				Row("x4tagCEleConversion", "Weapon Base as Elemental", Percent(stats.PhysicalToElemental)),
			],
			RaceTitle = Game("x4tagCMonsterStats", "Monster Race Bonuses"),
			Races = [.. Enum.GetValues<StatRace>().Select(r =>
			{
				var (icon, tag, fallback) = RaceIcons[(int)r];
				var name = Game(tag, fallback);
				var bonus = stats.RaceBonuses[r];
				string Line(string lineTag, string lineFallback, double value)
					=> CharacterPanelLayout.FormatGameText(Game(lineTag, lineFallback).Replace("{%s1}", name, StringComparison.Ordinal), (long)Math.Round(value));
				var lines = new List<string>
				{
					name,
					Line("RacialBonusPercentDamage", "{%+.0f0}% Damage to {%s1}", bonus.PercentDamage),
					Line("RacialBonusAbsoluteDamage", "{%+.0f0} Damage to {%s1}", bonus.AbsoluteDamage),
					Line("RacialBonusPercentDefense", "{%+.0f0}% Less Damage from {%s1}", bonus.PercentDefense),
				};
				return new StatIconView(icon, string.Empty, string.Join(Environment.NewLine, lines));
			})],
		};
	}
}
