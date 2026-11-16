using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// The sources the rollovers list (tab III, the energy reserve) and the Buffs tab's tool tips: only true, non-zero values.
/// </summary>
public class BuffRolloverTests
{
	private readonly Mock<ICharacterStatsService> _stats = new();
	private readonly Mock<ICharacterEditService> _edit = new();
	private readonly CharacterPanelViewBuilder _builder;
	private IReadOnlyList<BuffSource> _sources = [];

	public BuffRolloverTests()
	{
		this._stats.Setup(s => s.GetBuffSources(It.IsAny<BuffSourceRequest>())).Returns(() => this._sources);
		this._stats.Setup(s => s.GetBuffScrolls()).Returns([]);
		this._stats.Setup(s => s.GetGameBalance()).Returns(new GameBalance());
		this._stats.Setup(s => s.GetWeaponStance(It.IsAny<IEnumerable<Item>>(), It.IsAny<bool>())).Returns(WeaponStance.Unarmed);
		this._stats.Setup(s => s.GetAttackAnimationDurations(It.IsAny<bool>())).Returns(new Dictionary<WeaponStance, double>());
		this._builder = new CharacterPanelViewBuilder(this._stats.Object, Mock.Of<ITranslationService>(), this._edit.Object);
	}

	private static CharacterPanelInput Input()
	{
		var info = new PlayerInfo(new PathIO());
		return new CharacterPanelInput
		{
			Player = new PlayerCollection("HeroA", @"C:\saves\_HeroA\Player.chr") { PlayerInfo = info },
			Info = info,
			Attributes = [300, 400, 50, 60, 70],
			Skills = [],
		};
	}

	private static BuffSource Source(string id, BuffGroup group, bool on, params (string Name, double Value)[] values)
	{
		var sum = new AttributeSum();
		foreach (var (name, value) in values)
			sum.Add(name, value);
		return new BuffSource { Id = id, Group = group, NameTag = id, DefaultOn = on, Sum = sum, Icon = "icon.tex", RecordId = $"""records\{id}.dbr""".ToRecordId() };
	}

	[Fact]
	public void DamageRollover_ListsTheSourcesOfTheDamageAndOfTheDamageOverTime()
	{
		this._sources =
		[
			Source("skill:a", BuffGroup.Passive, true, ("offensiveFireModifier", 20), ("offensiveSlowFireDurationModifier", 50)),
			Source("skill:b", BuffGroup.Passive, true, ("offensiveTotalDamageModifier", 10)),
		];

		var fire = this._builder.StatsView(Input()).Damages[5].ToolTip;

		var sources = fire.Blocks.Where(b => b.Title?.StartsWith("Sources", StringComparison.Ordinal) == true).ToList();
		sources.Should().HaveCount(3, "damage, damage over time, duration");
		sources[0].Lines.Should().Equal("skill:a: +20% (+20)", "skill:b: +10% (+10)");
		sources[1].Lines.Should().Equal("skill:b: +10% (+10)");
		sources[2].Lines.Should().ContainSingle("the duration is a plain sum").Which.Should().Be("skill:a: +50%");
	}

	[Fact]
	public void RaceRollover_ListsEachSourcesNonZeroBonuses()
	{
		var sum = new AttributeSum();
		sum.AddRace("Undead", new RacialBonus(0, 10, 30));
		this._sources = [new BuffSource { Id = "item:0", Group = BuffGroup.Gear, NameTag = "Ring", DefaultOn = true, Sum = sum, GearRole = BuffGearRole.Other }];

		var undead = this._builder.StatsView(Input()).Races[(int)StatRace.Undead].ToolTip;

		var line = undead.Blocks[^1].Lines.Should().ContainSingle().Subject;
		line.Should().StartWith("Ring: +10% Damage to Undead, +30% Less Damage from Undead");
	}

	[Fact]
	public void EnergyRow_TellsTheEnergyReservedAndWhoReservesIt()
	{
		this._sources =
		[
			Source("curse:a", BuffGroup.Curse, true, ("characterManaLimitReservePercentage", 3.75)),
			Source("tartarus:a", BuffGroup.Tartarus, true, ("characterManaLimitReservePercentage", 55)),
			Source("tartarus:off", BuffGroup.Tartarus, false, ("characterManaLimitReservePercentage", 10)),
		];

		var energy = this._builder.Build(Input()).Attributes[CharacterPanelInput.Energy].ToolTip;

		var reserve = energy.Blocks.Single(b => b.Title == "Energy reserved");
		reserve.Lines.Should().Equal("58.75% reserved: 235 energy, 165 usable", "curse:a: 3.75%", "tartarus:a: 55%");
	}

	[Fact]
	public void BuffToolTip_OnlyNonZeroValues_UnknownVariablesInWordsOrLeftOut_TheReserveInWords()
	{
		var effect = Source("tartarus:a", BuffGroup.Tartarus, false, ("characterManaLimitReservePercentage", 55), ("characterAttackSpeed", -0.35));
		this._sources = [effect];
		this._edit.Setup(e => e.DescribeSkill(effect.RecordId!, 1, null)).Returns(new GrantedSkill(effect.RecordId!, "Effect", 1, "1", null, null, [],
		[
			"-0.35 Base Attack Speed",
			"+0% Total Damage",
			"?potionCooldownIncrease?: 5.000000, 10.000000",
			"?enemyScaleModifier?: 1.000000",
			"?skillDurationModifier?: 13.000000",
			"?someOtherVariable?: 2.000000",
			"?characterManaLimitReservePercentage?: 55.000000",
		]));

		var tip = this._builder.Build(Input()).Buffs.Sections.Single().Entries.Single().ToolTip;

		tip.Blocks[1].Lines.Should().Equal("-0.35 Base Attack Speed", "+5 to 10 s Potion Recharge", "+13% Skill Duration",
			"Reserves 55% of the energy");
	}

	[Fact]
	public void BuffToolTip_ToggleAnItemCastsWhenWorn_SaysItIsAlwaysOn()
	{
		this._sources = [Source("granted:a", BuffGroup.Active, true, ("characterStrength", 5)) with { OnEquip = true }];

		var tip = this._builder.Build(Input()).Buffs.Sections.Single().Entries.Single().ToolTip;

		tip.Blocks[0].Lines.Should().Contain(l => l.StartsWith("Cast by the item when worn", StringComparison.Ordinal));
	}
}
