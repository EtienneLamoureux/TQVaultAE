using System.Drawing;
using System.Globalization;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Layout;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation;

/// <summary>
/// The state of a character the statistics panel is built from: the save, the attributes as spent, the skills as
/// learned, and the options. The character editor gives its editors' values; the main window gives the save's.
/// </summary>
public sealed record CharacterPanelInput
{
	/// <summary>Indexes of <see cref="Attributes"/>, in the order of the game's character window.</summary>
	public const int Health = 0, Energy = 1, Strength = 2, Intelligence = 3, Dexterity = 4;

	/// <summary>
	/// The character as saved, for the main window: nothing can be changed, the difficulty is the last one unlocked.
	/// <paramref name="equipment"/> are the items worn, the ones the item requirements count
	/// (<see cref="IPlayerService.GetWornEquipment"/>), and <paramref name="augments"/> their skill bonuses.
	/// </summary>
	/// <param name="buffs">The toggles of the Buffs tab for this character, kept by the panel's owner; none for the defaults.</param>
	/// <param name="alternateWeapons">The weapon set counted (<see cref="AlternateWeapons"/>); null for the one in hand in the save.</param>
	public static CharacterPanelInput FromSave(PlayerCollection player, PlayerInfo info, IReadOnlyList<Item> equipment, IReadOnlyList<MasteryTree> trees, IReadOnlyList<SkillAugment> augments, BuffSelection? buffs = null, bool? alternateWeapons = null)
		=> new()
		{
			Player = player,
			Info = info,
			Equipment = equipment,
			Attributes = [info.BaseHealth, info.BaseMana, info.BaseStrength, info.BaseIntelligence, info.BaseDexterity],
			AttributePoints = info.AttributesPoints,
			Level = info.CurrentLevel,
			Xp = info.CurrentXP,
			Skills = info.SkillRecordList,
			Trees = trees,
			Augments = augments,
			Difficulty = CharacterPanelLayout.ShownDifficulty(info.DifficultyUnlocked),
			Buffs = buffs ?? new(),
			AlternateWeapons = alternateWeapons,
		};

	public required PlayerCollection Player { get; init; }
	public required PlayerInfo Info { get; init; }

	/// <summary>The items worn; null takes every item of the character's equipment sack.</summary>
	public IReadOnlyList<Item>? Equipment { get; init; }

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

	/// <summary>Which buff sources count: the Buffs tab's toggles, kept by the panel's owner for the character shown.</summary>
	public BuffSelection Buffs { get; init; } = new();

	/// <summary>
	/// The weapon set the statistics count, chosen in the panel without moving the pieces: true for the second set (rows
	/// 2 and 3 of the weapon slots), false for the first; null for the one in hand in the save
	/// (<see cref="PlayerCollection.UseAlternate"/>). <see cref="Augments"/> must be those of the same set.
	/// </summary>
	public bool? AlternateWeapons { get; init; }

	/// <summary>True when the statistics count the second weapon set (<see cref="AlternateWeapons"/>).</summary>
	public bool CountsAlternateWeapons => this.AlternateWeapons ?? this.Player.UseAlternate;

	/// <summary>True when the masteries changed: the class is the one <see cref="Trees"/> make, not the save's.</summary>
	public bool ClassCleared { get; init; }

	/// <summary>True when the attributes can be changed in the panel: "+" buttons while points are left, and their tool tip.</summary>
	public bool Editable { get; init; }

	/// <summary>True when attributes changed since the start: the Undo button shows.</summary>
	public bool CanUndo { get; init; }
}

