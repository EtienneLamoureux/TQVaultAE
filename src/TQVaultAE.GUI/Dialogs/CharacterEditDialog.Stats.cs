using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// The statistics the Character page computes like the game (<see cref="CharacterStatsCalculator"/>): defense on tab I,
/// tab II, the damage and race bonuses of tab III, from the equipped items, the skills of the editor (their current
/// points), the attributes of the editor, the difficulty chosen and the buffs option.
/// </summary>
internal sealed partial class CharacterEditDialog
{
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
		(StatDamage.Physical, """XPack2\ingameui\characterscreen\resistances\physicalicon01.tex""", "DamagePhysical", "DamageModifierPhysical", "DamageDurationModifierPhysical"),
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

	/// <summary>The statistics for the current state of the editor.</summary>
	private CharacterStats ComputeStats()
	{
		var service = this.Services.CharacterStatsService;
		this._gearSum ??= service.GetGearAttributes(this.PlayerCollection.EquipmentSack as IEnumerable<Item> ?? []);

		// The skills as the editor has them now; their sum is kept until a point or the buffs option changes.
		var records = this._skills?.BuildSkillRecords() ?? this.Info.SkillRecordList;
		bool buffs = this._includeBuffs?.Checked == true;
		var key = (buffs ? "+" : "-") + string.Join(";", records.Select(r => r.skillName + "=" + r.skillLevel.ToString(CultureInfo.InvariantCulture)));
		if (this._skillSum?.Key != key)
		{
			var trees = this._skills?.Slots.OfType<MasteryTree>().ToList() ?? [];
			this._skillSum = (key, service.GetSkillAttributes(records, this._gearAugments ?? [], trees, buffs));
		}

		int Attribute(int index) => Convert.ToInt32(this.AttributeBoxes[index].Value);
		var difficulty = this._statsDifficulty?.SelectedIndex is >= 0 and var selected ? selected : Math.Clamp(this.Info.DifficultyUnlocked, 0, 2);
		return CharacterStatsCalculator.Compute(new CharacterStatsInput(
			Attribute(2), Attribute(4), Attribute(3), this._gearSum, this._skillSum.Value.Sum, difficulty, service.GetGameBalance()));
	}

	/// <summary>What the panel shows of the computed statistics, with the game's texts.</summary>
	private CharacterStatsView StatsView()
	{
		var stats = this.ComputeStats();
		var culture = CultureInfo.CurrentCulture;
		string Game(string tag, string fallback) => string.IsNullOrEmpty(tag) ? fallback : this.TranslateTag(tag, fallback);
		string Percent(int value) => value.ToString(culture) + "%";
		string Signed(string tag, string fallback, int value) => CharacterPanelLayout.FormatGameText(Game(tag, fallback), value);
		StatRowView Row(string tag, string fallback, string value, string toolTip = "") => new(Game(tag, fallback), value, toolTip);

		var computedInGame = Resources.CEComputedInGame;
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
				Row("tagCTotalDamage", "Average Damage", "-", computedInGame),
				Row("tagCAttackSpeed", "Attack Speed", "-", computedInGame),
				Row("tagCDPS", "Damage Per Second (DPS)", "-", computedInGame),
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
			Damages = [.. DamageIcons.Select(d =>
			{
				var lines = new List<string> { Game(d.Tag, d.Damage.ToString()) };
				if (!stats.DamageBonuses.TryGetValue(d.Damage, out var bonus))
					lines.Add(computedInGame);
				else
				{
					if (d.DamageTag is not null)
						lines.Add(Signed(d.DamageTag, "{%+.0f0}%", bonus.Damage));
					if (d.DurationDamageTag is not null)
						lines.Add(Signed(d.DurationDamageTag, "{%+.0f0}%", bonus.DurationDamage));
					if (d.DurationDamageTag is not null)
						lines.Add(string.Format(culture, Resources.CEDurationBonus, (bonus.Duration >= 0 ? "+" : string.Empty) + bonus.Duration.ToString(culture)));
				}

				return new StatIconView(d.Icon, string.Empty, string.Join(Environment.NewLine, lines));
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