/// <summary>
/// Builds what <see cref="CharacterStatsPanel"/> shows of a character: the statistics computed like the game
/// (<see cref="CharacterStatsCalculator"/>: defense on tab I, tab II, the damage and race bonuses of tab III) from the
/// equipped items, the skills, the attributes, the difficulty and the buff sources turned on (the Buffs tab), with the
/// game's texts. The buff sources are kept until the equipment, the skills or the difficulty change.
/// </summary>
/// <param name="editService">Describes the skills and buffs of the Buffs tab's tool tips; none leaves them out.</param>
/// <param name="itemText">Names the items of the Buffs tab; none names them by their record.</param>
public sealed partial class CharacterPanelViewBuilder(ICharacterStatsService service, ITranslationService translation,
	ICharacterEditService? editService = null, IItemTextFormatter? itemText = null)
{
	private readonly ICharacterStatsService _service = service;
	private readonly ITranslationService _translation = translation;
	private readonly ICharacterEditService? _editService = editService;
	private readonly IItemTextFormatter? _itemText = itemText;

	private string? _gearKey;
	private (string Key, int Levels, IReadOnlyList<BuffSource> Sources)? _sources;

	/// <summary>The sum of the gear sources turned on, once computed.</summary>
	public AttributeSum? GearSum { get; private set; }

	/// <summary>The sum of the other sources turned on (skills, curses, shrines...), once computed.</summary>
	public AttributeSum? SkillSum { get; private set; }

	/// <summary>The buff sources of the character, once computed.</summary>
	public IReadOnlyList<BuffSource> BuffSources => this._sources?.Sources ?? [];

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


	private static IEnumerable<Item> Equipment(CharacterPanelInput input)
		=> (IEnumerable<Item>?)input.Equipment ?? input.Player.EquipmentSack ?? Enumerable.Empty<Item>();

	/// <summary>Forgets the stance in hand when the equipment or the weapon set in hand changed.</summary>
	private void TrackGear(CharacterPanelInput input)
	{
		var key = (input.CountsAlternateWeapons ? "+" : "-") + string.Join(";", Equipment(input).Select(i =>
			string.Join(",", i.BaseItemId?.Raw, i.prefixID?.Raw, i.suffixID?.Raw, i.relicID?.Raw, i.RelicBonusId?.Raw, i.relic2ID?.Raw, i.RelicBonus2Id?.Raw,
				i.Seed.ToString(CultureInfo.InvariantCulture), i.Var1.ToString(CultureInfo.InvariantCulture), i.Var2.ToString(CultureInfo.InvariantCulture), i.Mod)));
		if (key == this._gearKey)
			return;

		this._gearKey = key;
		this.Stance = null;
	}

	private static int Difficulty(CharacterPanelInput input) => Math.Clamp(input.Difficulty, 0, CharacterPanelLayout.Legendary);

	/// <summary>
	/// The buff sources of the character (<see cref="ICharacterStatsService.GetBuffSources"/>), kept until the equipment,
	/// a skill point, the saved buffs, the difficulty, the scroll picked or the skill levels the buffs on add change.
	/// </summary>
	public IReadOnlyList<BuffSource> Sources(CharacterPanelInput input)
	{
		ArgumentNullException.ThrowIfNull(input);
		this.TrackGear(input);
		int difficulty = Difficulty(input);
		var key = string.Join("|", this._gearKey,
			string.Join(";", input.Skills.Select(r => r.skillName + "=" + r.skillLevel.ToString(CultureInfo.InvariantCulture))),
			string.Join(";", input.Info.SavedBuffs.Select(b => b.Normalized)),
			difficulty.ToString(CultureInfo.InvariantCulture), input.Buffs.Scroll?.Normalized);

		// A buff on that adds levels to every skill (Battle Standard's "+1 to all skills") raises the skills, its own
		// included: the sources are read again at those levels, until the levels added hold (a second read at most).
		int levels = this._sources is { } known && known.Key == key ? known.Levels : 0;
		for (int read = 0; read < 3; read++)
		{
			if (this._sources is not { } cached || cached.Key != key || cached.Levels != levels)
			{
				IReadOnlyList<SkillAugment> augments = levels == 0 ? input.Augments : [.. input.Augments, new SkillAugment(SkillAugmentKind.All, null, levels)];
				this._sources = (key, levels, this._service.GetBuffSources(new BuffSourceRequest([.. Equipment(input)], input.CountsAlternateWeapons,
					input.Skills, augments, input.Trees, input.Info.SavedBuffs, difficulty, input.Buffs.Scroll)));
				this._names.Clear();
				this._descriptions.Clear();
			}

			int added = BuffSums.SkillLevelsAdded(this._sources.Value.Sources, input.Buffs.IsOn);
			if (added == levels)
				break;
			levels = added;
		}

		return this._sources!.Value.Sources;
	}

	/// <summary>
	/// Toggles a buff source of the character in <paramref name="selection"/> (a click in the Buffs tab).
	/// </summary>
	/// <returns>false when the character has no such source.</returns>
	public bool ToggleBuff(BuffSelection selection, string id)
	{
		ArgumentNullException.ThrowIfNull(selection);
		if (this.BuffSources.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) is not { } source)
			return false;

		selection.Toggle(source);
		return true;
	}

	/// <summary>What the panel computes for a set of sources: the statistics, the bonuses summed, tab I's offense.</summary>
	private sealed record Evaluation(CharacterStats Stats, AttributeSum Bonuses, double AttackSpeed, OffenseValues Offense);

	/// <summary>The statistics with the sources turned on, without <paramref name="without"/> when given.</summary>
	private Evaluation Evaluate(CharacterPanelInput input, IReadOnlyList<BuffSource> sources, BuffSource? without = null)
	{
		var isOn = input.Buffs.IsOn;
		var (gear, skills) = BuffSums.Sum(sources, isOn, without);
		int difficulty = Difficulty(input);
		var balance = this._service.GetGameBalance();
		var stats = CharacterStatsCalculator.Compute(new CharacterStatsInput(input.Attributes[CharacterPanelInput.Strength],
			input.Attributes[CharacterPanelInput.Dexterity], input.Attributes[CharacterPanelInput.Intelligence], gear, skills, difficulty, balance));

		var all = new AttributeSum();
		all.Add(gear);
		all.Add(skills);

		// The attack: the stance of the weapons worn (a weapon turned off keeps its animation), the gear turned on.
		this.Stance ??= this._service.GetWeaponStance(Equipment(input), input.CountsAlternateWeapons);
		var durations = this._service.GetAttackAnimationDurations(input.Info.IsFemale);
		var attackSpeed = CharacterOffenseCalculator.AttackSpeed(all, this.Stance.Value, balance, difficulty,
			durations.GetValueOrDefault(WeaponStance.Unarmed), durations.GetValueOrDefault(this.Stance.Value));

		double Total(int attribute, string field) => CharacterStatsCalculator.Total(input.Attributes[attribute], all, field);
		var offenseGear = BuffSums.Offense(sources, isOn, without);
		var offense = CharacterOffenseCalculator.Damage(new OffenseInput(offenseGear, gear, skills,
			Total(CharacterPanelInput.Strength, "characterStrength"), Total(CharacterPanelInput.Dexterity, "characterDexterity"),
			Total(CharacterPanelInput.Intelligence, "characterIntelligence"),
			input.Level, attackSpeed, balance));

		if (without is null)
		{
			this.GearSum = gear;
			this.SkillSum = skills;
			this.OffenseGear = offenseGear;
		}

		return new Evaluation(stats, all, attackSpeed, offense);
	}

	/// <summary>The statistics of the character, with the buff sources turned on.</summary>
	public CharacterStats ComputeStats(CharacterPanelInput input)
		=> this.Evaluate(input, this.Sources(input)).Stats;

	/// <summary>Tab I's offense: attack speed from the stance in hand and its animations, the average damage and DPS.</summary>
	public (double AttackSpeed, OffenseValues Values) ComputeOffense(CharacterPanelInput input)
	{
		var evaluation = this.Evaluate(input, this.Sources(input));
		return (evaluation.AttackSpeed, evaluation.Offense);
	}

	/// <summary>What the statistics panel shows of the character.</summary>
	public CharacterPanelView Build(CharacterPanelInput input)
	{
		var info = input.Info;
		var culture = CultureInfo.CurrentCulture;
		string Game(string tag, string fallback) => this._translation.TranslateOr(tag, fallback);

		// The bonuses as the statistics sum them, unrounded: the game adds 13.5 strength of a mastery as is.
		var composition = this.Compose(input);
		var bonuses = composition.Total.Bonuses;
		CharacterAttributeView Attribute(int index, string tag, string label, string field, Color color)
		{
			var value = input.Attributes[index];
			int Total(AttributeSum sum) => CharacterPanelLayout.AttributeTotal(value, sum[field], sum[field + "Modifier"]);
			var total = Total(bonuses);
			// Health and energy show "current / maximum", full outside the game.
			var shown = index is CharacterPanelInput.Health or CharacterPanelInput.Energy ? $"{total} / {total}" : total.ToString(culture);
			var tip = string.Format(culture, input.Editable ? Resources.CEAttributeToolTip : Resources.CEAttributeBaseToolTip, value, total - value);
			var toolTip = composition.Append(StatToolTip.Sentence(tip), e => Total(e.Bonuses), [field, field + "Modifier"]);

			// The energy some sources reserve (curses, Tartarus): the part of the energy that cannot be used, and who
			// reserves it.
			if (index == CharacterPanelInput.Energy && bonuses[EnergyReserve] is var reserve and not 0)
			{
				var percent = Composition.Signed(reserve, culture).TrimStart('+');
				var amount = (int)Math.Round(total * Math.Clamp(reserve, 0, 100) / 100);
				toolTip = composition.Append(toolTip, null, sum => sum[EnergyReserve] is var r and not 0 ? [Composition.Signed(r, culture).TrimStart('+') + "%"] : [],
					Resources.BuffsEnergyReservedTitle, first: [string.Format(culture, Resources.BuffsEnergyReservedTotal, percent, amount, total - amount)]);
			}

			return new CharacterAttributeView(Game(tag, label), shown, color, toolTip);
		}

		var points = input.AttributePoints;
		return new CharacterPanelView
		{
			Name = input.Player.PlayerName,
			// The class goes with the masteries: after a change, the one the new masteries make.
			ClassName = this._translation.TranslateXTag(
				(input.ClassCleared ? CharacterClasses.TagOf(input.Trees.Select(t => t.Mastery)) : info.ClassTag) ?? string.Empty, true, true) ?? string.Empty,
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
				Attribute(CharacterPanelInput.Health, "tagCHealth", Resources.CEHealth, "characterLife", Color.FromArgb(255, 40, 40)),
				Attribute(CharacterPanelInput.Energy, "tagCEnergy", Resources.CEMana, "characterMana", Color.FromArgb(90, 90, 255)),
				Attribute(CharacterPanelInput.Strength, "tagCStrength", Resources.CEStrength, "characterStrength", Color.White),
				Attribute(CharacterPanelInput.Intelligence, "tagCIntelligence", Resources.CEIntelligence, "characterIntelligence", Color.White),
				Attribute(CharacterPanelInput.Dexterity, "tagCDexterity", Resources.CEDexterity, "characterDexterity", Color.White),
			],
			Stats = this.StatsView(input, composition),
			Buffs = this.BuffView(input),
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
			TabNames = [Game("tagCPrimaryStats", "I"), Game("tagCSecondaryStats", "II"), Game("x4tagCThirdStats", "III"), "IV"],
			Money = info.Money.ToString("N0", culture),
			Electrum = info.Electrum?.ToString("N0", culture) ?? string.Empty,
		};
	}

	/// <summary>What the panel shows of the computed statistics, with the game's texts.</summary>
	public CharacterStatsView StatsView(CharacterPanelInput input) => this.StatsView(input, this.Compose(input));

	/// <summary>
	/// The weapon set gems I and II of tab I: the set counted lit, each with its rollover (what a click does, which set
	/// the save has in hand).
	/// </summary>
	private List<WeaponSetView> WeaponSetsView(CharacterPanelInput input)
	{
		var culture = CultureInfo.CurrentCulture;
		var title = this._translation.TranslateOr("tagHUD_WeaponSwap_R", "Weapon Swap");
		var inSave = input.Player.UseAlternate ? "II" : "I";
		return [.. new[] { false, true }.Select(alternate =>
		{
			var name = alternate ? "II" : "I";
			bool counted = alternate == input.CountsAlternateWeapons;
			List<string> lines = [string.Format(culture, counted ? Resources.WeaponSetCounted : Resources.WeaponSetPick, name),
				string.Format(culture, Resources.WeaponSetInSave, inSave)];
			return new WeaponSetView(name, counted, new StatToolTip([new StatToolTipBlock(title, lines)]));
		})];
	}

	/// <summary>
	/// What the panel shows of the computed statistics, with the game's texts; the rollovers of tab I and II list the
	/// sources of each value (<see cref="Composition"/>).
	/// </summary>
	private CharacterStatsView StatsView(CharacterPanelInput input, Composition composition)
	{
		var stats = composition.Total.Stats;
		var (attackSpeed, offense) = (composition.Total.AttackSpeed, composition.Total.Offense);
		var culture = CultureInfo.CurrentCulture;
		var balance = this._service.GetGameBalance();
		string Game(string tag, string fallback) => this._translation.TranslateOr(tag, fallback);
		string Percent(int value) => value.ToString(culture) + "%";
		string Signed(string tag, string fallback, int value) => CharacterPanelLayout.FormatGameText(Game(tag, fallback), value);

		// A row whose rollover lists its sources: the value shown from an evaluation, the variables it reads.
		StatRowView Sourced(string tag, string fallback, string value, Func<Evaluation, double> shown, IReadOnlyList<string> variables, string toolTip = "")
			=> new(Game(tag, fallback), value, composition.Append(StatToolTip.Sentence(toolTip), shown, variables));
		string[] Own(string field) => [field, field + "Modifier"];

		// A damage's rollover blocks: its name, its bonus and its Global Multiplier, then the damage over time's name, its
		// bonus "with +n% Improved Duration" and its multiplier.
		List<StatToolTipBlock> DamageBlocks(StatDamage damage, string? damageTag, string? durationDamageTag, string name)
		{
			var blocks = new List<StatToolTipBlock>();
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
				lines = [Signed(damageTag, "{%+.0f0}%", bonus.Damage)];
				blocks.Add(new StatToolTipBlock(name, lines));
				if (damage == StatDamage.Physical)
					WeaponAndSpell("physicalDamageEquation", "physicalNonWeaponDamageEquation", "physicalDamageDV", "strengthDV", stats.Strength, "Str");
				else if (damage == StatDamage.Pierce)
					WeaponAndSpell("pierceDamageEquation", "pierceNonWeaponDamageEquation", "pierceDamageDV", "dexterityDV", stats.Dexterity, "Dex");
				else
					Multiplier(equations.Direct);
			}

			if (durationDamageTag is not null)
			{
				var over = durationDamageTag.Replace("DamageDurationModifier", "DamageDuration", StringComparison.Ordinal);
				lines = [Signed(durationDamageTag, "{%+.0f0}%", bonus.DurationDamage) + "  "
					+ Signed("ImprovedTimeFormat", "with {%+.0f0}% Improved Duration", bonus.Duration)];
				blocks.Add(new StatToolTipBlock(damageTag is null ? name : Game(over, name), lines));
				Multiplier(equations.OverTime);
			}

			return blocks;
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

		// The sources of a damage bonus: one block for the damage %, one for the damage over time %, one for its duration %
		// (a plain sum); life leech adds energy leech's.
		string Of(string part) => string.Format(culture, Resources.BuffsSourcesOf, part);
		StatToolTip DamageSources(StatToolTip tip, StatDamage damage, string name, string overName)
		{
			var (direct, overTime, duration) = CharacterStatsCalculator.DamageVariables(damage);
			if (direct.Count != 0)
				tip = composition.Append(tip, e => e.Stats.DamageBonuses[damage].Damage, direct, title: Of(name));
			if (overTime.Count != 0)
				tip = composition.Append(tip, e => e.Stats.DamageBonuses[damage].DurationDamage, overTime, title: Of(overName));
			if (duration.Count != 0)
				tip = composition.Append(tip, null, duration, title: Of($"{overName} ({Resources.BuffsDuration})"));
			if (damage == StatDamage.LifeLeech)
			{
				var mana = Game("DamageDurationManaLeach", "Energy Leech");
				tip = DamageSources(tip, StatDamage.ManaLeech, mana, mana);
			}

			return tip;
		}

		return new CharacterStatsView
		{
			DefenseTitle = Game("tagCDefense", "Defense"),
			Armor = Sourced("tagCCombineArmor", "Combined Armor", stats.CombinedArmor.ToString(culture), e => e.Stats.CombinedArmor, Own("defensiveProtection")),
			Resistances = [.. Enum.GetValues<StatResistance>().Select(r => new StatIconView(
				ResistanceIcons[(int)r].Icon,
				Percent(stats.Resistances[r]),
				composition.Append(new StatToolTip([new StatToolTipBlock(Game(ResistanceIcons[(int)r].Tag, ResistanceIcons[(int)r].Fallback), [])]),
					e => e.Stats.Resistances[r], CharacterStatsCalculator.ResistanceVariables(r),
					CharacterStatsCalculator.ResistancePenalty(r, balance, Difficulty(input)))))],
			OffenseTitle = Game("tagCOffense", "Offense"),
			WeaponSets = this.WeaponSetsView(input),
			Offense =
			[
				Sourced("tagCTotalDamage", "Average Damage", offense.AverageDamage.ToString(culture), e => e.Offense.AverageDamage, [],
					string.Format(culture, Resources.CEOffenseDamageToolTip, offense.WeaponDamage, offense.AddedDamage)),
				Sourced("tagCAttackSpeed", "Attack Speed", Percent(CharacterStatsCalculator.Round(attackSpeed)), e => CharacterStatsCalculator.Round(e.AttackSpeed),
					[.. Own("characterAttackSpeed"), "characterTotalSpeedModifier"]),
				Sourced("tagCDPS", "Damage Per Second (DPS)", offense.Dps.ToString(culture), e => e.Offense.Dps, []),
			],
			SecondaryTitle = Game("tagCSecondaryResist", "Secondary Resistances:"),
			Tab2Rows =
			[
				Sourced("tagCLifeRegen", "Health Regeneration", stats.LifeRegen.ToString("0.00", culture), e => Math.Round(e.Stats.LifeRegen, 2), Own("characterLifeRegen")),
				Sourced("tagCEnergyRegen", "Energy Regeneration", stats.ManaRegen.ToString("0.00", culture), e => Math.Round(e.Stats.ManaRegen, 2), Own("characterManaRegen")),
				Sourced("x4tagCManaCostReduction", "Energy Cost Reduction", Percent(stats.ManaCostReduction), e => e.Stats.ManaCostReduction, Own("skillManaCostReduction")),
				Sourced("x4tagCCooldownReduction", "Recharge Reduction", Percent(stats.CooldownReduction), e => e.Stats.CooldownReduction, Own("skillCooldownReduction")),
				Sourced("tagCCastSpeed", "Casting Speed", Percent(stats.CastSpeed), e => e.Stats.CastSpeed, [.. Own("characterSpellCastSpeed"), "characterTotalSpeedModifier"]),
				Sourced("tagCRunSpeed", "Movement Speed", Percent(stats.RunSpeed), e => e.Stats.RunSpeed, [.. Own("characterRunSpeed"), "characterTotalSpeedModifier"]),
				Sourced("tagCOffensiveAbility", "Offensive Ability", stats.OffensiveAbility.ToString(culture), e => e.Stats.OffensiveAbility, Own("characterOffensiveAbility")),
				Sourced("tagCDefensiveAbility", "Defensive Ability", stats.DefensiveAbility.ToString(culture), e => e.Stats.DefensiveAbility, Own("characterDefensiveAbility")),
				Sourced("x4tagCDodgePercent", "Chance to Dodge Attacks", Percent(stats.Dodge), e => e.Stats.Dodge, Own("characterDodgePercent")),
				Sourced("x4tagCDeflectProjectileChance", "Chance to Avoid Projectiles", Percent(stats.DeflectProjectile), e => e.Stats.DeflectProjectile, Own("characterDeflectProjectile")),
				Sourced("tagCBlockPercent", "Block Chance", Percent(stats.Block), e => e.Stats.Block, ["defensiveBlockChance", "defensiveBlockModifier"]),
			],
			DamageTitle = Game("x4tagCDamageStats2", "Damage & Duration Bonuses"),
			// As the game's rollover: one block per damage, its name then its bonus; the damage over time "with +n%
			// Improved Duration".
			Damages = [.. DamageIcons.Select(d =>
			{
				var name = Game(d.Tag, d.Damage.ToString());
				if (d.Damage == StatDamage.Pet)
				{
					// The pet bonuses are plain sums: each source with its own values.
					string[] petVariables = [.. new[] { "offensivePhysicalModifier", "offensiveElementalModifier", "offensiveLifeModifier",
						"offensivePhysicalAverage", "offensiveElementalAverage", "offensiveLifeAverage" }.Select(v => AttributeSum.PetBonusPrefix + v)];
					return new StatIconView(d.Icon, string.Empty, composition.Append(
						new StatToolTip([new StatToolTipBlock(name, [.. PetLines(stats.PetBonuses)])]), null, petVariables));
				}

				var blocks = DamageBlocks(d.Damage, d.DamageTag, d.DurationDamageTag, name);

				// The game shows energy leech under life leech, as a second block.
				if (d.Damage == StatDamage.LifeLeech)
					blocks.AddRange(DamageBlocks(StatDamage.ManaLeech, null, "DamageDurationModifierManaLeach", Game("DamageDurationManaLeach", "Energy Leech")));

				// The damage over time of a direct damage by the game's name ("Burn Damage", "Crushing Damage" for physical,
				// whose rollover has no such block), not to read as a second block of the direct damage.
				var tip = new StatToolTip(blocks.Count == 0 ? [new StatToolTipBlock(name, [])] : blocks);
				var overName = d.DamageTag is null ? name
					: Game((d.DurationDamageTag ?? d.DamageTag)
						.Replace("DamageDurationModifier", "DamageDuration", StringComparison.Ordinal)
						.Replace("DamageModifier", "DamageDuration", StringComparison.Ordinal), name);
				return new StatIconView(d.Icon, string.Empty, DamageSources(tip, d.Damage, name, overName));
			})],
			Tab3Rows =
			[
				Sourced("x4tagCADctH", "Weapon Damage as Health", Percent(stats.AttackDamageToHealth), e => e.Stats.AttackDamageToHealth, ["offensiveLifeLeechMin"]),
				Sourced("x4tagCEleConversion", "Weapon Base as Elemental", Percent(stats.PhysicalToElemental), e => e.Stats.PhysicalToElemental, ["characterPhysToElementalRatio"]),
			],
			RaceTitle = Game("x4tagCMonsterStats", "Monster Race Bonuses"),
			Races = [.. Enum.GetValues<StatRace>().Select(r =>
			{
				var (icon, tag, fallback) = RaceIcons[(int)r];
				var name = Game(tag, fallback);
				string Line(string lineTag, string lineFallback, double value)
					=> CharacterPanelLayout.FormatGameText(Game(lineTag, lineFallback), (long)Math.Round(value), name);
				// In the game's order: the flat damage, the damage %, the damage taken %.
				IEnumerable<string> Lines(RacialBonus bonus) =>
				[
					Line("RacialBonusAbsoluteDamage", "{%+.0f0} Damage to {%s1}", bonus.AbsoluteDamage),
					Line("RacialBonusPercentDamage", "{%+.0f0}% Damage to {%s1}", bonus.PercentDamage),
					Line("RacialBonusPercentDefense", "{%+.0f0}% Less Damage from {%s1}", bonus.PercentDefense),
				];

				// Plain sums too: each source with its non-zero lines.
				var tip = new StatToolTip([new StatToolTipBlock(name, [.. Lines(stats.RaceBonuses[r])])]);
				return new StatIconView(icon, string.Empty, composition.Append(tip, null, sum =>
				{
					var bonus = CharacterStatsCalculator.RaceBonus(r, sum);
					return bonus == default ? [] : [string.Join(", ", Lines(bonus).Where((_, i) => i switch
					{
						0 => bonus.AbsoluteDamage != 0,
						1 => bonus.PercentDamage != 0,
						_ => bonus.PercentDefense != 0,
					}))];
				}));
			})],
		};
	}
}
